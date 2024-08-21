using System;

namespace Game {
    public class EditGVVolatileListMemoryBankDialog(GVVolatileListMemoryBankData memoryBankData, Action handler) : EditGVListMemoryBankDialog(memoryBankData, handler) {
        public static Action m_volatileHelpAction = () => {
            bool zh = ModsManager.Configs["Language"]?.StartsWith("zh") ?? false;
            WebBrowserManager.LaunchBrowser($"https://xiaofengdizhu.github.io/GigavoltDoc/{(zh ? "zh" : "en")}/expand/memory_banks/volatile_memory_banks.html#{(zh ? "易失性一维存储器" : "volatile-list-memory-bank")}");
        };

        public override Action HelpAction => m_volatileHelpAction;
    }
}