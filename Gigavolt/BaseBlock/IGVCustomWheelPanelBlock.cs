using System.Collections.Generic;
using System.Linq;
using GameEntitySystem;

namespace Game {
    public interface IGVCustomWheelPanelBlock {
        public List<int> GetCustomWheelPanelValues(int centerValue) =>
            BlocksManager.Blocks[Terrain.ExtractContents(centerValue)].GetCreativeValues().ToList();

        public int GetCustomCopyBlock(Project project, int centerValue) => centerValue;

        public static readonly List<int> BasicElementsValues = [];

        public static readonly List<int> WireThroughValues = [];

        public static readonly List<int> TransformerValues = [];

        public static readonly List<int> MemoryBankValues = [];
        public static readonly List<int> LedValues = [];
    }
}