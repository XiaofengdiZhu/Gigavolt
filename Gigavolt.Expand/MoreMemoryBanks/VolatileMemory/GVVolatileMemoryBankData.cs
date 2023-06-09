using System;
using System.Globalization;

namespace Game {
    public class GVVolatileMemoryBankData : GVMemoryBankData {
        public GVVolatileMemoryBankData() {
            m_ID = GVStaticStorage.GetUniqueGVMBID();
            m_worldDirectory = null;
            m_data = null;
            m_isDataInitialized = false;
            m_updateTime = DateTime.Now;
        }

        public GVVolatileMemoryBankData(uint ID, uint[] image = null) {
            m_ID = ID;
            m_data = image;
            m_isDataInitialized = image != null;
            if (!GVStaticStorage.GVMBIDDataDictionary.ContainsKey(m_ID)) {
                GVStaticStorage.GVMBIDDataDictionary.Add(m_ID, this);
            }
            m_updateTime = DateTime.Now;
        }

        public override uint LastOutput {
            get => 0u;
            set { }
        }

        public override void LoadString(string data) {
            string[] array = data.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (array.Length >= 1) {
                string text = array[0];
                m_ID = uint.Parse(text, NumberStyles.HexNumber, null);
                GVStaticStorage.GVMBIDDataDictionary.Add(m_ID, this);
            }
        }

        public override string SaveString() => m_ID.ToString("X", null);
    }
}