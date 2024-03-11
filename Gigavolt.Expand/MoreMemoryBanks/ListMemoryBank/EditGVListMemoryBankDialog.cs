using System;
using System.Xml.Linq;
using Engine;

namespace Game {
    public class EditGVListMemoryBankDialog : BaseEditGVMemoryBankDialog {
        public readonly Action m_handler;

        public readonly ButtonWidget m_okButton;
        public readonly ButtonWidget m_cancelButton;
        public readonly ButtonWidget m_moreButton;

        public readonly TextBoxWidget m_linearTextBox;
        public readonly TextBoxWidget m_colCountTextBox;
        public readonly TextBoxWidget m_offsetTextBox;
        public readonly TextBoxWidget m_widthTextBox;
        public readonly TextBoxWidget m_heightTextBox;
        public readonly LabelWidget m_colCountTextLabel;
        public readonly LabelWidget m_IDLabel;
        public readonly BitmapButtonWidget m_copyIDButton;
        public readonly BitmapButtonWidget m_copyDataButton;
        public readonly BitmapButtonWidget m_deleteDataButton;

        public readonly GVListMemoryBankData m_memoryBankData;

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
            m_offsetTextBox = Children.Find<TextBoxWidget>("EditGVListMemoryBankDialog.Offset");
            m_widthTextBox = Children.Find<TextBoxWidget>("EditGVListMemoryBankDialog.Width");
            m_heightTextBox = Children.Find<TextBoxWidget>("EditGVListMemoryBankDialog.Height");
            m_IDLabel = Children.Find<LabelWidget>("EditGVListMemoryBankDialog.ID");
            m_IDLabel.Text += $"ID: {memoryBankData.m_ID.ToString("X", null)}";
            m_copyIDButton = Children.Find<BitmapButtonWidget>("EditGVListMemoryBankDialog.CopyID");
            m_copyDataButton = Children.Find<BitmapButtonWidget>("EditGVListMemoryBankDialog.CopyData");
            m_deleteDataButton = Children.Find<BitmapButtonWidget>("EditGVListMemoryBankDialog.DeleteData");
            m_handler = handler;
            m_memoryBankData = memoryBankData;
            UpdateFromData();
        }

        public override void UpdateFromData() {
            if (m_memoryBankData.Data != null) {
                m_colCountTextBox.Text = m_memoryBankData.Data.Count.ToString();
                if (m_memoryBankData.Data.Count > 100000) {
                    m_linearTextBox.Text = LanguageControl.Get(GetType().Name, 1);
                    m_enterString = m_linearTextBox.Text;
                    m_linearTextBox.IsEnabled = false;
                    m_colCountTextLabel.Color = Color.Gray;
                }
                else {
                    m_linearTextBox.Text = m_memoryBankData.GetString();
                    m_enterString = m_linearTextBox.Text;
                }
                m_widthTextBox.Text = m_memoryBankData.m_width.ToString();
                m_heightTextBox.Text = m_memoryBankData.m_height.ToString();
                m_offsetTextBox.Text = m_memoryBankData.m_offset.ToString();
            }
        }

        public override void Update() {
            if (m_okButton.IsClicked) {
                if (uint.TryParse(m_widthTextBox.Text, out uint width)
                    && uint.TryParse(m_heightTextBox.Text, out uint height)
                    && uint.TryParse(m_offsetTextBox.Text, out uint offset)
                    && int.TryParse(m_colCountTextBox.Text, out int length)
                    && length >= 0) {
                    if (m_enterString != m_linearTextBox.Text) {
                        try {
                            m_memoryBankData.String2Data(m_linearTextBox.Text, length);
                            m_memoryBankData.m_width = width;
                            m_memoryBankData.m_height = height;
                            m_memoryBankData.m_offset = offset;
                            m_memoryBankData.SaveString();
                            Dismiss(true);
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
                    }
                    else {
                        m_memoryBankData.m_width = width;
                        m_memoryBankData.m_height = height;
                        m_memoryBankData.m_offset = offset;
                        m_memoryBankData.m_updateTime = DateTime.Now;
                        if (m_memoryBankData.Data.Count > length) {
                            m_memoryBankData.Data.RemoveRange(length, m_memoryBankData.Data.Count - length);
                            m_memoryBankData.m_dataChanged = true;
                            m_memoryBankData.SaveString();
                            Dismiss(true);
                        }
                        else {
                            Dismiss(false);
                        }
                    }
                }
                else {
                    DialogsManager.ShowDialog(
                        null,
                        new MessageDialog(
                            LanguageControl.Error,
                            LanguageControl.Get(GetType().Name, 3),
                            "OK",
                            null,
                            null
                        )
                    );
                    return;
                }
            }
            if (m_copyIDButton.IsClicked) {
                ClipboardManager.ClipboardString = m_memoryBankData.m_ID.ToString("X");
            }
            if (m_copyDataButton.IsClicked
                && m_linearTextBox.Text.Length > 0) {
                ClipboardManager.ClipboardString = m_linearTextBox.Text;
            }
            if (m_deleteDataButton.IsClicked) {
                m_linearTextBox.Text = string.Empty;
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

        public override void Dismiss(bool result, bool hide = true) {
            if (hide) {
                DialogsManager.HideDialog(this);
            }
            if (m_handler != null && result) {
                m_handler();
            }
        }

        public override GVArrayData GetArrayData() => m_memoryBankData;
    }
}