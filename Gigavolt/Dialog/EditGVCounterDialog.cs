using System;
using System.Globalization;
using System.Xml.Linq;
using Engine;

namespace Game {
    public class EditGVCounterDialog : Dialog {
        public readonly Action<uint> m_handler;

        public readonly ButtonWidget m_okButton;

        public readonly ButtonWidget m_cancelButton;

        public readonly BevelledButtonWidget m_overflowButton;
        public readonly BevelledButtonWidget m_initialButton;
        public readonly BevelledButtonWidget m_currentButton;
        public readonly LabelWidget m_currentLabel;

        public readonly GVCounterData m_blockData;
        public uint m_current;

        public EditGVCounterDialog(GVCounterData blockData, CounterGVElectricElement element, Action<uint> handler) {
            XElement node = ContentManager.Get<XElement>("Dialogs/EditGVCounterDialog");
            LoadContents(this, node);
            m_okButton = Children.Find<ButtonWidget>("EditGVCounterDialog.OK");
            m_cancelButton = Children.Find<ButtonWidget>("EditGVCounterDialog.Cancel");
            m_overflowButton = Children.Find<BevelledButtonWidget>("EditGVCounterDialog.Overflow");
            m_overflowButton.Text = blockData.Overflow.ToString("X");
            m_initialButton = Children.Find<BevelledButtonWidget>("EditGVCounterDialog.Initial");
            m_currentButton = Children.Find<BevelledButtonWidget>("EditGVCounterDialog.Current");
            m_currentLabel = Children.Find<LabelWidget>("EditGVCounterDialog.CurrentLabel");
            if (element == null) {
                m_currentButton.IsEnabled = false;
                m_currentButton.Text = "0";
                m_currentLabel.Color = Color.Gray;
            }
            else {
                m_current = element.m_counter;
                m_currentButton.Text = m_current.ToString("X");
            }
            m_handler = handler;
            m_blockData = blockData;
        }

        public override void Update() {
            if (m_overflowButton.IsClicked) {
                DialogsManager.ShowDialog(this, new EditGVUintDialog(m_blockData.Overflow, newOverflow => m_overflowButton.Text = newOverflow.ToString("X")));
            }
            else if (m_initialButton.IsClicked) {
                DialogsManager.ShowDialog(
                    this,
                    new EditGVUintDialog(
                        m_blockData.Initial,
                        newInitial => {
                            string newInitialText = newInitial.ToString("X");
                            m_initialButton.Text = newInitialText;
                            if (m_current < newInitial) {
                                m_currentButton.Text = newInitialText;
                            }
                        }
                    )
                );
            }
            else if (m_currentButton.IsClicked) {
                DialogsManager.ShowDialog(this, new EditGVUintDialog(m_current, newCurrent => m_currentButton.Text = newCurrent.ToString("X")));
            }
            if (m_okButton.IsClicked) {
                if (uint.TryParse(m_overflowButton.Text, NumberStyles.HexNumber, null, out uint newOverflow)
                    && uint.TryParse(m_initialButton.Text, NumberStyles.HexNumber, null, out uint newInitial)
                    && uint.TryParse(m_currentButton.Text, NumberStyles.HexNumber, null, out uint newCurrent)) {
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
                    else if (newCurrent < newInitial) {
                        DialogsManager.ShowDialog(
                            null,
                            new MessageDialog(
                                "发生错误",
                                "不能设置当前电压低于初始电压",
                                "OK",
                                null,
                                null
                            )
                        );
                    }
                    else {
                        m_blockData.Overflow = newOverflow;
                        m_blockData.Initial = newInitial;
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