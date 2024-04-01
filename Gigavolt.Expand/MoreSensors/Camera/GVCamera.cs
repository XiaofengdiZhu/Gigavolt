using Engine;

namespace Game {
    public class GVCamera : BasePerspectiveCamera {
        public float m_viewAngel = MathUtils.PI / 2;
        public Point2 m_viewSize = new(512, 512);
        public override bool UsesMovementControls => false;

        public override bool IsEntityControlEnabled => false;

        public GVCamera(GameWidget gameWidget) : base(gameWidget) { }

        public void Activate() { }

        public void Activate(Vector3 viewPosition, Vector3 viewDirection, Vector3 viewUp) {
            SetupPerspectiveCamera(viewPosition, viewDirection, viewUp);
        }

        public override void Activate(Camera previousCamera) { }

        public override void Update(float dt) { }

        public static Matrix GVCalculateBaseProjectionMatrix(Vector2 wh, float viewAngle) {
            float num3 = wh.X / wh.Y;
            float num4 = MathUtils.Min(viewAngle * num3, viewAngle);
            float num5 = num4 * num3;
            return Matrix.CreatePerspectiveFieldOfView(num4, num3, 0.1f, 2048f);
        }

        public override Matrix ProjectionMatrix {
            get {
                if (m_projectionMatrix == null) {
                    m_projectionMatrix = GVCalculateBaseProjectionMatrix(new Vector2(m_viewSize), m_viewAngel);
                    m_projectionMatrix *= CreateScaleTranslation(0.5f * m_viewSize.X, -0.5f * m_viewSize.Y, m_viewSize.X / 2f, m_viewSize.Y / 2f) * Matrix.Identity * CreateScaleTranslation(2f / m_viewSize.X, -2f / m_viewSize.Y, -1f, 1f);
                }
                return m_projectionMatrix.Value;
            }
        }

        public override Matrix ScreenProjectionMatrix {
            get {
                if (m_screenProjectionMatrix == null) {
                    Point2 size = Window.Size;
                    m_screenProjectionMatrix = GVCalculateBaseProjectionMatrix(new Vector2(m_viewSize), m_viewAngel) * CreateScaleTranslation(0.5f * m_viewSize.X, -0.5f * m_viewSize.Y, m_viewSize.X / 2f, m_viewSize.Y / 2f) * Matrix.Identity * CreateScaleTranslation(2f / m_viewSize.X, -2f / m_viewSize.Y, -1f, 1f);
                }
                return m_screenProjectionMatrix.Value;
            }
        }

        public override Vector2 ViewportSize {
            get {
                if (m_viewportSize == null) {
                    m_viewportSize = new Vector2(m_viewSize);
                }
                return m_viewportSize.Value;
            }
        }

        public static Matrix CreateScaleTranslation(float sx, float sy, float tx, float ty) => new(
            sx,
            0.0f,
            0.0f,
            0.0f,
            0.0f,
            sy,
            0.0f,
            0.0f,
            0.0f,
            0.0f,
            1f,
            0.0f,
            tx,
            ty,
            0.0f,
            1f
        );
    }
}