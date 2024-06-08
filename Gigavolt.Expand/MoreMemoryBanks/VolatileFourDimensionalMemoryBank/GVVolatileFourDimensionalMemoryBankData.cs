using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Game {
    public class GVVolatileFourDimensionalMemoryBankData : GVFourDimensionalMemoryBankData {
        public GVVolatileFourDimensionalMemoryBankData() {
            m_ID = GVStaticStorage.GetUniqueGVMBID();
            m_worldDirectory = null;
            m_data = null;
            m_isDataInitialized = false;
            m_updateTime = DateTime.Now;
        }

        public GVVolatileFourDimensionalMemoryBankData(uint ID, Dictionary<int, Image<Rgba32>> image = null, int xLength = 0, int yLength = 0, int zLength = 0, int wLength = 0) {
            m_ID = ID;
            m_data = image;
            m_xLength = xLength;
            m_yLength = yLength;
            m_xyProduct = xLength * yLength;
            m_zLength = zLength;
            m_xyzProduct = m_xyProduct * zLength;
            m_wLength = wLength;
            m_totalLength = m_xyzProduct * wLength;
            m_isDataInitialized = image != null;
            GVStaticStorage.GVMBIDDataDictionary[m_ID] = this;
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
                GVStaticStorage.GVMBIDDataDictionary[m_ID] = this;
            }
            if (array.Length >= 2) {
                string[] array2 = array[1].Split(',');
                if (array2.Length == 10) {
                    m_xLength = int.Parse(array2[0]);
                    m_yLength = int.Parse(array2[1]);
                    m_zLength = int.Parse(array2[2]);
                    m_wLength = int.Parse(array2[3]);
                    m_xOffset = int.Parse(array2[4]);
                    m_yOffset = int.Parse(array2[5]);
                    m_zOffset = int.Parse(array2[6]);
                    m_wOffset = int.Parse(array2[7]);
                    m_xSize = int.Parse(array2[8]);
                    m_ySize = int.Parse(array2[9]);
                }
            }
        }

        public override string SaveString() {
            StringBuilder stringBuilder = new();
            stringBuilder.Append(m_ID.ToString("X"));
            stringBuilder.Append($";{m_xLength},{m_yLength},{m_zLength},{m_wLength},{m_xOffset},{m_yOffset},{m_zOffset},{m_wOffset},{m_xSize},{m_ySize}");
            return stringBuilder.ToString();
        }
    }
}