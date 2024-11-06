using Engine;

namespace Game {
    public class GVBlockIconWidget : CanvasWidget {
        public static readonly Color LostFocusColorTransform = new(178, 178, 178);

        public readonly LabelWidget NameLabel = new() { FontScale = 0.7f, HorizontalAlignment = WidgetAlignment.Center, VerticalAlignment = WidgetAlignment.Near, IsVisible = false };
        public readonly BlockIconWidget Icon = new() { IsDrawRequired = true, VerticalAlignment = WidgetAlignment.Center, HorizontalAlignment = WidgetAlignment.Center, ColorTransform = LostFocusColorTransform };
        public float FullHeight => Size.Y - NameLabel.Margin.Y;
        public float NameLabelMarginY => NameLabel.Margin.Y;

        public int Value {
            get => Icon.Value;
            set {
                if (value != Icon.Value) {
                    Icon.Value = value;
                    string text = BlocksManager.Blocks[Terrain.ExtractContents(value)].GetDisplayName(Icon.DrawBlockEnvironmentData.SubsystemTerrain, value);
                    if (ModsManager.Configs["Language"]?.StartsWith("zh") ?? true) {
                        if (text.Length > 4) {
                            text = text.Insert(text[3] == 'G' && text[4] == 'V' ? 5 : 4, "\n");
                        }
                    }
                    else {
                        char[] chars = text.ToCharArray();
                        int spaceCount = 0;
                        for (int i = 0; i < chars.Length; i++) {
                            if (text[i] == ' '
                                && ++spaceCount == 2) {
                                chars[i] = '\n';
                                break;
                            }
                        }
                        text = new string(chars);
                    }
                    NameLabel.Text = text;
                    NameLabel.Margin = new Vector2(0f, -NameLabel.Font.MeasureText(text, new Vector2(NameLabel.FontScale), NameLabel.FontSpacing).Y);
                }
            }
        }

        public float FontScale {
            get => NameLabel.FontScale;
            set {
                if (value != NameLabel.FontScale) {
                    NameLabel.FontScale = value;
                    NameLabel.Margin = new Vector2(0f, -NameLabel.Font.MeasureText(NameLabel.Text, new Vector2(value), NameLabel.FontSpacing).Y);
                }
            }
        }

        public SubsystemTerrain SubsystemTerrain {
            get => Icon.DrawBlockEnvironmentData.SubsystemTerrain;
            set => Icon.DrawBlockEnvironmentData.SubsystemTerrain = value;
        }

        public bool m_alwaysInFocus;

        public bool AlwaysInFocus {
            get => m_alwaysInFocus;
            init {
                m_alwaysInFocus = value;
                NameLabel.IsVisible = value;
                Icon.ColorTransform = Color.White;
            }
        }

        public bool m_hasFocus;

        public bool HasFocus {
            get => AlwaysInFocus || m_hasFocus;
            set {
                if (AlwaysInFocus) {
                    return;
                }
                m_hasFocus = value;
                NameLabel.IsVisible = value;
                Icon.ColorTransform = value ? Color.White : LostFocusColorTransform;
            }
        }

        public GVBlockIconWidget() {
            FontScale = 0.7f;
            AddChildren(Icon);
            AddChildren(NameLabel);
        }
    }
}