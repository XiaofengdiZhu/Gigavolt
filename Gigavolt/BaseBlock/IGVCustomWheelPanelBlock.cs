using System.Collections.Generic;
using System.Linq;
using GameEntitySystem;

namespace Game {
    public interface IGVCustomWheelPanelBlock {
        public List<int> GetCustomWheelPanelValues(int centerValue) => BlocksManager.Blocks[Terrain.ExtractContents(centerValue)].GetCreativeValues().ToList();
        public int GetCustomCopyBlock(Project project, int centerValue) => centerValue;

        public static readonly List<int> BasicElementsValues = [
            GVNotGateBlock.Index,
            GVAndGateBlock.Index,
            GVOrGateBlock.Index,
            GVXorGateBlock.Index,
            GVNandGateBlock.Index,
            GVNorGateBlock.Index,
            GVDelayGateBlock.Index
        ];

        public static readonly List<int> WireThroughValues = [
            GVWireThroughPlanksBlock.Index,
            GVWireThroughStoneBlock.Index,
            GVWireThroughBricksBlock.Index,
            GVWireThroughSemiconductorBlock.Index,
            GVWireThroughCobblestoneBlock.Index
        ];

        public static readonly List<int> TransformerValues = [GV2OTransformerBlock.Index, O2GVTransformerBlock.Index];

        public static readonly List<int> MemoryBankValues = [GVMemoryBankBlock.Index];
        public static readonly List<int> LedValues = new List<int> { GVMulticoloredLedBlock.Index, GV8NumberLedBlock.Index, GVOneLedBlock.Index }.Concat(BlocksManager.Blocks[GV8x4LedBlock.Index].GetCreativeValues()).ToList();
    }
}