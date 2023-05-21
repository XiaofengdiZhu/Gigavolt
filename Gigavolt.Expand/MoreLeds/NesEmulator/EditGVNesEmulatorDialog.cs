using System;
using System.Xml.Linq;

namespace Game {
    public class EditGVNesEmulatorDialog : Dialog {
        public Action m_handler;

        public ButtonWidget m_okButton;

        public ButtonWidget m_cancelButton;

        public TextBoxWidget m_romPathTextBox;

        public EditGVNesEmulatorDialogData m_blockData;

        public SubsystemNesEmulatorBlockBehavior m_subsystem;

        public string m_lastRomPath;

        public EditGVNesEmulatorDialog(EditGVNesEmulatorDialogData blockData, SubsystemNesEmulatorBlockBehavior subsystem, Action handler) {
            XElement node = ContentManager.Get<XElement>("Dialogs/EditGVNesEmulatorDialog");
            LoadContents(this, node);
            m_okButton = Children.Find<ButtonWidget>("EditGVNesEmulatorDialog.OK");
            m_cancelButton = Children.Find<ButtonWidget>("EditGVNesEmulatorDialog.Cancel");
            m_romPathTextBox = Children.Find<TextBoxWidget>("EditGVNesEmulatorDialog.RomPath");
            m_romPathTextBox.Text = blockData.Data;
            m_lastRomPath = blockData.Data;
            m_handler = handler;
            m_blockData = blockData;
            m_subsystem = subsystem;
        }

        public override void Update() {
            if (m_okButton.IsClicked) {
                if (m_romPathTextBox.Text.Length > 0) {
                    if (m_romPathTextBox.Text == m_lastRomPath) {
                        Dismiss(false);
                    }
                    else {
                        try {
                            m_subsystem.LoadRomFromPath(m_romPathTextBox.Text);
                            m_blockData.Data = m_romPathTextBox.Text;
                            m_blockData.SaveString();
                            Dismiss(true);
                        }
                        catch (Exception ex) {
                            DialogsManager.ShowDialog(
                                null,
                                new MessageDialog(
                                    LanguageControl.Error,
                                    ex.ToString(),
                                    "OK",
                                    null,
                                    null
                                )
                            );
                        }
                    }
                }
                else {
                    DialogsManager.ShowDialog(
                        null,
                        new MessageDialog(
                            LanguageControl.Error,
                            "ROM·��δ��д",
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
            if (m_handler != null && result) {
                m_handler();
            }
        }
    }
}