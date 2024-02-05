using System;
using System.Xml.Linq;
using Engine;

namespace Game {
    public class EditGVFourDimensionalMemoryBankDialog : BaseEditGVMemoryBankDialog {
        public readonly Action m_handler;

        public readonly ButtonWidget m_okButton;
        public readonly ButtonWidget m_cancelButton;
        public readonly ButtonWidget m_moreButton;

        public readonly TextBoxWidget m_linearTextBox;
        public readonly TextBoxWidget m_xLengthTextBox;
        public readonly LabelWidget m_xLengthTextLabel;
        public readonly TextBoxWidget m_yLengthTextBox;
        public readonly LabelWidget m_yLengthTextLabel;
        public readonly TextBoxWidget m_zLengthTextBox;
        public readonly LabelWidget m_zLengthTextLabel;
        public readonly TextBoxWidget m_wLengthTextBox;
        public readonly LabelWidget m_wLengthTextLabel;
        public readonly LabelWidget m_IDLabel;
        public readonly BitmapButtonWidget m_copyIDButton;
        public readonly BitmapButtonWidget m_copyDataButton;
        public readonly BitmapButtonWidget m_deleteDataButton;

        public readonly GVFourDimensionalMemoryBankData m_memoryBankData;

        public string m_enterString;

        public EditGVFourDimensionalMemoryBankDialog(GVFourDimensionalMemoryBankData memoryBankData, Action handler) {
            XElement node = ContentManager.Get<XElement>("Dialogs/EditGVFourDimensionalMemoryBankDialog");
            LoadContents(this, node);
            m_okButton = Children.Find<ButtonWidget>("EditGVFourDimensionalMemoryBankDialog.OK");
            m_cancelButton = Children.Find<ButtonWidget>("EditGVFourDimensionalMemoryBankDialog.Cancel");
            m_moreButton = Children.Find<ButtonWidget>("EditGVFourDimensionalMemoryBankDialog.More");
            m_linearTextBox = Children.Find<TextBoxWidget>("EditGVFourDimensionalMemoryBankDialog.LinearText");
            m_xLengthTextBox = Children.Find<TextBoxWidget>("EditGVFourDimensionalMemoryBankDialog.xLength");
            m_xLengthTextLabel = Children.Find<LabelWidget>("EditGVFourDimensionalMemoryBankDialog.xLengthLabel");
            m_yLengthTextBox = Children.Find<TextBoxWidget>("EditGVFourDimensionalMemoryBankDialog.yLength");
            m_yLengthTextLabel = Children.Find<LabelWidget>("EditGVFourDimensionalMemoryBankDialog.yLengthLabel");
            m_zLengthTextBox = Children.Find<TextBoxWidget>("EditGVFourDimensionalMemoryBankDialog.zLength");
            m_zLengthTextLabel = Children.Find<LabelWidget>("EditGVFourDimensionalMemoryBankDialog.zLengthLabel");
            m_wLengthTextBox = Children.Find<TextBoxWidget>("EditGVFourDimensionalMemoryBankDialog.wLength");
            m_wLengthTextLabel = Children.Find<LabelWidget>("EditGVFourDimensionalMemoryBankDialog.wLengthLabel");
            m_IDLabel = Children.Find<LabelWidget>("EditGVFourDimensionalMemoryBankDialog.ID");
            m_IDLabel.Text += $"ID: {memoryBankData.m_ID.ToString("X", null)}";
            m_copyIDButton = Children.Find<BitmapButtonWidget>("EditGVFourDimensionalMemoryBankDialog.CopyID");
            m_copyDataButton = Children.Find<BitmapButtonWidget>("EditGVFourDimensionalMemoryBankDialog.CopyData");
            m_deleteDataButton = Children.Find<BitmapButtonWidget>("EditGVFourDimensionalMemoryBankDialog.DeleteData");
            m_handler = handler;
            m_memoryBankData = memoryBankData;
            UpdateFromData();
        }

        public override void UpdateFromData() {
            if (m_memoryBankData.Data != null) {
                m_xLengthTextBox.IsEnabled = false;
                m_xLengthTextBox.Text = m_memoryBankData.m_xLength.ToString();
                m_xLengthTextLabel.Color = Color.Gray;
                m_yLengthTextBox.IsEnabled = false;
                m_yLengthTextBox.Text = m_memoryBankData.m_yLength.ToString();
                m_yLengthTextLabel.Color = Color.Gray;
                m_zLengthTextBox.IsEnabled = false;
                m_zLengthTextBox.Text = m_memoryBankData.m_zLength.ToString();
                m_zLengthTextLabel.Color = Color.Gray;
                m_wLengthTextBox.IsEnabled = false;
                m_wLengthTextBox.Text = m_memoryBankData.m_wLength.ToString();
                m_wLengthTextLabel.Color = Color.Gray;
                if (m_memoryBankData.Data.Count > 100000) {
                    m_linearTextBox.Text = LanguageControl.Get(GetType().Name, 1);
                    m_linearTextBox.IsEnabled = false;
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
                    if (int.TryParse(m_xLengthTextBox.Text, out int xLength)
                        && xLength > 0
                        && int.TryParse(m_yLengthTextBox.Text, out int yLength)
                        && yLength > 0
                        && int.TryParse(m_zLengthTextBox.Text, out int zLength)
                        && zLength > 0
                        && int.TryParse(m_wLengthTextBox.Text, out int wLength)
                        && wLength > 0) {
                        try {
                            m_memoryBankData.String2Data(
                                m_linearTextBox.Text,
                                ref xLength,
                                ref yLength,
                                ref zLength,
                                ref wLength
                            );
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
                    }
                }
                else {
                    Dismiss(false);
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