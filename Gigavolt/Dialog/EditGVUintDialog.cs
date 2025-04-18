using System;
using System.Xml.Linq;
using Engine;
using Engine.Input;

namespace Game {
    public class EditGVUintDialog : Dialog {
        public readonly Action<uint> m_handler;

        public readonly BevelledButtonWidget[] BinKeyboard = new BevelledButtonWidget[32];

        public readonly CheckboxWidget OctCheckbox;
        public readonly TextBoxWidget OctTextBox;
        public readonly BitmapButtonWidget CopyOct;
        public readonly CheckboxWidget DecCheckbox;
        public readonly TextBoxWidget DecTextBox;
        public readonly BitmapButtonWidget CopyDec;
        public readonly CheckboxWidget HexCheckbox;
        public readonly TextBoxWidget HexTextBox;
        public readonly BitmapButtonWidget CopyHex;
        public readonly LabelWidget FixedLabel;
        public readonly BitmapButtonWidget CopyFixed;
        public readonly TransparentRectangleWidget ColorRectangle;

        public readonly BevelledButtonWidget NumberKeyboardShiftLeft;
        public readonly BevelledButtonWidget NumberKeyboardShiftRight;
        public readonly BevelledButtonWidget NumberKeyboardNegate;
        public readonly BevelledButtonWidget NumberKeyboardBackSpace;
        public readonly BevelledButtonWidget NumberKeyboardClearEntry;
        public readonly BevelledButtonWidget[] NumberKeyboard = new BevelledButtonWidget[16];
        public readonly BevelledButtonWidget NumberKeyboardPlusPlus;
        public readonly BevelledButtonWidget NumberKeyboardSubSub;
        public readonly BevelledButtonWidget NumberKeyboardLeft;
        public readonly BevelledButtonWidget NumberKeyboardRight;

        public readonly ButtonWidget ButtonOk;
        public readonly ButtonWidget ButtonCancel;

        public readonly uint m_originalVoltage;
        public uint m_lastValidVoltage;
        public string m_lastOctString = "0";
        public string m_lastDecString = "0";
        public string m_lastHexString = "0";
        TextBoxWidget m_lastFocusedTextBox;
        int m_lastCaret;
        int m_lastFocusedLength;

        public EditGVUintDialog(uint originalVoltage, Action<uint> handler) {
            XElement node = ContentManager.Get<XElement>("Dialogs/EditGVUintDialog");
            LoadContents(this, node);
            for (int i = 0; i < 32; i++) {
                BinKeyboard[i] = Children.Find<BevelledButtonWidget>($"EditGVUintDialog.BinKeyboard{i}");
            }
            OctCheckbox = Children.Find<CheckboxWidget>("EditGVUintDialog.OctCheckbox");
            OctTextBox = Children.Find<TextBoxWidget>("EditGVUintDialog.OctTextBox");
            CopyOct = Children.Find<BitmapButtonWidget>("EditGVUintDialog.CopyOct");
            DecCheckbox = Children.Find<CheckboxWidget>("EditGVUintDialog.DecCheckbox");
            DecTextBox = Children.Find<TextBoxWidget>("EditGVUintDialog.DecTextBox");
            CopyDec = Children.Find<BitmapButtonWidget>("EditGVUintDialog.CopyDec");
            HexCheckbox = Children.Find<CheckboxWidget>("EditGVUintDialog.HexCheckbox");
            HexCheckbox.IsChecked = true;
            HexTextBox = Children.Find<TextBoxWidget>("EditGVUintDialog.HexTextBox");
            CopyHex = Children.Find<BitmapButtonWidget>("EditGVUintDialog.CopyHex");
            FixedLabel = Children.Find<LabelWidget>("EditGVUintDialog.FixedLabel");
            CopyFixed = Children.Find<BitmapButtonWidget>("EditGVUintDialog.CopyFixed");
            ColorRectangle = Children.Find<TransparentRectangleWidget>("EditGVUintDialog.Color");
            NumberKeyboardShiftLeft = Children.Find<BevelledButtonWidget>("EditGVUintDialog.NumberKeyboardShiftLeft");
            NumberKeyboardShiftRight = Children.Find<BevelledButtonWidget>("EditGVUintDialog.NumberKeyboardShiftRight");
            NumberKeyboardNegate = Children.Find<BevelledButtonWidget>("EditGVUintDialog.NumberKeyboardNegate");
            NumberKeyboardBackSpace = Children.Find<BevelledButtonWidget>("EditGVUintDialog.NumberKeyboardBackSpace");
            NumberKeyboardClearEntry = Children.Find<BevelledButtonWidget>("EditGVUintDialog.NumberKeyboardClearEntry");
            for (int i = 0; i < 16; i++) {
                NumberKeyboard[i] = Children.Find<BevelledButtonWidget>($"EditGVUintDialog.NumberKeyboard{i:X}");
            }
            NumberKeyboardPlusPlus = Children.Find<BevelledButtonWidget>("EditGVUintDialog.NumberKeyboardPlusPlus");
            NumberKeyboardSubSub = Children.Find<BevelledButtonWidget>("EditGVUintDialog.NumberKeyboardSubSub");
            NumberKeyboardLeft = Children.Find<BevelledButtonWidget>("EditGVUintDialog.NumberKeyboardLeft");
            NumberKeyboardRight = Children.Find<BevelledButtonWidget>("EditGVUintDialog.NumberKeyboardRight");
            ButtonOk = Children.Find<ButtonWidget>("EditGVUintDialog.OK");
            ButtonCancel = Children.Find<ButtonWidget>("EditGVUintDialog.Cancel");
            m_handler = handler;
            m_originalVoltage = originalVoltage;
            if (originalVoltage != 0u) {
                ApplyNewVoltage(originalVoltage);
            }
        }

