using Engine;
using System.Collections.Generic;

namespace Game
{
    public static class GVStaticStorage
    {
        public static Dictionary<uint, GVMemoryBankData> GVMBIDDataDictionary = new Dictionary<uint, GVMemoryBankData>();
        public static uint GetUniqueGVMBID()
        {
            Random random = new Random();
            while (true)
            {
                uint num = random.UInt();
                if (GVMBIDDataDictionary.ContainsKey(num))
                {
                    continue;
                }
                else
                {
                    return num;
                }
            }
        }
    }
}
