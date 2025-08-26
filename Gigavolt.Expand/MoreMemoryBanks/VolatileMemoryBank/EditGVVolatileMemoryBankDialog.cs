using System;

namespace Game {
    public class EditGVVolatileMemoryBankDialog(GVVolatileMemoryBankData memoryBankData, Action handler)
        : EditGVMemoryBankDialog(memoryBankData, handler) {
        public static Action m_volatileHelpAction = () => {
            bool zh = ModsManager.Configs["Language"]?.StartsWith("zh") ?? false;
            WebBrowserManager.LaunchBrowser(
                $"https://xiaofengdizhu.github.io/GigavoltDoc/{(zh ? "zh" : "en")}/expand/memory_banks/volatile_memory_banks.html#{(zh ? "易失性存储器" : "volatile-memory-bank")}"
            );
        };

        public override Action HelpAction => m_volatileHelpAction;

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