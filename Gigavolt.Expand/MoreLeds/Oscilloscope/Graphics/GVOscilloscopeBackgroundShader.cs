using Game;

namespace Engine.Graphics {
    public class GVOscilloscopeBackgroundShader : Shader {
        public ShaderParameter m_worldViewProjectionMatrixParameter;
        public ShaderParameter m_leftOffset;
        public ShaderParameter m_upOffset;
        public ShaderParameter m_horizontalSpacing;
        public ShaderParameter m_verticalSpacing;
        public ShaderParameter m_dashLength;
        public ShaderParameter m_dashAndGapLength;

        public readonly ShaderTransforms Transforms;

        public float LeftOffset {
            set => m_leftOffset.SetValue(value);
        }

        public float UpOffset {
            set => m_upOffset.SetValue(value);
        }

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
            m_leftOffset = GetParameter("u_leftOffset", true);
            m_upOffset = GetParameter("u_upOffset", true);
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