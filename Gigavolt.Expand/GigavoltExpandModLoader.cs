using GameEntitySystem;

namespace Game {
    public class GigavoltExpandModLoader : ModLoader {
        public override void __ModInitialize() {
            ModsManager.RegisterHook("OnProjectLoaded", this);
        }

        public override void OnProjectLoaded(Project project) {
            IGVCustomWheelPanelBlock.MemoryBankValues.AddRange(
                [
                    GVBlocksManager.GetBlockIndex<GVVolatileMemoryBankBlock>(),
                    GVBlocksManager.GetBlockIndex<GVListMemoryBankBlock>(),
                    GVBlocksManager.GetBlockIndex<GVVolatileListMemoryBankBlock>(),
                    GVBlocksManager.GetBlockIndex<GVFourDimensionalMemoryBankBlock>(),
                    GVBlocksManager.GetBlockIndex<GVVolatileFourDimensionalMemoryBankBlock>()
                ]
            );
            IGVCustomWheelPanelBlock.LedValues.AddRange(GVBlocksManager.GetBlock<GVDisplayLedBlock>().GetCreativeValues());
            IGVCustomWheelPanelBlock.LedValues.AddRange([GVBlocksManager.GetBlockIndex<GVOscilloscopeBlock>(), GVBlocksManager.GetBlockIndex<GVSolid8NumberLedBlock>()]);
        }
    }
}