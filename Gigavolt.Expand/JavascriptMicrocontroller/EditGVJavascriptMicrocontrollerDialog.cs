using System;
using System.Xml.Linq;
using Engine;

namespace Game {
    public class EditGVJavascriptMicrocontrollerDialog : Dialog {
        public Action<bool> m_handler;

        public readonly ButtonWidget m_okButton;
        public readonly ButtonWidget m_cancelButton;
        public readonly TextBoxWidget m_linearTextBox;
        public readonly BitmapButtonWidget m_copyDataButton;
        public readonly BitmapButtonWidget m_deleteDataButton;
        public readonly BevelledButtonWidget[] m_portButtons = new BevelledButtonWidget[5];

        public GVJavascriptMicrocontrollerData m_blockData;
        public readonly int[] m_tempPortsDefinition;
        public readonly string[] m_state2String = [LanguageControl.GetContentWidgets("EditGVJavascriptMicrocontrollerDialog", 8), LanguageControl.GetContentWidgets("EditGVJavascriptMicrocontrollerDialog", 9), LanguageControl.GetContentWidgets("EditGVJavascriptMicrocontrollerDialog", 10)];

        public EditGVJavascriptMicrocontrollerDialog(GVJavascriptMicrocontrollerData blockData, Action<bool> handler) {
            try {
                XElement node = ContentManager.Get<XElement>("Dialogs/EditGVJavascriptMicrocontrollerDialog");
                LoadContents(this, node);
                m_okButton = Children.Find<ButtonWidget>("EditGVJavascriptMicrocontrollerDialog.OK");
                m_cancelButton = Children.Find<ButtonWidget>("EditGVJavascriptMicrocontrollerDialog.Cancel");
                m_linearTextBox = Children.Find<TextBoxWidget>("EditGVJavascriptMicrocontrollerDialog.LinearText");
                m_copyDataButton = Children.Find<BitmapButtonWidget>("EditGVJavascriptMicrocontrollerDialog.CopyData");
                m_deleteDataButton = Children.Find<BitmapButtonWidget>("EditGVJavascriptMicrocontrollerDialog.DeleteData");
                m_tempPortsDefinition = (int[])blockData.m_portsDefinition.Clone();
                for (int i = 0; i < 5; i++) {
                    BevelledButtonWidget button = Children.Find<BevelledButtonWidget>($"EditGVJavascriptMicrocontrollerDialog.P{GVJavascriptMicrocontrollerData.OriginDirection2CustomDirection(i)}");
                    button.Text = m_state2String[m_tempPortsDefinition[i] + 1];
                    m_portButtons[i] = button;
                }
                m_handler = handler;
                m_blockData = blockData;
                m_linearTextBox.Text = m_blockData.LastLoadedCode;
            }
            catch (Exception ex) {
                Log.Error(ex);
            }
        }

        public override void Update() {
            for (int i = 0; i < 5; i++) {
                if (m_portButtons[i].IsClicked) {
                    int newState = m_tempPortsDefinition[i] >= 1 ? -1 : m_tempPortsDefinition[i] + 1;
                    m_portButtons[i].Text = m_state2String[newState + 1];
                    m_tempPortsDefinition[i] = newState;
                }
            }
            if (m_copyDataButton.IsClicked
                && m_linearTextBox.Text.Length > 0) {
                ClipboardManager.ClipboardString = m_linearTextBox.Text;
            }
            if (m_deleteDataButton.IsClicked) {
                m_linearTextBox.Text = string.Empty;
            }
            if (m_okButton.IsClicked) {
                m_blockData.LoadCode(m_linearTextBox.Text, out string error);
                if (error == null) {
                    m_blockData.m_portsDefinition = (int[])m_tempPortsDefinition.Clone();
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