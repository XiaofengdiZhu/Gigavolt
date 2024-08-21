using System;
using System.Xml.Linq;
using Engine;

namespace Game {
    public class EditGVTruthTableDialog : Dialog {
        public readonly Action m_handler;

        public readonly ButtonWidget m_okButton;
        public readonly ButtonWidget m_cancelButton;
        public readonly BitmapButtonWidget m_copyDataButton;
        public readonly BitmapButtonWidget m_deleteDataButton;
        public readonly TextBoxWidget m_linearTextBox;
        public readonly BevelledButtonWidget m_helpButton;

        public readonly GVTruthTableData m_truthTableData;

        public bool m_ignoreTextChanges;

        public static Action m_helpAction = () => { WebBrowserManager.LaunchBrowser($"https://xiaofengdizhu.github.io/GigavoltDoc/{(ModsManager.Configs["Language"]?.StartsWith("zh") ?? false ? "zh" : "en")}/base/shift/truth_table.html"); };

        public EditGVTruthTableDialog(GVTruthTableData truthTableData, Action handler) {
            try {
                XElement node = ContentManager.Get<XElement>("Dialogs/EditGVTruthTableDialog");
                LoadContents(this, node);
                m_okButton = Children.Find<ButtonWidget>("EditGVTruthTableDialog.OK");
                m_cancelButton = Children.Find<ButtonWidget>("EditGVTruthTableDialog.Cancel");
                m_copyDataButton = Children.Find<BitmapButtonWidget>("EditGVTruthTableDialog.CopyData");
                m_deleteDataButton = Children.Find<BitmapButtonWidget>("EditGVTruthTableDialog.DeleteData");
                m_linearTextBox = Children.Find<TextBoxWidget>("EditGVTruthTableDialog.LinearText");
                m_helpButton = Children.Find<BevelledButtonWidget>("EditGVTruthTableDialog.Help");
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
            if (m_copyDataButton.IsClicked
                && m_linearTextBox.Text.Length > 0) {
                ClipboardManager.ClipboardString = m_linearTextBox.Text;
            }
            if (m_deleteDataButton.IsClicked) {
                m_linearTextBox.Text = string.Empty;
            }
            if (m_helpButton.IsClicked) {
                m_helpAction();
            }
            if (Input.Cancel
                || m_cancelButton.IsClicked) {
                Dismiss(false);
            }
        }

        public void Dismiss(bool result) {
            DialogsManager.HideDialog(this);
            if (m_handler != null && result) {
                m_handler();
            }
        }
    }
}