using System;
using System.Globalization;
using System.Xml.Linq;
using Engine;

namespace Game {
    public class EditGVCounterDialog : Dialog {
        public readonly Action<uint> m_handler;

        public readonly ButtonWidget m_okButton;

        public readonly ButtonWidget m_cancelButton;

        public readonly TextBoxWidget m_overflowTextBox;
        public readonly TextBoxWidget m_currentTextBox;
        public readonly LabelWidget m_currentLabel;

        public readonly GigaVoltageLevelData m_blockData;

        public EditGVCounterDialog(GigaVoltageLevelData blockData, CounterGVElectricElement element, Action<uint> handler) {
            XElement node = ContentManager.Get<XElement>("Dialogs/EditGVCounterDialog");
            LoadContents(this, node);
            m_okButton = Children.Find<ButtonWidget>("EditGVCounterDialog.OK");
            m_cancelButton = Children.Find<ButtonWidget>("EditGVCounterDialog.Cancel");
            m_overflowTextBox = Children.Find<TextBoxWidget>("EditGVCounterDialog.Overflow");
            m_overflowTextBox.Text = blockData.Data.ToString("X");
            m_currentTextBox = Children.Find<TextBoxWidget>("EditGVCounterDialog.Current");
            m_currentLabel = Children.Find<LabelWidget>("EditGVCounterDialog.CurrentLabel");
            if (element == null) {
                m_currentTextBox.IsEnabled = false;
                m_currentTextBox.Text = "0";
                m_currentLabel.Color = Color.Gray;
            }
            else {
                m_currentTextBox.Text = element.m_counter.ToString("X");
            }
            m_handler = handler;
            m_blockData = blockData;
        }

        public override void Update() {
            if (m_okButton.IsClicked) {
                if (uint.TryParse(m_overflowTextBox.Text, NumberStyles.HexNumber, null, out uint newOverflow)
                    && uint.TryParse(m_currentTextBox.Text, NumberStyles.HexNumber, null, out uint newCurrent)) {
                    if (newOverflow > 0
                        && newCurrent >= newOverflow) {
                        DialogsManager.ShowDialog(
                            null,
                            new MessageDialog(
                                "发生错误",
                                "不能设置当前电压大于等于溢出电压",
                                "OK",
                                null,
                                null
                            )
                        );
                    }
                    else {
                        m_blockData.Data = newOverflow;
                        Dismiss(true, newCurrent);
                    }
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

        public void Dismiss(bool result, uint newCurrent = 0u) {
            DialogsManager.HideDialog(this);
            if (m_handler != null && result) {
                m_handler(newCurrent);
            }
        }
    }
}