using System;
using System.Xml.Linq;
using Engine;

namespace Game {
    public class EditGVListMemoryBankDialog : Dialog {
        public Action m_handler;

        public ButtonWidget m_okButton;
        public ButtonWidget m_cancelButton;
        public ButtonWidget m_moreButton;

        public TextBoxWidget m_linearTextBox;
        public TextBoxWidget m_colCountTextBox;
        public LabelWidget m_colCountTextLabel;
        public LabelWidget m_IDLabel;
        public BitmapButtonWidget m_copyIDButton;
        public BitmapButtonWidget m_copyDataButton;

        public GVListMemoryBankData m_memoryBankData;

        public string m_enterString;

        public EditGVListMemoryBankDialog(GVListMemoryBankData memoryBankData, Action handler) {
            XElement node = ContentManager.Get<XElement>("Dialogs/EditGVListMemoryBankDialog");
            LoadContents(this, node);
            m_okButton = Children.Find<ButtonWidget>("EditGVListMemoryBankDialog.OK");
            m_cancelButton = Children.Find<ButtonWidget>("EditGVListMemoryBankDialog.Cancel");
            m_moreButton = Children.Find<ButtonWidget>("EditGVListMemoryBankDialog.More");
            m_linearTextBox = Children.Find<TextBoxWidget>("EditGVListMemoryBankDialog.LinearText");
            m_colCountTextBox = Children.Find<TextBoxWidget>("EditGVListMemoryBankDialog.ColCount");
            m_colCountTextLabel = Children.Find<LabelWidget>("EditGVListMemoryBankDialog.ColCountLabel");
            m_IDLabel = Children.Find<LabelWidget>("EditGVListMemoryBankDialog.ID");
            m_IDLabel.Text += $"ID: {memoryBankData.m_ID.ToString("X", null)}";
            m_copyIDButton = Children.Find<BitmapButtonWidget>("EditGVListMemoryBankDialog.CopyID");
            m_copyDataButton = Children.Find<BitmapButtonWidget>("EditGVListMemoryBankDialog.CopyData");
            m_handler = handler;
            m_memoryBankData = memoryBankData;
            UpdateFromData();
        }

        public void UpdateFromData() {
            if (m_memoryBankData.Data != null) {
                m_colCountTextBox.Text = m_memoryBankData.Data.Count.ToString();
                if (m_memoryBankData.Data.Count > 100000) {
                    m_linearTextBox.Text = LanguageControl.Get(GetType().Name, 1);
                    m_linearTextBox.IsEnabled = false;
                    m_colCountTextLabel.Color = Color.Gray;
                    m_okButton.IsEnabled = false;
                }
                else {
                    m_linearTextBox.Text = m_memoryBankData.GetString();
                    m_enterString = m_linearTextBox.Text;
                }
            }
        }

        public override void Update() {
            if (m_okButton.IsClicked) {
                if (m_enterString != m_linearTextBox.Text) {
                    int.TryParse(m_colCountTextBox.Text, out int width);
                    try {
                        m_memoryBankData.String2Data(m_linearTextBox.Text, width);
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
            if (m_copyIDButton.IsClicked) {
                ClipboardManager.ClipboardString = m_memoryBankData.m_ID.ToString("X");
            }
            if (m_copyDataButton.IsClicked) {
                ClipboardManager.ClipboardString = m_linearTextBox.Text;
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