using System;
using System.Xml.Linq;

namespace Game {
    public class EditGVButtonCabinetDialog : Dialog {
        public readonly Action<int> m_handler;

        public readonly ButtonWidget m_okButton;
        public readonly ButtonWidget m_cancelButton;

        public readonly TextBoxWidget m_durationTextBox;

        public EditGVButtonCabinetDialog(int duration, Action<int> handler) {
            XElement node = ContentManager.Get<XElement>("Dialogs/EditGVButtonCabinetDialog");
            LoadContents(this, node);
            m_okButton = Children.Find<ButtonWidget>("EditGVButtonCabinetDialog.OK");
            m_cancelButton = Children.Find<ButtonWidget>("EditGVButtonCabinetDialog.Cancel");
            m_durationTextBox = Children.Find<TextBoxWidget>("EditGVButtonCabinetDialog.Duration");
            m_durationTextBox.Text = duration.ToString();
            m_handler = handler;
        }

        public override void Update() {
            if (m_okButton.IsClicked) {
                if (int.TryParse(m_durationTextBox.Text, out int duration)
                    && duration > 1) {
                    Dismiss(true, duration);
                }
                else {
                    string typeName = GetType().Name;
                    DialogsManager.ShowDialog(
                        null,
                        new MessageDialog(
                            LanguageControl.Get("ContentWidgets", typeName, "8"),
                            LanguageControl.Get("ContentWidgets", typeName, "9"),
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

        public void Dismiss(bool result, int duration = 0) {
            DialogsManager.HideDialog(this);
            if (m_handler != null && result) {
                m_handler(duration);
            }
        }
    }
}