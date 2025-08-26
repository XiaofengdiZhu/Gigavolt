using Game;

namespace Engine.Graphics {
    public class GVOscilloscopeBlurShader2 : Shader {
        public readonly ShaderParameter m_worldViewProjectionMatrixParameter;

        public readonly ShaderParameter m_horizontal;
        public readonly ShaderParameter m_textureSizeParameter;
        public readonly ShaderParameter m_textureParameter;
        public readonly ShaderParameter m_texture2Parameter;
        public readonly ShaderParameter m_samplerStateParameter;
        public readonly ShaderParameter m_samplerState2Parameter;

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

        public Texture2D Texture2 {
            set => m_texture2Parameter.SetValue(value);
        }

        public SamplerState SamplerState {
            set => m_samplerStateParameter.SetValue(value);
        }

        public SamplerState SamplerState2 {
            set => m_samplerState2Parameter.SetValue(value);
        }

        public GVOscilloscopeBlurShader2() : base(
            ShaderCodeManager.GetFast("Shaders/GVOscilloscopeBlur.vsh"),
            ShaderCodeManager.GetFast("Shaders/GVOscilloscopeBlur2.psh")
        ) {
            m_worldViewProjectionMatrixParameter = GetParameter("u_worldViewProjectionMatrix", true);
            m_horizontal = GetParameter("u_horizontal", true);
            m_textureSizeParameter = GetParameter("u_textureSize", true);
            m_textureParameter = GetParameter("u_texture", true);
            m_texture2Parameter = GetParameter("u_texture2", true);
            m_samplerStateParameter = GetParameter("u_samplerState", true);
            m_samplerState2Parameter = GetParameter("u_samplerState2", true);
            Transforms = new ShaderTransforms(1);
        }

        public override void PrepareForDrawingOverride() {
            Transforms.UpdateMatrices(1, false, false, true);
            m_worldViewProjectionMatrixParameter.SetValue(Transforms.WorldViewProjection, 1);
        }
    }
}