using System;
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
        public uint m_overflow;
        public uint m_initial;
        public uint m_current;

        public EditGVCounterDialog(GVCounterData blockData, CounterGVElectricElement element, Action<uint> handler) {
            XElement node = ContentManager.Get<XElement>("Dialogs/EditGVCounterDialog");
            LoadContents(this, node);
            m_overflow = blockData.Overflow;
            m_initial = blockData.Initial;
            m_okButton = Children.Find<ButtonWidget>("EditGVCounterDialog.OK");
            m_cancelButton = Children.Find<ButtonWidget>("EditGVCounterDialog.Cancel");
            m_overflowButton = Children.Find<BevelledButtonWidget>("EditGVCounterDialog.Overflow");
            m_overflowButton.Text = m_overflow.ToString("X");
            m_initialButton = Children.Find<BevelledButtonWidget>("EditGVCounterDialog.Initial");
            m_initialButton.Text = m_initial.ToString("X");
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
                DialogsManager.ShowDialog(
                    this,
                    new EditGVUintDialog(
                        m_overflow,
                        newOverflow => {
                            if (newOverflow != 0
                                && newOverflow <= m_initial) {
                                newOverflow = m_initial + 1;
                            }
                            m_overflow = newOverflow;
                            string newOverflowText = newOverflow.ToString("X");
                            m_overflowButton.Text = newOverflowText;
                            if (m_current >= newOverflow) {
                                m_current = newOverflow - 1;
                                m_currentButton.Text = newOverflowText;
                            }
                        }
                    )
                );
            }
            else if (m_initialButton.IsClicked) {
                DialogsManager.ShowDialog(
                    this,
                    new EditGVUintDialog(
                        m_initial,
                        newInitial => {
                            if (newInitial >= m_overflow) {
                                newInitial = m_overflow - 1;
                            }
                            m_initial = newInitial;
                            string newInitialText = newInitial.ToString("X");
                            m_initialButton.Text = newInitialText;
                            if (m_current < newInitial) {
                                m_current = newInitial;
                                m_currentButton.Text = newInitialText;
                            }
                        }
                    )
                );
            }
            else if (m_currentButton.IsClicked) {
                DialogsManager.ShowDialog(
                    this,
                    new EditGVUintDialog(
                        m_current,
                        newCurrent => {
                            if (newCurrent < m_initial) {
                                newCurrent = m_initial;
                            }
                            else if (newCurrent >= m_overflow) {
                                newCurrent = m_overflow - 1;
                            }
                            m_current = newCurrent;
                            m_currentButton.Text = newCurrent.ToString("X");
                        }
                    )
                );
            }
            if (m_okButton.IsClicked) {
                m_blockData.Overflow = m_overflow;
                m_blockData.Initial = m_initial;
                Dismiss(true, m_current);
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