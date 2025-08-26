using Engine;
using Engine.Graphics;
using Engine.Media;

namespace Game {
    public class GVVoltageRectangleWidget : Widget {
        public BitmapFont m_font;
        public string m_text = string.Empty;
        public Vector2? m_size;

        public Vector2 Size {
            get => m_size ?? Vector2.Zero;
            set => m_size = value;
        }

        public string Title { get; set; }

        public string Description { get; set; }

        public string Text {
            get => m_text;
            set => m_text = value;
        }

        public bool OverwriteMode { get; set; }

        public BitmapFont Font {
            get => m_font;
            set => m_font = value;
        }

        public float FontScale { get; set; }
        public Vector2 FontSpacing { get; set; }
        public bool VoltageCentered { get; set; }
        public Color Color { get; set; }
        public bool TextureLinearFilter { get; set; }
        public bool IsRightmost { get; set; }
        public bool IsBottom { get; set; }

        public GVVoltageRectangleWidget() {
            ClampToBounds = true;
            Color = Color.White;
            TextureLinearFilter = true;
            Font = ContentManager.Get<BitmapFont>("Fonts/Pericles");
            FontScale = 1f;
            Title = string.Empty;
            Description = string.Empty;
        }

        public override void Update() { }

        public override void MeasureOverride(Vector2 parentAvailableSize) {
            IsDrawRequired = true;
            if (m_size.HasValue) {
                DesiredSize = m_size.Value;
                return;
            }
            DesiredSize = Font.MeasureText(Text.Length == 0 ? " " : Text, new Vector2(FontScale), FontSpacing);
            DesiredSize += new Vector2(FontScale * Font.Scale, 0f);
        }

        public override void Draw(DrawContext dc) {
            Color color = Color * GlobalColorTransform;
            if (!string.IsNullOrEmpty(m_text)) {
                Vector2 position = new(
                    VoltageCentered ? ActualSize.X / 2f : ActualSize.X - 16f,
                    ActualSize.Y / 2f - FontScale * Font.Scale * Font.GlyphHeight / 2f
                );
                SamplerState samplerState = TextureLinearFilter ? SamplerState.LinearClamp : SamplerState.PointClamp;
                FontBatch2D fontBatch2D = dc.PrimitivesRenderer2D.FontBatch(Font, 1, DepthStencilState.None, null, null, samplerState);
                int count = fontBatch2D.TriangleVertices.Count;
                fontBatch2D.QueueText(
                    Text,
                    position,
                    0f,
                    color,
                    VoltageCentered ? TextAnchor.HorizontalCenter : TextAnchor.Right,
                    new Vector2(FontScale),
                    FontSpacing
                );
                fontBatch2D.TransformTriangles(GlobalTransform, count);
            }
            FlatBatch2D flatBatch2D = dc.PrimitivesRenderer2D.FlatBatch(1, DepthStencilState.None);
            int count2 = flatBatch2D.LineVertices.Count;
            flatBatch2D.QueueLine(new Vector2(0f, ActualSize.Y), Vector2.Zero, 0f, Color.White);
            flatBatch2D.QueueLine(Vector2.Zero, new Vector2(IsRightmost ? ActualSize.X - 1 : ActualSize.X, 0f), 0f, Color.White);
            if (IsRightmost) {
                flatBatch2D.QueueLine(new Vector2(ActualSize.X - 1, 0f), new Vector2(ActualSize.X - 1, ActualSize.Y), 0f, Color.White);
            }
            if (IsBottom) {
                flatBatch2D.QueueLine(
                    IsRightmost ? new Vector2(ActualSize.X - 1, ActualSize.Y) : ActualSize,
                    new Vector2(0f, ActualSize.Y),
                    0f,
                    Color.White
                );
            }
            //flatBatch2D.QueueQuad(v - new Vector2(0f, Font.GlyphHeight / 2f * FontScale * Font.Scale), v + new Vector2(1f, Font.GlyphHeight / 2f * FontScale * Font.Scale), 0f, color);
            flatBatch2D.TransformLines(GlobalTransform, count2);
        }
    }
}