using System;
using System.Xml.Linq;
using Engine;

namespace Game {
    public class EditGVTruthTableDialog : Dialog {
        public Action<bool> m_handler;

        public ButtonWidget m_okButton;

        public ButtonWidget m_cancelButton;

        public TextBoxWidget m_linearTextBox;

        public GVTruthTableData m_truthTableData;

        public bool m_ignoreTextChanges;

        public EditGVTruthTableDialog(GVTruthTableData truthTableData, Action<bool> handler) {
            try {
                XElement node = ContentManager.Get<XElement>("Dialogs/EditGVTruthTableDialog");
                LoadContents(this, node);
                m_okButton = Children.Find<ButtonWidget>("EditGVTruthTableDialog.OK");
                m_cancelButton = Children.Find<ButtonWidget>("EditGVTruthTableDialog.Cancel");
                m_linearTextBox = Children.Find<TextBoxWidget>("EditGVTruthTableDialog.LinearText");
                m_handler = handler;
                m_truthTableData = truthTableData;
                m_linearTextBox.Text = m_truthTableData.LastLoadedString;
            }
            catch (Exception ex) {
                Log.Error(ex);
            }
        }

        public override void Update() {
            if (m_okButton.IsClicked) {
                m_truthTableData.LoadString(m_linearTextBox.Text, out string error);
                if (error == null) {
                    Dismiss(true);
                }
                else {
                    DialogsManager.ShowDialog(
                        null,
                        new MessageDialog(
                            "发生错误",
                            error,
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

        public void Dismiss(bool result) {
            DialogsManager.HideDialog(this);
            m_handler?.Invoke(result);
        }
    }
}