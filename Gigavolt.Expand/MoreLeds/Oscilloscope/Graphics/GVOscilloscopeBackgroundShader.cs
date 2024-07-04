using Game;

namespace Engine.Graphics {
    public class GVOscilloscopeBackgroundShader : Shader {
        public readonly ShaderParameter m_worldViewProjectionMatrixParameter;
        public readonly ShaderParameter m_horizontalSpacing;
        public readonly ShaderParameter m_verticalSpacing;
        public readonly ShaderParameter m_dashLength;
        public readonly ShaderParameter m_dashAndGapLength;

        public readonly ShaderTransforms Transforms;

        public float HorizontalSpacing {
            set => m_horizontalSpacing.SetValue(value);
        }

        public float VerticalSpacing {
            set => m_verticalSpacing.SetValue(value);
        }

        public float DashLength {
            set => m_dashLength.SetValue(value);
        }

        public float DashAndGapLength {
            set => m_dashAndGapLength.SetValue(value);
        }

        public GVOscilloscopeBackgroundShader() : base(ShaderCodeManager.GetFast("Shaders/GVOscilloscopeBackground.vsh"), ShaderCodeManager.GetFast("Shaders/GVOscilloscopeBackground.psh")) {
            m_worldViewProjectionMatrixParameter = GetParameter("u_worldViewProjectionMatrix", true);
            m_horizontalSpacing = GetParameter("u_horizontalSpacing", true);
            m_verticalSpacing = GetParameter("u_verticalSpacing", true);
            m_dashLength = GetParameter("u_dashLength", true);
            m_dashAndGapLength = GetParameter("u_dashAndGapLength", true);
            Transforms = new ShaderTransforms(1);
        }

        public override void PrepareForDrawingOverride() {
            Transforms.UpdateMatrices(1, false, false, true);
            m_worldViewProjectionMatrixParameter.SetValue(Transforms.WorldViewProjection, 1);
        }
    }
}