        public override void Update() {
            for (int i = 0; i < 32; i++) {
                if (BinKeyboard[i].IsClicked) {
                    ApplyNewVoltage(m_lastValidVoltage ^ (1u << i));
                }
            }
            if (NumberKeyboardShiftLeft.IsClicked) {
                ApplyNewVoltage(m_lastValidVoltage << 1);
            }
            else if (NumberKeyboardShiftRight.IsClicked) {
                ApplyNewVoltage(m_lastValidVoltage >> 1);
            }
            else if (NumberKeyboardNegate.IsClicked) {
                ApplyNewVoltage(~m_lastValidVoltage);
            }
            else if (NumberKeyboardClearEntry.IsClicked) {
                ApplyNewVoltage(0u, true);
            }
            if (OctTextBox.Text != m_lastOctString) {
                try {
                    uint newVoltage = OctTextBox.Text == string.Empty ? 0u : Convert.ToUInt32(OctTextBox.Text, 8);
                    ApplyNewVoltage(newVoltage, true);
                }
                catch (Exception e) {
                    OctTextBox.Text = m_lastOctString;
                    ShowErrorDialog(e, 8);
                }
            }
            else if (DecTextBox.Text != m_lastDecString) {
                try {
                    uint newVoltage = DecTextBox.Text == string.Empty ? 0u : Convert.ToUInt32(DecTextBox.Text, 10);
                    ApplyNewVoltage(newVoltage, true);
                }
                catch (Exception e) {
                    DecTextBox.Text = m_lastDecString;
                    ShowErrorDialog(e);
                }
            }
            else if (HexTextBox.Text != m_lastHexString) {
                try {
                    uint newVoltage = HexTextBox.Text == string.Empty ? 0u : Convert.ToUInt32(HexTextBox.Text, 16);
                    ApplyNewVoltage(newVoltage, true);
                }
                catch (Exception e) {
                    HexTextBox.Text = m_lastHexString;
                    ShowErrorDialog(e, 16);
                }
            }
            else if (OctCheckbox.IsClicked
                || OctTextBox.HasFocus) {
                if (!OctCheckbox.IsChecked) {
                    OctCheckbox.IsChecked = true;
                    if (DecCheckbox.IsChecked) {
                        DecCheckbox.IsChecked = false;
                    }
                    if (HexCheckbox.IsChecked) {
                        HexCheckbox.IsChecked = false;
                    }
                    for (int i = 8; i < 16; i++) {
                        NumberKeyboard[i].IsEnabled = false;
                    }
                }
            }
            else if (DecCheckbox.IsClicked
                || DecTextBox.HasFocus) {
                if (!DecCheckbox.IsChecked) {
                    DecCheckbox.IsChecked = true;
                    if (OctCheckbox.IsChecked) {
                        OctCheckbox.IsChecked = false;
                    }
                    if (HexCheckbox.IsChecked) {
                        HexCheckbox.IsChecked = false;
                    }
                    for (int i = 8; i < 10; i++) {
                        NumberKeyboard[i].IsEnabled = true;
                    }
                    for (int i = 10; i < 16; i++) {
                        NumberKeyboard[i].IsEnabled = false;
                    }
                }
            }
            else if (HexCheckbox.IsClicked
                || HexTextBox.HasFocus) {
                if (!HexCheckbox.IsChecked) {
                    HexCheckbox.IsChecked = true;
                    if (OctCheckbox.IsChecked) {
                        OctCheckbox.IsChecked = false;
                    }
                    if (DecCheckbox.IsChecked) {
                        DecCheckbox.IsChecked = false;
                    }
                    for (int i = 8; i < 16; i++) {
                        NumberKeyboard[i].IsEnabled = true;
                    }
                }
            }
            TextBoxWidget newFocusedTextBox = OctCheckbox.IsChecked ? OctTextBox :
                DecCheckbox.IsChecked ? DecTextBox : HexTextBox;
            if (!newFocusedTextBox.HasFocus) {
                newFocusedTextBox.HasFocus = true;
            }
            if (newFocusedTextBox == m_lastFocusedTextBox) {
                int newFocusedLength = newFocusedTextBox.Text.Length;
                if (m_lastCaret > newFocusedLength) {
                    newFocusedTextBox.Caret = newFocusedLength;
                }
                else {
                    newFocusedTextBox.Caret = m_lastCaret;
                    if (newFocusedLength != m_lastFocusedLength) {
                        newFocusedTextBox.Caret += newFocusedLength - m_lastFocusedLength;
                    }
                }
            }
            else {
                newFocusedTextBox.Caret = newFocusedTextBox.Text.Length;
            }
            for (int i = 0; i < 16; i++) {
                if (NumberKeyboard[i].IsClicked
                    && newFocusedTextBox.Text.Length < newFocusedTextBox.MaximumLength) {
                    string newVoltageString = newFocusedTextBox.Text.Insert(newFocusedTextBox.Caret, NumberKeyboard[i].Text);
                    int fromBase = OctCheckbox.IsChecked ? 8 :
                        DecCheckbox.IsChecked ? 10 : 16;
                    try {
                        uint newVoltage = Convert.ToUInt32(newVoltageString, fromBase);
                        ApplyNewVoltage(newVoltage);
                        newFocusedTextBox.Caret++;
                    }
                    catch (Exception e) {
                        newFocusedTextBox.Text = OctCheckbox.IsChecked ? m_lastOctString :
                            DecCheckbox.IsChecked ? m_lastDecString : m_lastHexString;
                        ShowErrorDialog(e, fromBase);
                    }
                }
            }
            if (NumberKeyboardBackSpace.IsClicked) {
                if (newFocusedTextBox.Caret > 0) {
                    bool flag = newFocusedTextBox.Caret != newFocusedTextBox.Text.Length;
                    string newVoltageString = newFocusedTextBox.Text.Remove(newFocusedTextBox.Caret - 1, 1).Trim();
                    uint newVoltage = newVoltageString == string.Empty
                        ? 0u
                        : Convert.ToUInt32(
                            newVoltageString,
                            OctCheckbox.IsChecked ? 8 :
                            DecCheckbox.IsChecked ? 10 : 16
                        );
                    ApplyNewVoltage(newVoltage, true);
                    if (flag) {
                        newFocusedTextBox.Caret--;
                    }
                }
            }
            else if (NumberKeyboardPlusPlus.IsClicked
                || newFocusedTextBox.Input.IsKeyDownOnce(Key.Plus)) {
                if (m_lastValidVoltage < uint.MaxValue) {
                    ApplyNewVoltage(m_lastValidVoltage + 1);
                }
            }
            else if (NumberKeyboardSubSub.IsClicked
                || newFocusedTextBox.Input.IsKeyDownOnce(Key.Minus)) {
                if (m_lastValidVoltage > 0u) {
                    ApplyNewVoltage(m_lastValidVoltage - 1);
                }
            }
            else if (NumberKeyboardLeft.IsClicked
                || newFocusedTextBox.Input.IsKeyDownOnce(Key.LeftArrow)) {
                newFocusedTextBox.Caret--;
            }
            else if (NumberKeyboardRight.IsClicked
                || newFocusedTextBox.Input.IsKeyDownOnce(Key.RightArrow)) {
                newFocusedTextBox.Caret++;
            }
            else if (CopyOct.IsClicked) {
                ClipboardManager.ClipboardString = OctTextBox.Text;
            }
            else if (CopyDec.IsClicked) {
                ClipboardManager.ClipboardString = DecTextBox.Text;
            }
            else if (CopyHex.IsClicked) {
                ClipboardManager.ClipboardString = HexTextBox.Text;
            }
            else if (CopyFixed.IsClicked) {
                ClipboardManager.ClipboardString = FixedLabel.Text;
            }
            m_lastFocusedTextBox = OctCheckbox.IsChecked ? OctTextBox :
                DecCheckbox.IsChecked ? DecTextBox : HexTextBox;
            m_lastCaret = m_lastFocusedTextBox.Caret;
            m_lastFocusedLength = m_lastFocusedTextBox.Text.Length;
            if (ButtonOk.IsClicked) {
                if (m_lastValidVoltage != m_originalVoltage) {
                    Dismiss(true, m_lastValidVoltage);
                }
                else {
                    Dismiss(false);
                }
            }
            if (Input.Cancel
                || ButtonCancel.IsClicked
                || Input.Back) {
                Dismiss(false);
            }
        }

