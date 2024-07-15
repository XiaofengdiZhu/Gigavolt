using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVBlockHelperWidget : CanvasWidget {
        public readonly LabelWidget m_label = new() { FontScale = 0.7f, HorizontalAlignment = WidgetAlignment.Center, VerticalAlignment = WidgetAlignment.Far, Text = LanguageControl.Get("RecipaediaScreen", "3") };
        public readonly RectangleWidget m_icon = new() { Size = new Vector2(60f), FillColor = Color.White, OutlineThickness = 0f };
        public int m_recipaediaCount;

        public int RecipaediaCount {
            get => m_recipaediaCount;
            set {
                if (m_recipaediaCount != value) {
                    m_recipaediaCount = value;
                    m_label.Text = value switch {
                        > 0 => $"{m_recipaediaCount} {LanguageControl.Get("ContentWidgets", "RecipaediaScreen", "2")}", //"{0} 配方"
                        0 => LanguageControl.Get("RecipaediaScreen", "3"), //"没有配方"
                        _ => LanguageControl.Get("ContentWidgets", "RecipaediaScreen", "1") //"描述"
                    };
                }
                m_icon.Subtexture ??= value switch {
                    >= 0 => ContentManager.Get<Subtexture>("Textures/Gui/GVRecipaedia"),
                    _ => ContentManager.Get<Subtexture>("Textures/Atlas/HelpTopicIcon")
                };
            }
        }

        public GVBlockHelperWidget() {
            Size = new Vector2(64f, 84f);
            IsDrawRequired = true;
            AddChildren(m_icon);
            AddChildren(m_label);
            SetPosition(m_icon, new Vector2(2f));
        }

        public override void Draw(DrawContext dc) {
            Vector2 center = Vector2.Transform(new Vector2(Size.X / 2f), GlobalTransform);
            Color color1 = new Color(0, 0, 0, 128) * GlobalColorTransform;
            Color color2 = new Color(0, 0, 0, 96) * GlobalColorTransform;
            Color color3 = new Color(0, 0, 0, 64) * GlobalColorTransform;
            FlatBatch2D flatBatch2D = dc.PrimitivesRenderer2D.FlatBatch(100);
            float radius = Size.X / 2f * GlobalTransform.Right.Length();
            flatBatch2D.QueueEllipse(
                center,
                new Vector2(radius),
                0f,
                color1,
                64
            );
            flatBatch2D.QueueEllipse(
                center,
                new Vector2(radius - 0.5f),
                0f,
                color2,
                64
            );
            flatBatch2D.QueueEllipse(
                center,
                new Vector2(radius + 0.5f),
                0f,
                color3,
                64
            );
            flatBatch2D.QueueDisc(
                center,
                new Vector2(radius),
                0f,
                color3,
                64
            );
            base.Draw(dc);
        }

        public void Action(int value) {
            value = Terrain.ReplaceLight(value, 0);
            switch (RecipaediaCount) {
                case > 0:
                    ScreensManager.SwitchScreen("RecipaediaRecipes", value);
                    return;
                case -1:
                    ScreensManager.SwitchScreen("RecipaediaDescription", [value, new List<int> { value }]);
                    return;
            }
        }
    }
}