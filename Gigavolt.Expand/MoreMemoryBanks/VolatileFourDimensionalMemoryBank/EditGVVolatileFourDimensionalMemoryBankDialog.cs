using System;

namespace Game {
    public class EditGVVolatileFourDimensionalMemoryBankDialog(GVVolatileFourDimensionalMemoryBankData memoryBankData, Action handler) : EditGVFourDimensionalMemoryBankDialog(memoryBankData, handler) {
        public static Action m_volatileHelpAction = () => {
            bool zh = ModsManager.Configs["Language"]?.StartsWith("zh") ?? false;
            WebBrowserManager.LaunchBrowser($"https://xiaofengdizhu.github.io/GigavoltDoc/{(zh ? "zh" : "en")}/expand/memory_banks/volatile_memory_banks.html#{(zh ? "易失性四维存储器" : "volatile-four-dimensional-memory-bank")}");
        };

        public override Action HelpAction => m_volatileHelpAction;
    }
}