        public void Dismiss(bool result, uint voltage = 0u) {
            DialogsManager.HideDialog(this);
            if (m_handler != null && result) {
                m_handler(voltage);
            }
        }

        public void ApplyNewVoltage(uint newVoltage, bool keepEmpty = false) {
            int num = -1;
            if (keepEmpty && newVoltage == 0u) {
                if (OctCheckbox.IsChecked) {
                    m_lastOctString = "";
                    OctTextBox.Text = m_lastOctString;
                    num = 0;
                }
                else if (DecCheckbox.IsChecked) {
                    m_lastDecString = "";
                    DecTextBox.Text = m_lastDecString;
                    num = 1;
                }
                else if (HexCheckbox.IsChecked) {
                    m_lastHexString = "";
                    HexTextBox.Text = m_lastHexString;
                    num = 2;
                }
            }
            if (num != 0) {
                m_lastOctString = Convert.ToString(newVoltage, 8).ToUpper();
                OctTextBox.Text = m_lastOctString;
            }
            if (num != 1) {
                m_lastDecString = Convert.ToString(newVoltage, 10).ToUpper();
                DecTextBox.Text = m_lastDecString;
            }
            if (num != 2) {
                m_lastHexString = Convert.ToString(newVoltage, 16).ToUpper();
                HexTextBox.Text = m_lastHexString;
            }
            FixedLabel.Text = ((newVoltage >> 31 == 1 ? -1 : 1) * (((newVoltage >> 16) & 0x7fffu) + (double)(newVoltage & 0xffffu) / 0xffff)).ToString("G");
            for (int i = 0; i < 32; i++) {
                bool newBit = ((newVoltage >> i) & 1u) == 1u;
                bool oldBit = ((m_lastValidVoltage >> i) & 1u) == 1u;
                if (newBit != oldBit) {
                    BinKeyboard[i].Text = newBit ? "1" : "0";
                    BinKeyboard[i].IsChecked = !newBit;
                }
            }
            ColorRectangle.FillColor = new Color(newVoltage);
            m_lastValidVoltage = newVoltage;
        }

        public void ShowErrorDialog(Exception exception, int fromBase = 10) {
            string typeName = GetType().Name;
            DialogsManager.ShowDialog(
                null,
                new MessageDialog(
                    LanguageControl.Get("ContentWidgets", typeName, "9"),
                    exception switch {
                        ArgumentOutOfRangeException => LanguageControl.Get("ContentWidgets", typeName, "10"),
                        FormatException => string.Format(LanguageControl.Get("ContentWidgets", typeName, "11"), fromBase == 8 ? "7" : "9", fromBase == 16 ? LanguageControl.Get("ContentWidgets", typeName, "12") : "", fromBase == 16 ? LanguageControl.Get("ContentWidgets", typeName, "13") : ""),
                        OverflowException => string.Format(
                            LanguageControl.Get("ContentWidgets", typeName, "14"),
                            fromBase switch {
                                8 => LanguageControl.Get("ContentWidgets", typeName, "15"),
                                16 => LanguageControl.Get("ContentWidgets", typeName, "16"),
                                _ => LanguageControl.Get("ContentWidgets", typeName, "17")
                            }
                        ),
                        _ => exception.ToString()
                    },
                    "OK",
                    null,
                    null
                )
            );
        }
    }
}