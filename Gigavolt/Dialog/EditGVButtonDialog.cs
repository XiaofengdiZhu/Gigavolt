using System;
using System.Globalization;
using System.Xml.Linq;

namespace Game {
    public class EditGVButtonDialog : Dialog {
        public readonly Action<uint> m_handler;

        public readonly ButtonWidget m_okButton;
        public readonly ButtonWidget m_cancelButton;

        public readonly BevelledButtonWidget m_gigaVoltageLevelButton;
        public readonly TextBoxWidget m_durationTextBox;

        public readonly GVButtonData m_blockData;

        public EditGVButtonDialog(GVButtonData blockData, Action<uint> handler) {
            XElement node = ContentManager.Get<XElement>("Dialogs/EditGVButtonDialog");
            LoadContents(this, node);
            m_okButton = Children.Find<ButtonWidget>("EditGVButtonDialog.OK");
            m_cancelButton = Children.Find<ButtonWidget>("EditGVButtonDialog.Cancel");
            m_gigaVoltageLevelButton = Children.Find<BevelledButtonWidget>("EditGVButtonDialog.GigaVoltageLevelButton");
            m_gigaVoltageLevelButton.Text = blockData.GigaVoltageLevel.ToString("X", null);
            m_durationTextBox = Children.Find<TextBoxWidget>("EditGVButtonDialog.Duration");
            m_durationTextBox.Text = blockData.Duration.ToString();
            m_handler = handler;
            m_blockData = blockData;
        }

        public override void Update() {
            if (m_gigaVoltageLevelButton.IsClicked) {
                DialogsManager.ShowDialog(this, new EditGVUintDialog(m_blockData.GigaVoltageLevel, newVoltage => m_gigaVoltageLevelButton.Text = newVoltage.ToString("X", null)));
            }
            if (m_okButton.IsClicked) {
                if (uint.TryParse(m_gigaVoltageLevelButton.Text, NumberStyles.HexNumber, null, out uint voltage)
                    && int.TryParse(m_durationTextBox.Text, out int duration)
                    && duration > 1) {
                    m_blockData.GigaVoltageLevel = voltage;
                    m_blockData.Duration = duration;
                    m_blockData.SaveString();
                    Dismiss(true, voltage);
                }
                else {
                    if (m_gigaVoltageLevelButton.Text.Length == 0
                        || m_durationTextBox.Text.Length == 0) {
                        Dismiss(false);
                    }
                    DialogsManager.ShowDialog(
                        null,
                        new MessageDialog(
                            "发生错误",
                            "输入的数字不符合要求",
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
        }

        public void Dismiss(bool result, uint voltage = 0u) {
            DialogsManager.HideDialog(this);
            if (m_handler != null && result) {
                m_handler(voltage);
            }
        }
    }
}