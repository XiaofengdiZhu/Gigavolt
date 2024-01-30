using System;

namespace Game {
    public class EditGVVolatileMemoryBankDialog(GVVolatileMemoryBankData memoryBankData, Action handler) : EditGVMemoryBankDialog(memoryBankData, handler) {
        public override void UpdateFromData() {
            if (m_memoryBankData.Data != null) {
                m_rowCountTextBox.Text = m_memoryBankData.m_height.ToString();
                m_colCountTextBox.Text = m_memoryBankData.m_width.ToString();
                if (m_memoryBankData.Data.LongLength > 100000) {
                    m_linearTextBox.Text = LanguageControl.Get(GetType().BaseType?.Name, 1);
                    m_linearTextBox.IsEnabled = false;
                    m_okButton.IsEnabled = false;
                }
                else {
                    m_linearTextBox.Text = m_memoryBankData.GetString();
                    m_enterString = m_linearTextBox.Text;
                }
            }
        }
    }
}