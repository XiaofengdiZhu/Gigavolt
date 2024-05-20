using Game;

namespace Engine.Graphics {
    public class GVOscilloscopeBlurShader1 : Shader {
        public ShaderParameter m_worldViewProjectionMatrixParameter;

        public ShaderParameter m_horizontal;
        public ShaderParameter m_textureSizeParameter;
        public ShaderParameter m_textureParameter;
        public ShaderParameter m_samplerStateParameter;

        public readonly ShaderTransforms Transforms;

        public bool Horizontal {
            set => m_horizontal.SetValue(value ? 1.0f : 0.0f);
        }

        public Vector2 TextureSize {
            set => m_textureSizeParameter.SetValue(value);
        }

        public Texture2D Texture {
            set => m_textureParameter.SetValue(value);
        }

        public SamplerState SamplerState {
            set => m_samplerStateParameter.SetValue(value);
        }

        public GVOscilloscopeBlurShader1() : base(ShaderCodeManager.GetFast("Shaders/GVOscilloscopeBlur.vsh"), ShaderCodeManager.GetFast("Shaders/GVOscilloscopeBlur1.psh")) {
            m_worldViewProjectionMatrixParameter = GetParameter("u_worldViewProjectionMatrix", true);
            m_horizontal = GetParameter("u_horizontal", true);
            m_textureSizeParameter = GetParameter("u_textureSize", true);
            m_textureParameter = GetParameter("u_texture", true);
            m_samplerStateParameter = GetParameter("u_samplerState", true);
            Transforms = new ShaderTransforms(1);
        }

        public override void PrepareForDrawingOverride() {
            Transforms.UpdateMatrices(1, false, false, true);
            m_worldViewProjectionMatrixParameter.SetValue(Transforms.WorldViewProjection, 1);
        }
    }
}