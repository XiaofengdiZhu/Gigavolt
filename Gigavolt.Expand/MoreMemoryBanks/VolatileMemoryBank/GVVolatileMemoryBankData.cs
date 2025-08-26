﻿using System;
using System.Globalization;

namespace Game {
    public class GVVolatileMemoryBankData : GVMemoryBankData {
        public GVVolatileMemoryBankData() {
            ID = GVStaticStorage.GetUniqueGVMBID();
            m_worldDirectory = null;
            m_data = [];
            m_isDataInitialized = true;
            m_updateTime = DateTime.Now;
        }

        public GVVolatileMemoryBankData(uint ID, uint[] image = null, uint width = 0, uint height = 0) {
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

        public override void Write(uint index, uint data) {
            int intIndex = MathUint.ToIntWithClamp(index);
            if (m_isDataInitialized && intIndex < Data.Length) {
                Data[intIndex] = data;
                m_updateTime = DateTime.Now;
                m_dataChanged = true;
            }
        }

        public override void Write(uint col, uint row, uint data) {
            if (m_isDataInitialized) {
                if (col >= m_width) {
                    uint[] newData = new uint[(col + 1) * m_height];
                    for (int y = 0; y < m_height; y++) {
                        Array.Copy(Data, y * m_width, newData, y * col, m_width);
                    }
                    Data = newData;
                    m_width = col + 1;
                }
                if (row >= m_height) {
                    uint[] newData = new uint[m_width * (row + 1)];
                    Array.Copy(Data, newData, Data.Length);
                    Data = newData;
                    m_height = row + 1;
                }
            }
            else {
                Data = new uint[(col + 1) * (row + 1)];
                m_width = col + 1;
                m_height = row + 1;
            }
            Write(row * m_width + col, data);
        }

        public override IEditableItemData Copy() => new GVVolatileMemoryBankData(
            GVStaticStorage.GetUniqueGVMBID(),
            m_isDataInitialized ? (uint[])Data.Clone() : null,
            m_width,
            m_height
        );

        public override IEditableItemData Copy(uint id) => new GVVolatileMemoryBankData(
            id,
            m_isDataInitialized ? (uint[])Data.Clone() : null,
            m_width,
            m_height
        );

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