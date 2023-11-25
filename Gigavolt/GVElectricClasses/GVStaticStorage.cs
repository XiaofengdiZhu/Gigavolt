using System.Collections.Generic;
using Engine;

namespace Game {
    public static class GVStaticStorage {
        public static Dictionary<uint, GVArrayData> GVMBIDDataDictionary = new();

        public static uint GetUniqueGVMBID() {
            Random random = new();
            while (true) {
                uint num = random.UInt();
                if (num == 0u
                    || GVMBIDDataDictionary.ContainsKey(num)) {
                    continue;
                }
                return num;
            }
        }

        public static List<SoundGeneratorGVElectricElement> GVSGCFEEList = new();

        public static bool PreventChunkFromBeingFree = false;
        public static HashSet<Point2> GVUsingChunks = new();
        public static bool DisplayVoltage = false;
    }
}