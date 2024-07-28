using System;
using System.Collections.Generic;
using System.Globalization;

namespace Game {
    public class GVVolatileListMemoryBankData : GVListMemoryBankData {
        public GVVolatileListMemoryBankData() {
            ID = GVStaticStorage.GetUniqueGVMBID();
            m_worldDirectory = null;
            m_data = [];
            m_isDataInitialized = true;
            m_updateTime = DateTime.Now;
        }

        public GVVolatileListMemoryBankData(uint ID, List<uint> image = null, uint width = 0, uint height = 0) {
            this.ID = ID;
            m_data = image;
            m_width = width;
            m_height = height;
            m_isDataInitialized = image != null;
            m_updateTime = DateTime.Now;
        }

        public override uint LastOutput {
            get => 0u;
            set { }
        }

        public override IEditableItemData Copy() => Copy(GVStaticStorage.GetUniqueGVMBID());

        public override IEditableItemData Copy(uint id) => new GVVolatileListMemoryBankData(id, m_isDataInitialized ? new List<uint>(Data) : null, m_width, m_height);

        public override void LoadString(string data) {
            string[] array = data.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (array.Length >= 1) {
                string text = array[0];
                ID = uint.Parse(text, NumberStyles.HexNumber, null);
            }
        }

        public override string SaveString() => ID.ToString("X", null);
    }
}