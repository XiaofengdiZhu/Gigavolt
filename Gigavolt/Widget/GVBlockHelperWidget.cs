using System;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVBlockHelperWidget : CanvasWidget {
        public enum DisplayMode { Recipes, Description, Duplicate, Cancel }

        public readonly LabelWidget m_label = new() { FontScale = 0.7f, Color = Color.LightGray, HorizontalAlignment = WidgetAlignment.Center };
        public readonly RectangleWidget m_icon = new() { FillColor = Color.LightGray, OutlineThickness = 0f };
        public readonly DisplayMode m_mode;

        public DisplayMode Mode {
            get => m_mode;
            init {
                m_mode = value;
                switch (value) {
                    case DisplayMode.Recipes:
                        m_icon.Subtexture = ContentManager.Get<Subtexture>("Textures/Gui/GVRecipaedia");
                        m_icon.TextureLinearFilter = false;
                        m_icon.Size = new Vector2(56f);
                        SetPosition(m_icon, new Vector2(4f));
                        m_label.Text = $"{m_recipesCount} {LanguageControl.Get("ContentWidgets", "RecipaediaScreen", "2")}"; //"{0} 配方"
                        m_label.VerticalAlignment = WidgetAlignment.Far;
                        break;
                    case DisplayMode.Description:
                        m_icon.Subtexture = ContentManager.Get<Subtexture>("Textures/Atlas/HelpTopicIcon");
                        m_icon.Size = new Vector2(60f);
                        SetPosition(m_icon, new Vector2(2f));
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
                    case DisplayMode.Cancel:
                        m_icon.Subtexture = ContentManager.Get<Subtexture>("Textures/Atlas/Plus");
                        m_icon.Size = new Vector2(48f);
                        SetPosition(m_icon, new Vector2(2f));
                        m_icon.RenderTransform *= Matrix.CreateRotationZ(MathF.PI / 4f) * Matrix.CreateTranslation(24f, -8.485f, 0f);
                        SetPosition(m_icon, new Vector2(8f, 28f));
                        m_label.Text = LanguageControl.Get("Usual", "cancel"); //"取消"
                        break;
                }
            }
        }

        public int m_recipesCount = -1;

        public int RecipesCount {
            get => m_recipesCount;
            set {
                if (Mode == DisplayMode.Recipes
                    && m_recipesCount != value) {
                    m_recipesCount = value;
                    m_label.Text = value switch {
                        > 0 => $"{m_recipesCount} {LanguageControl.Get("ContentWidgets", "RecipaediaScreen", "2")}", //"{0} 配方"
                        0 => LanguageControl.Get("RecipaediaScreen", "3"), //"没有配方"
                        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
                    };
                    if (value > 0) {
                        m_label.Text = $"{m_recipesCount} {LanguageControl.Get("ContentWidgets", "RecipaediaScreen", "2")}"; //"{0} 配方"
                        m_label.Color = Color.LightGray;
                        m_icon.FillColor = Color.LightGray;
                    }
                    else {
                        m_label.Text = LanguageControl.Get("RecipaediaScreen", "3"); //"没有配方"
                        m_label.Color = Color.Gray;
                        m_icon.FillColor = Color.Gray;
                    }
                }
            }
        }

        public bool m_hasFocus;

        public bool HasFocus {
            get => m_hasFocus;
            set {
                m_hasFocus = value;
                if (Mode != DisplayMode.Recipes
                    || RecipesCount > 0) {
                    if (value) {
                        m_label.Color = Color.White;
                        m_icon.FillColor = Color.White;
                    }
                    else {
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
        }

        public override void Draw(DrawContext dc) {
            Vector2 center = Vector2.Transform(new Vector2(Size.X / 2f, Mode is DisplayMode.Duplicate or DisplayMode.Cancel ? Size.Y - Size.X / 2f : Size.X / 2f), GlobalTransform);
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