using Engine.Media;

namespace Engine.Graphics {
    public class GVPrimitivesRenderer2D : BasePrimitivesRenderer<GVFlatBatch2D, TexturedBatch2D, FontBatch2D> {
        public static Matrix ViewportMatrix() {
            Viewport viewport = Display.Viewport;
            float num = 1f / viewport.Width;
            float num2 = 1f / viewport.Height;
            return new Matrix(
                2f * num,
                0f,
                0f,
                0f,
                0f,
                -2f * num2,
                0f,
                0f,
                0f,
                0f,
                1f,
                0f,
                -1f,
                1f,
                0f,
                1f
            );
        }

        public GVFlatBatch2D FlatBatch(int layer = 0, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, BlendState blendState = null) {
            depthStencilState = depthStencilState ?? DepthStencilState.None;
            rasterizerState = rasterizerState ?? RasterizerState.CullNoneScissor;
            blendState = blendState ?? BlendState.AlphaBlend;
            return FindFlatBatch(layer, depthStencilState, rasterizerState, blendState);
        }

        public TexturedBatch2D TexturedBatch(Texture2D texture, bool useAlphaTest = false, int layer = 0, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, BlendState blendState = null, SamplerState samplerState = null) {
            depthStencilState = depthStencilState ?? DepthStencilState.None;
            rasterizerState = rasterizerState ?? RasterizerState.CullNoneScissor;
            blendState = blendState ?? BlendState.AlphaBlend;
            samplerState = samplerState ?? SamplerState.LinearClamp;
            return FindTexturedBatch(
                texture,
                useAlphaTest,
                layer,
                depthStencilState,
                rasterizerState,
                blendState,
                samplerState
            );
        }

        public FontBatch2D FontBatch(BitmapFont font = null, int layer = 0, DepthStencilState depthStencilState = null, RasterizerState rasterizerState = null, BlendState blendState = null, SamplerState samplerState = null) {
            font = font ?? BitmapFont.DebugFont;
            depthStencilState = depthStencilState ?? DepthStencilState.None;
            rasterizerState = rasterizerState ?? RasterizerState.CullNoneScissor;
            blendState = blendState ?? BlendState.AlphaBlend;
            samplerState = samplerState ?? SamplerState.LinearClamp;
            return FindFontBatch(
                font,
                layer,
                depthStencilState,
                rasterizerState,
                blendState,
                samplerState
            );
        }

        public void Flush(bool clearAfterFlush = true, int maxLayer = int.MaxValue) {
            Flush(ViewportMatrix(), clearAfterFlush, maxLayer);
        }
    }
}