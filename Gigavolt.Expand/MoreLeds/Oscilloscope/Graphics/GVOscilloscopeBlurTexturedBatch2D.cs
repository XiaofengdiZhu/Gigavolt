namespace Engine.Graphics {
    public class GVOscilloscopeBlurTexturedBatch2D : TexturedBatch2D {
        public static GVOscilloscopeBlurShader1 GVOscilloscopeBlurShader1 = new();
        public static GVOscilloscopeBlurShader2 GVOscilloscopeBlurShader2 = new();

        public RenderTarget2D FlushBlur() {
            RenderTarget2D originRenderTarget = Display.RenderTarget;
            RenderTarget2D tempRenderTarget = new(
                Texture.Width,
                Texture.Height,
                1,
                ColorFormat.Rgba8888,
                DepthFormat.None
            );
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
            RenderTarget2D blurredTexture = new(
                Texture.Width,
                Texture.Height,
                1,
                ColorFormat.Rgba8888,
                DepthFormat.None
            );
            Display.RenderTarget = blurredTexture;
            Display.Clear(Color.Black);
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
            tempRenderTarget.Dispose();
            Clear();
            Display.RenderTarget = originRenderTarget;
            return blurredTexture;
        }
    }
}