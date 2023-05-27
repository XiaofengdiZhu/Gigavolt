using System;
using System.Globalization;
using System.Xml.Linq;

namespace Game {
    public class EditGigaVoltageLevelDialog : Dialog {
        public readonly Action<uint> m_handler;

        public readonly ButtonWidget m_okButton;

        public readonly ButtonWidget m_cancelButton;

        public readonly TextBoxWidget m_voltageLevelTextBox;

        public readonly GigaVoltageLevelData m_blockData;

        public EditGigaVoltageLevelDialog(GigaVoltageLevelData blockData, Action<uint> handler) {
            XElement node = ContentManager.Get<XElement>("Dialogs/EditGigaVoltageLevelDialog");
            LoadContents(this, node);
            m_okButton = Children.Find<ButtonWidget>("EditGigaVoltageLevelDialog.OK");
            m_cancelButton = Children.Find<ButtonWidget>("EditGigaVoltageLevelDialog.Cancel");
            m_voltageLevelTextBox = Children.Find<TextBoxWidget>("EditGigaVoltageLevelDialog.GigaVoltageLevel");
            m_voltageLevelTextBox.Text = blockData.Data.ToString("X", null);
            m_handler = handler;
            m_blockData = blockData;
        }

        public override void Update() {
            if (m_okButton.IsClicked) {
                if (uint.TryParse(m_voltageLevelTextBox.Text, NumberStyles.HexNumber, null, out uint voltage)) {
                    m_blockData.Data = voltage;
                    m_blockData.SaveString();
                    Dismiss(true, voltage);
                }
                else {
                    DialogsManager.ShowDialog(
                        null,
                        new MessageDialog(
                            "发生错误",
                            "不能转换为自然数",
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