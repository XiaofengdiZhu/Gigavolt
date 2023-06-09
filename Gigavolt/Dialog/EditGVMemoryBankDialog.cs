using System;
using System.Xml.Linq;
using Engine;

namespace Game {
    public class EditGVMemoryBankDialog : Dialog {
        public Action m_handler;

        public ButtonWidget m_okButton;
        public ButtonWidget m_cancelButton;
        public ButtonWidget m_moreButton;

        public TextBoxWidget m_linearTextBox;
        public TextBoxWidget m_rowCountTextBox;
        public LabelWidget m_rowCountTextLabel;
        public TextBoxWidget m_colCountTextBox;
        public LabelWidget m_colCountTextLabel;
        public LabelWidget m_IDLabel;

        public GVMemoryBankData m_memoryBankData;

        public string m_enterString;

        public EditGVMemoryBankDialog(GVMemoryBankData memoryBankData, Action handler) {
            XElement node = ContentManager.Get<XElement>("Dialogs/EditGVMemoryBankDialog");
            LoadContents(this, node);
            m_okButton = Children.Find<ButtonWidget>("EditGVMemoryBankDialog.OK");
            m_cancelButton = Children.Find<ButtonWidget>("EditGVMemoryBankDialog.Cancel");
            m_moreButton = Children.Find<ButtonWidget>("EditGVMemoryBankDialog.More");
            m_linearTextBox = Children.Find<TextBoxWidget>("EditGVMemoryBankDialog.LinearText");
            m_rowCountTextBox = Children.Find<TextBoxWidget>("EditGVMemoryBankDialog.RowCount");
            m_rowCountTextLabel = Children.Find<LabelWidget>("EditGVMemoryBankDialog.RowCountLabel");
            m_colCountTextBox = Children.Find<TextBoxWidget>("EditGVMemoryBankDialog.ColCount");
            m_colCountTextLabel = Children.Find<LabelWidget>("EditGVMemoryBankDialog.ColCountLabel");
            m_IDLabel = Children.Find<LabelWidget>("EditGVMemoryBankDialog.ID");
            m_IDLabel.Text += $"ID: {memoryBankData.m_ID.ToString("X", null)}";
            m_handler = handler;
            m_memoryBankData = memoryBankData;
            UpdateFromData();
        }

        public void UpdateFromData() {
            if (m_memoryBankData.Data != null) {
                m_rowCountTextBox.IsEnabled = false;
                m_rowCountTextBox.Text = m_memoryBankData.m_height.ToString();
                m_rowCountTextLabel.Color = Color.Gray;
                m_colCountTextBox.IsEnabled = false;
                m_colCountTextBox.Text = m_memoryBankData.m_width.ToString();
                m_colCountTextLabel.Color = Color.Gray;
                if (m_memoryBankData.Data.LongLength > 100000) {
                    m_linearTextBox.Text = LanguageControl.Get(GetType().Name, 1);
                    m_linearTextBox.IsEnabled = false;
                    m_okButton.IsEnabled = false;
                }
                else {
                    m_linearTextBox.Text = m_memoryBankData.Data2String();
                    m_enterString = m_linearTextBox.Text;
                }
            }
        }

        public override void Update() {
            if (m_okButton.IsClicked) {
                if (m_enterString != m_linearTextBox.Text) {
                    int.TryParse(m_colCountTextBox.Text, out int width);
                    int.TryParse(m_rowCountTextBox.Text, out int height);
                    try {
                        m_memoryBankData.String2Data(m_linearTextBox.Text, width, height);
                    }
                    catch (Exception ex) {
                        string error = ex.ToString();
                        Log.Error(error);
                        DialogsManager.ShowDialog(
                            null,
                            new MessageDialog(
                                LanguageControl.Error,
                                LanguageControl.Get(GetType().Name, 2),
                                "OK",
                                null,
                                null
                            )
                        );
                    }
                    m_memoryBankData.SaveString();
                    Dismiss(true);
                }
                else {
                    Dismiss(false);
                }
            }
            if (m_moreButton.IsClicked) {
                if (!ScreensManager.m_screens.ContainsKey("GVMBExternalContent")) {
                    ScreensManager.AddScreen("GVMBExternalContent", new GVMBExternalContentScreen());
                }
                ScreensManager.SwitchScreen("GVMBExternalContent", this);
            }
            if (Input.Cancel
                || m_cancelButton.IsClicked) {
                Dismiss(false);
            }
        }

        public void Dismiss(bool result, bool hide = true) {
            if (hide) {
                DialogsManager.HideDialog(this);
            }
            if (m_handler != null && result) {
                m_handler();
            }
        }
    }
}