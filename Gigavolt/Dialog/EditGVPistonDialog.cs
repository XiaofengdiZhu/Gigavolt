using System;
using System.Xml.Linq;
using Engine;

// ReSharper disable RedundantExplicitArraySize

namespace Game {
    public class EditGVPistonDialog : Dialog {
        public readonly LabelWidget m_title;
        public readonly TextBoxWidget m_maxExtensionWidget;
        public readonly TextBoxWidget m_pullCountWidget;
        public readonly CheckboxWidget m_transparentCheckBoxWidget;
        public readonly LabelWidget m_label2;
        public readonly SliderWidget m_slider3;
        public readonly ButtonWidget m_okButton;
        public readonly ButtonWidget m_cancelButton;

        public readonly Action m_handler;
        public readonly GVPistonData m_pistonData;

        public int m_speed;

        public readonly string m_languageType;

        public EditGVPistonDialog(GVPistonMode mode, GVPistonData pistonData, Action handler) {
            try {
                XElement node = ContentManager.Get<XElement>("Dialogs/EditGVPistonDialog");
                LoadContents(this, node);
                m_title = Children.Find<LabelWidget>("EditGVPistonDialog.Title");
                m_maxExtensionWidget = Children.Find<TextBoxWidget>("EditGVPistonDialog.MaxExtension");
                m_label2 = Children.Find<LabelWidget>("EditGVPistonDialog.Label2");
                m_pullCountWidget = Children.Find<TextBoxWidget>("EditGVPistonDialog.PullCount");
                m_transparentCheckBoxWidget = Children.Find<CheckboxWidget>("EditGVPistonDialog.Transparent");
                m_slider3 = Children.Find<SliderWidget>("EditGVPistonDialog.Slider3");
                m_okButton = Children.Find<ButtonWidget>("EditGVPistonDialog.OK");
                m_cancelButton = Children.Find<ButtonWidget>("EditGVPistonDialog.Cancel");
                m_handler = handler;
                m_pistonData = pistonData;
                m_speed = m_pistonData.Speed;
                m_languageType = ModsManager.Configs.TryGetValue("Language", out string config) ? config : "zh-CN";
                m_title.Text = GVPistonBlock.Mode2Name(mode);
                m_maxExtensionWidget.Text = (pistonData.MaxExtension + 1).ToString();
                m_pullCountWidget.Text = (pistonData.PullCount + 1).ToString();
                m_transparentCheckBoxWidget.IsChecked = pistonData.Transparent;
                m_slider3.Granularity = 1f;
                m_slider3.MinValue = 0f;
                m_slider3.MaxValue = 6f;
                m_label2.Text = mode == GVPistonMode.Pushing ? LanguageControl.Get(GetType().Name, 3) : LanguageControl.Get(GetType().Name, 2);
                UpdateControls();
            }
            catch (Exception e) {
                Log.Error(e);
            }
        }

        public override void Update() {
            if (m_slider3.IsSliding) {
                m_speed = (int)m_slider3.Value;
            }
            if (m_okButton.IsClicked) {
                if (int.TryParse(m_maxExtensionWidget.Text, out int m)
                    && m >= 0) {
                    m_pistonData.MaxExtension = m - 1;
                    if (int.TryParse(m_pullCountWidget.Text, out int p)
                        && p >= 0) {
                        m_pistonData.PullCount = p - 1;
                        m_pistonData.Transparent = m_transparentCheckBoxWidget.IsChecked;
                        m_pistonData.Speed = m_speed;
                        m_pistonData.SaveString();
                        Dismiss(true);
                    }
                    else {
                        DialogsManager.ShowDialog(
                            null,
                            new MessageDialog(
                                "发生错误",
                                "最大推拉数不能转换为自然数",
                                "OK",
                                null,
                                null
                            )
                        );
                    }
                }
                else {
                    DialogsManager.ShowDialog(
                        null,
                        new MessageDialog(
                            "发生错误",
                            "最大延伸数不能转换为自然数",
                            "OK",
                            null,
                            null
                        )
                    );
                }
            }
            if (Input.Cancel
                || m_cancelButton.IsClicked) {
                Dismiss(false);
            }
            UpdateControls();
        }

        public void UpdateControls() {
            m_slider3.Value = m_speed;
            m_slider3.Text = LanguageControl.Get(GetType().Name + "Speed", m_speed);
        }

        public void Dismiss(bool result) {
            DialogsManager.HideDialog(this);
            if (m_handler != null && result) {
                m_handler();
            }
        }
    }
}