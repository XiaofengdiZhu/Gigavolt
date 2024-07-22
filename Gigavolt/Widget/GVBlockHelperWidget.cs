using System;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVBlockHelperWidget : CanvasWidget {
        public enum DisplayMode { Recipes, Description, Duplicate }

        public readonly LabelWidget m_label = new() { FontScale = 0.7f, HorizontalAlignment = WidgetAlignment.Center };
        public readonly RectangleWidget m_icon = new() { Size = new Vector2(60f), FillColor = Color.White, OutlineThickness = 0f };
        public DisplayMode m_mode;

        public DisplayMode Mode {
            get => m_mode;
            set {
                m_mode = value;
                m_icon.Subtexture ??= value switch {
                    DisplayMode.Recipes => ContentManager.Get<Subtexture>("Textures/Gui/GVRecipaedia"),
                    DisplayMode.Description => ContentManager.Get<Subtexture>("Textures/Atlas/HelpTopicIcon"),
                    DisplayMode.Duplicate => ContentManager.Get<Subtexture>("Textures/Gui/GVCopy"),
                    _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
                };
                m_label.Text = value switch {
                    DisplayMode.Recipes => $"{m_recipesCount} {LanguageControl.Get("ContentWidgets", "RecipaediaScreen", "2")}", //"{0} 配方"
                    DisplayMode.Description => LanguageControl.Get("ContentWidgets", "RecipaediaScreen", "1"), //"描述"
                    DisplayMode.Duplicate => LanguageControl.Get("ContentWidgets", "MoreCommunityLinkDialog", "8"), //"复制"
                    _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
                };
                m_label.VerticalAlignment = value switch {
                    DisplayMode.Duplicate => WidgetAlignment.Near,
                    _ => WidgetAlignment.Far
                };
                switch (value) {
                    case DisplayMode.Recipes:
                        m_icon.Subtexture = ContentManager.Get<Subtexture>("Textures/Gui/GVRecipaedia");
                        m_icon.TextureLinearFilter = false;
                        m_label.Text = $"{m_recipesCount} {LanguageControl.Get("ContentWidgets", "RecipaediaScreen", "2")}"; //"{0} 配方"
                        m_label.VerticalAlignment = WidgetAlignment.Far;
                        break;
                    case DisplayMode.Description:
                        m_icon.Subtexture = ContentManager.Get<Subtexture>("Textures/Atlas/HelpTopicIcon");
                        m_label.Text = LanguageControl.Get("ContentWidgets", "RecipaediaScreen", "1"); //"描述"
                        m_label.VerticalAlignment = WidgetAlignment.Far;
                        break;
                    case DisplayMode.Duplicate:
                        m_icon.Subtexture = ContentManager.Get<Subtexture>("Textures/Gui/GVCopy");
                        m_icon.TextureLinearFilter = false;
                        m_icon.Size = new Vector2(32f);
                        SetPosition(m_icon, new Vector2(16f, 36f));
                        m_label.Text = LanguageControl.Get("ContentWidgets", "MoreCommunityLinkDialog", "8"); //"复制"
                        break;
                }
            }
        }

        public int m_recipesCount;

        public int RecipesCount {
            get => m_recipesCount;
            set {
                if (m_recipesCount != value) {
                    m_recipesCount = value;
                    m_label.Text = value switch {
                        > 0 => $"{m_recipesCount} {LanguageControl.Get("ContentWidgets", "RecipaediaScreen", "2")}", //"{0} 配方"
                        0 => LanguageControl.Get("RecipaediaScreen", "3"), //"没有配方"
                        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
                    };
                    if (value > 0) {
                        m_label.Text = $"{m_recipesCount} {LanguageControl.Get("ContentWidgets", "RecipaediaScreen", "2")}"; //"{0} 配方"
                        m_label.Color = Color.White;
                        m_icon.FillColor = Color.White;
                    }
                    else {
                        m_label.Text = LanguageControl.Get("RecipaediaScreen", "3"); //"没有配方"
                        m_label.Color = Color.LightGray;
                        m_icon.FillColor = Color.LightGray;
                    }
                }
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
            Vector2 center = Vector2.Transform(new Vector2(Size.X / 2f, Mode == DisplayMode.Duplicate ? Size.Y - Size.X / 2f : Size.X / 2f), GlobalTransform);
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
    }
}