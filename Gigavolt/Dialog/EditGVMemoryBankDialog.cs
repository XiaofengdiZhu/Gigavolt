using System;
using System.Xml.Linq;
using Engine;
using Engine.Media;

namespace Game {
    public class EditGVMemoryBankDialog : Dialog {
        public Action m_handler;

        public ButtonWidget m_okButton;
        public ButtonWidget m_cancelButton;
        public ButtonWidget m_moreButton;

        public TextBoxWidget m_linearTextBox;
        public TextBoxWidget m_rowCountTextBox;
        public TextBoxWidget m_colCountTextBox;
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
            m_colCountTextBox = Children.Find<TextBoxWidget>("EditGVMemoryBankDialog.ColCount");
            m_IDLabel = Children.Find<LabelWidget>("EditGVMemoryBankDialog.ID");
            m_IDLabel.Text += $"ID: {memoryBankData.m_ID.ToString("X", null)}";
            m_handler = handler;
            m_memoryBankData = memoryBankData;
            UpdateFromData();
        }

        public void UpdateFromData() {
            if (!(m_memoryBankData.Data == null)) {
                m_rowCountTextBox.IsEnabled = false;
                m_rowCountTextBox.Text = m_memoryBankData.Data.Height.ToString();
                m_colCountTextBox.IsEnabled = false;
                m_colCountTextBox.Text = m_memoryBankData.Data.Width.ToString();
                if (m_memoryBankData.Data.Pixels.LongLength > 1000000) {
                    m_linearTextBox.Text = LanguageControl.Get(GetType().Name, 1);
                    m_linearTextBox.IsEnabled = false;
                    m_okButton.IsEnabled = false;
                }
                else {
                    m_linearTextBox.Text = GVMemoryBankData.Image2String(m_memoryBankData.Data);
                    m_enterString = m_linearTextBox.Text;
                }
            }
        }

        public override void Update() {
            if (m_okButton.IsClicked) {
                if (m_enterString != m_linearTextBox.Text) {
                    int width = 0;
                    int height = 0;
                    int.TryParse(m_colCountTextBox.Text, out width);
                    int.TryParse(m_rowCountTextBox.Text, out height);
                    string error = null;
                    Image image = null;
                    try {
                        image = GVMemoryBankData.String2Image(m_linearTextBox.Text, width, height);
                    }
                    catch (Exception ex) {
                        error = ex.ToString();
                        Log.Error(error);
                    }
                    if (image == null) {
                        DialogsManager.ShowDialog(
                            null,
                            new MessageDialog(
                                LanguageControl.Error,
                                error ?? LanguageControl.Get(GetType().Name, 2),
                                "OK",
                                null,
                                null
                            )
                        );
                    }
                    else {
                        m_memoryBankData.Data = image;
                        m_memoryBankData.SaveString();
                        Dismiss(true);
                    }
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

        public void Dismiss(bool result) {
            DialogsManager.HideDialog(this);
            if (m_handler != null && result) {
                m_handler();
            }
        }
    }
}