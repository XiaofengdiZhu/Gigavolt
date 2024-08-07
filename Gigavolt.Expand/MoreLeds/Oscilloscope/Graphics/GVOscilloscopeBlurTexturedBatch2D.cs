namespace Engine.Graphics {
    public class GVOscilloscopeBlurTexturedBatch2D : TexturedBatch2D {
        public static readonly GVOscilloscopeBlurShader1 GVOscilloscopeBlurShader1 = new();
        public static readonly GVOscilloscopeBlurShader2 GVOscilloscopeBlurShader2 = new();

        public void FlushBlur(RenderTarget2D tempRenderTarget) {
            RenderTarget2D originRenderTarget = Display.RenderTarget;
            Display.RenderTarget = tempRenderTarget;
            Display.Clear(Color.Black);
            Display.DepthStencilState = DepthStencilState;
            Display.RasterizerState = RasterizerState;
            Display.BlendState = BlendState;
            GVOscilloscopeBlurShader1.TextureSize = new Vector2(Texture.Width, Texture.Height);
            GVOscilloscopeBlurShader1.Texture = Texture;
            GVOscilloscopeBlurShader1.SamplerState = SamplerState;
            Matrix matrix = PrimitivesRenderer2D.ViewportMatrix();
            GVOscilloscopeBlurShader1.Transforms.World[0] = matrix;
            Display.DrawUserIndexed(
                PrimitiveType.TriangleList,
                GVOscilloscopeBlurShader1,
                VertexPositionColorTexture.VertexDeclaration,
                TriangleVertices.Array,
                0,
                TriangleVertices.Count,
                TriangleIndices.Array,
                0,
                TriangleIndices.Count
            );
            Display.RenderTarget = originRenderTarget;
            GVOscilloscopeBlurShader2.TextureSize = new Vector2(Texture.Width, Texture.Height);
            GVOscilloscopeBlurShader2.Texture = Texture;
            GVOscilloscopeBlurShader2.Texture2 = tempRenderTarget;
            GVOscilloscopeBlurShader2.SamplerState = SamplerState;
            GVOscilloscopeBlurShader2.SamplerState2 = SamplerState;
            GVOscilloscopeBlurShader2.Transforms.World[0] = matrix;
            Display.DrawUserIndexed(
                PrimitiveType.TriangleList,
                GVOscilloscopeBlurShader2,
                VertexPositionColorTexture.VertexDeclaration,
                TriangleVertices.Array,
                0,
                TriangleVertices.Count,
                TriangleIndices.Array,
                0,
                TriangleIndices.Count
            );
            Clear();
        }
    }
}