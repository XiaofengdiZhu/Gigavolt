using System;
using System.Xml.Linq;
using Engine;

namespace Game {
    public class EditGVMemoryBankDialog : BaseEditGVMemoryBankDialog {
        public readonly Action m_handler;

        public readonly ButtonWidget m_okButton;
        public readonly ButtonWidget m_cancelButton;
        public readonly ButtonWidget m_moreButton;

        public readonly TextBoxWidget m_linearTextBox;
        public readonly TextBoxWidget m_rowCountTextBox;
        public readonly LabelWidget m_rowCountTextLabel;
        public readonly TextBoxWidget m_colCountTextBox;
        public readonly LabelWidget m_colCountTextLabel;
        public readonly LabelWidget m_IDLabel;
        public readonly BitmapButtonWidget m_copyIDButton;
        public readonly BitmapButtonWidget m_copyDataButton;
        public readonly BitmapButtonWidget m_deleteDataButton;
        public readonly BevelledButtonWidget m_helpButton;

        public readonly GVMemoryBankData m_memoryBankData;

        public string m_enterString;

        public static Action m_helpAction = () => {
            WebBrowserManager.LaunchBrowser(
                $"https://xiaofengdizhu.github.io/GigavoltDoc/{(ModsManager.Configs["Language"]?.StartsWith("zh") ?? false ? "zh" : "en")}/base/shift/memory_bank.html"
            );
        };

        public virtual Action HelpAction => m_helpAction;

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
            m_IDLabel.Text += $"ID: {memoryBankData.ID.ToString("X", null)}";
            m_copyIDButton = Children.Find<BitmapButtonWidget>("EditGVMemoryBankDialog.CopyID");
            m_copyDataButton = Children.Find<BitmapButtonWidget>("EditGVMemoryBankDialog.CopyData");
            m_deleteDataButton = Children.Find<BitmapButtonWidget>("EditGVMemoryBankDialog.DeleteData");
            m_helpButton = Children.Find<BevelledButtonWidget>("EditGVMemoryBankDialog.Help");
            m_handler = handler;
            m_memoryBankData = memoryBankData;
            UpdateFromData();
        }

        public override void UpdateFromData() {
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
                    m_linearTextBox.Text = m_memoryBankData.GetString();
                    m_enterString = m_linearTextBox.Text;
                }
            }
        }

        public override void Update() {
            if (m_okButton.IsClicked) {
                if (m_enterString != m_linearTextBox.Text) {
                    if (int.TryParse(m_colCountTextBox.Text, out int width)
                        && width > 0
                        && int.TryParse(m_rowCountTextBox.Text, out int height)
                        && height > 0) {
                        try {
                            m_memoryBankData.String2Data(m_linearTextBox.Text, width, height);
                            m_memoryBankData.SaveString();
                            Dismiss(true);
                        }
                        catch (Exception ex) {
                            string error = ex.ToString();
                            Log.Error(error);
                            DialogsManager.ShowDialog(
                                null,
                                new MessageDialog(LanguageControl.Error, LanguageControl.Get(GetType().Name, 2), "OK", null, null)
                            );
                        }
                    }
                    else {
                        DialogsManager.ShowDialog(
                            null,
                            new MessageDialog(LanguageControl.Error, LanguageControl.Get(GetType().Name, 3), "OK", null, null)
                        );
                    }
                }
                else {
                    Dismiss(false);
                }
            }
            if (m_copyIDButton.IsClicked) {
                ClipboardManager.ClipboardString = m_memoryBankData.ID.ToString("X");
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
            if (m_helpButton.IsClicked) {
                HelpAction();
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