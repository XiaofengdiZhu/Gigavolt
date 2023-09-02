using System;
using System.Xml.Linq;

namespace Game {
    public class EditGVDataModifierProjectileDialog : Dialog {
        public readonly Action<int> m_handler;

        public readonly ButtonWidget m_okButton;

        public readonly ButtonWidget m_cancelButton;

        public readonly TextBoxWidget m_dataTextBox;

        public EditGVDataModifierProjectileDialog(int oldData, Action<int> handler) {
            XElement node = ContentManager.Get<XElement>("Dialogs/EditGVDataModifierProjectileDialog");
            LoadContents(this, node);
            m_okButton = Children.Find<ButtonWidget>("EditGVDataModifierProjectileDialog.OK");
            m_cancelButton = Children.Find<ButtonWidget>("EditGVDataModifierProjectileDialog.Cancel");
            m_dataTextBox = Children.Find<TextBoxWidget>("EditGVDataModifierProjectileDialog.Data");
            m_dataTextBox.Text = oldData.ToString();
            m_handler = handler;
        }

        public override void Update() {
            if (m_okButton.IsClicked) {
                if (int.TryParse(m_dataTextBox.Text, out int data)
                    && data > 0
                    && data < 16384) {
                    Dismiss(true, data);
                }
                else {
                    if (m_dataTextBox.Text.Length == 0) {
                        Dismiss(false);
                    }
                    DialogsManager.ShowDialog(
                        null,
                        new MessageDialog(
                            "发生错误",
                            "特殊值输入不符合要求",
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

        public void Dismiss(bool result, int data = 0) {
            DialogsManager.HideDialog(this);
            if (m_handler != null && result) {
                m_handler(data);
            }
        }
    }
}