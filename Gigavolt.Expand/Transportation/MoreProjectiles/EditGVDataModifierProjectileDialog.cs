using System;
using System.Xml.Linq;
using Engine;

namespace Game {
    public class EditGVDataModifierProjectileDialog : Dialog {
        public readonly ButtonWidget m_cancelButton;

        public readonly TextBoxWidget m_dataTextBox;

        public readonly Action<int> m_handler;

        public readonly ButtonWidget m_okButton;

        public BevelledButtonWidget[] m_binaryKeys = new BevelledButtonWidget[14];

        public int m_data;

        string m_lastInputText = "0";

        public EditGVDataModifierProjectileDialog(int oldData, Action<int> handler) {
            XElement node = ContentManager.Get<XElement>("Dialogs/EditGVDataModifierProjectileDialog");
            LoadContents(this, node);
            m_okButton = Children.Find<ButtonWidget>("EditGVDataModifierProjectileDialog.OK");
            m_cancelButton = Children.Find<ButtonWidget>("EditGVDataModifierProjectileDialog.Cancel");
            m_dataTextBox = Children.Find<TextBoxWidget>("EditGVDataModifierProjectileDialog.Data");
            m_dataTextBox.Text = oldData.ToString();
            m_handler = handler;
            GridPanelWidget grid = Children.Find<GridPanelWidget>("BinaryInputGrid");
            for (int i = 0; i < 14; i++) {
                BevelledButtonWidget mBinaryKey = new() { Size = new Vector2(48, 48), Text = "0", IsAutoCheckingEnabled = true };
                grid.Children.Add(mBinaryKey);
                grid.SetWidgetCell(mBinaryKey, new Point2(13 - i, 0));
                m_binaryKeys[i] = mBinaryKey;
            }
        }

        public override void Update() {
            for (int i = 0; i < 14; i++) {
                BevelledButtonWidget cellButton = m_binaryKeys[i];
                if (cellButton.IsClicked) {
                    m_data = KeysStateToData();
                    m_dataTextBox.Text = m_lastInputText = m_data.ToString();
                    UpdateKeysState();
                }
            }
            if (m_dataTextBox.Text != m_lastInputText) {
                if (TryParseText(out int parseData)) {
                    m_lastInputText = m_dataTextBox.Text;
                    m_data = parseData;
                }
                else {
                    m_dataTextBox.Text = m_lastInputText;
                    DialogsManager.ShowDialog(
                        null,
                        new MessageDialog(
                            "发生错误",
                            "特殊值输入不符合要求",
                            "OK",
                            null,
                            null
                        )
                    );
                }
                UpdateKeysState();
            }
            if (m_okButton.IsClicked) {
                Dismiss(true, m_data);
            }
            if (Input.Cancel
                || m_cancelButton.IsClicked) {
                Dismiss(false);
            }
        }

        public void UpdateKeysState() {
            for (int i = 0; i < 14; i++) {
                int cellState = (m_data >> i) & 1;
                m_binaryKeys[i].IsChecked = cellState == 1;
                m_binaryKeys[i].Text = cellState == 1 ? "1" : "0";
            }
        }

        public bool TryParseText(out int parseData) {
            if (m_dataTextBox.Text.Length == 0) {
                parseData = 0;
                return true;
            }
            return int.TryParse(m_dataTextBox.Text, out parseData) && parseData >= 0 && parseData < 16384;
        }

        public int KeysStateToData() {
            int result = 0;
            for (ushort i = 0; i < 14; i++) {
                BevelledButtonWidget cellKey = m_binaryKeys[i];
                result = SetBitValue(result, i, cellKey.IsChecked);
            }
            return result;
        }

        public static int SetBitValue(int value, ushort index, bool bitValue) {
            if (index > 31) {
                throw new ArgumentOutOfRangeException("index"); //索引出错
            }
            int val = 1 << index;
            return bitValue ? value | val : value & ~val;
        }

        public void Dismiss(bool result, int data = 0) {
            DialogsManager.HideDialog(this);
            if (m_handler != null && result) {
                m_handler(data);
            }
        }
    }
}