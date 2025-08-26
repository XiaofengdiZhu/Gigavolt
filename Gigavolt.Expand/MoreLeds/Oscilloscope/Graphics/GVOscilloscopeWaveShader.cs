using Game;

namespace Engine.Graphics {
    public class GVOscilloscopeWaveShader : Shader {
        public readonly ShaderParameter m_worldViewProjectionMatrixParameter;

        public readonly ShaderTransforms Transforms;

        public GVOscilloscopeWaveShader() : base(
            ShaderCodeManager.GetFast("Shaders/GVOscilloscopeWave.vsh"),
            ShaderCodeManager.GetFast("Shaders/GVOscilloscopeWave.psh")
        ) {
            m_worldViewProjectionMatrixParameter = GetParameter("u_worldViewProjectionMatrix", true);
            Transforms = new ShaderTransforms(1);
        }

        public override void PrepareForDrawingOverride() {
            Transforms.UpdateMatrices(1, false, false, true);
            m_worldViewProjectionMatrixParameter.SetValue(Transforms.WorldViewProjection, 1);
        }
    }
}