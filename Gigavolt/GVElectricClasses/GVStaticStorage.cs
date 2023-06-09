using System.Collections.Generic;

namespace Game {
    public static class GVStaticStorage {
        public static Dictionary<uint, GVArrayData> GVMBIDDataDictionary = new Dictionary<uint, GVArrayData>();

        public static uint GetUniqueGVMBID() {
            Random random = new Random();
            while (true) {
                uint num = random.UInt();
                if (num == 0u
                    || GVMBIDDataDictionary.ContainsKey(num)) {
                    continue;
                }
                return num;
            }
        }

        public static List<SoundGeneratorGVElectricElement> GVSGCFEEList = new List<SoundGeneratorGVElectricElement>();
    }
}