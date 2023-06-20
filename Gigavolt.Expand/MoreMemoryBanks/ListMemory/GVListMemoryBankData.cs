using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Engine;

namespace Game {
    public class GVListMemoryBankData : GVArrayData, IEditableItemData {
        public List<uint> m_data;
        public bool m_dataChanged;

        public List<uint> Data {
            get => m_data;
            set {
                m_updateTime = DateTime.Now;
                m_dataChanged = true;
                m_data = value;
                m_isDataInitialized = value != null;
            }
        }

        public GVListMemoryBankData() {
            m_ID = GVStaticStorage.GetUniqueGVMBID();
            m_worldDirectory = null;
            m_data = null;
            m_isDataInitialized = false;
            m_updateTime = DateTime.Now;
        }

        public GVListMemoryBankData(uint ID, string worldDirectory, List<uint> image = null, uint lastOutput = 0) {
            m_ID = ID;
            m_worldDirectory = worldDirectory;
            m_data = image;
            m_isDataInitialized = image != null;
            LastOutput = lastOutput;
            GVStaticStorage.GVMBIDDataDictionary[m_ID] = this;
            m_updateTime = DateTime.Now;
        }

        public virtual uint LastOutput { get; set; }

        public override uint Read(uint index) {
            int intIndex = MathUint.ToInt(index);
            if (m_isDataInitialized && intIndex < Data.Count) {
                return Data[intIndex];
            }
            return 0u;
        }

        public override void Write(uint index, uint data) {
            if (m_isDataInitialized) {
                int intIndex = MathUint.ToInt(index);
                if (index < Data.Count) {
                    Data[intIndex] = data;
                }
                else {
                    Data.Capacity = intIndex + 1;
                    for (int i = Data.Count; i < intIndex; i++) {
                        Data.Add(0u);
                    }
                    Data.AddRange(Enumerable.Repeat(0u, intIndex - Data.Count));
                    Data.Add(data);
                }
                m_updateTime = DateTime.Now;
                m_dataChanged = true;
            }
        }

        public override IEditableItemData Copy() => new GVListMemoryBankData(m_ID, m_worldDirectory, m_isDataInitialized ? new List<uint>(Data) : null, LastOutput);

        public override void LoadData() {
            if (m_worldDirectory != null) {
                try {
                    string path = $"{m_worldDirectory}/GVMB/{m_ID.ToString("X", null)}.bin";
                    if (Storage.FileExists(path)) {
                        Stream2Data(Storage.OpenFile(path, OpenFileMode.Read));
                    }
                }
                catch (Exception ex) {
                    Log.Error(ex);
                }
            }
        }

        public override void LoadString(string data) {
            string[] array = data.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (array.Length >= 1) {
                string text = array[0];
                m_ID = uint.Parse(text, NumberStyles.HexNumber, null);
                LoadData();
                GVStaticStorage.GVMBIDDataDictionary[m_ID] = this;
            }
            if (array.Length >= 2) {
                LastOutput = uint.Parse(array[1], NumberStyles.HexNumber, null);
            }
        }

        public override string SaveString() => SaveString(true);

        public string SaveString(bool saveLastOutput) {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(m_ID.ToString("X", null));
            if (saveLastOutput) {
                stringBuilder.Append(';');
                stringBuilder.Append(LastOutput.ToString("X", null));
            }
            if (m_isDataInitialized && m_dataChanged) {
                try {
                    Stream stream = Storage.OpenFile($"{m_worldDirectory}/GVMB/{m_ID.ToString("X", null)}.bin", OpenFileMode.CreateOrOpen);
                    byte[] bytes = GetBytes();
                    stream.Write(bytes, 0, bytes.Length);
                    stream.Flush();
                    stream.Close();
                    m_dataChanged = false;
                }
                catch (Exception ex) {
                    Log.Error(ex);
                }
            }
            return stringBuilder.ToString();
        }

        public static List<uint> Stream2UintList(Stream stream) {
            List<uint> image = new List<uint>((int)(stream.Length / 4 + 1));
            for (int i = 0; i < stream.Length / 4 + 1; i++) {
                byte[] fourBytes = new byte[4];
                if (stream.Read(fourBytes, 0, 4) > 0) {
                    image.Add(fourBytes[3] | ((uint)fourBytes[2] << 8) | ((uint)fourBytes[1] << 16) | ((uint)fourBytes[0] << 24));
                }
            }
            return image;
        }

        public override void Stream2Data(Stream stream) {
            Data = Stream2UintList(stream);
        }

        public static byte[] UintList2Bytes(List<uint> array, int startIndex = 0, int length = int.MaxValue) {
            byte[] bytes = new byte[array.Count * 4];
            for (int i = startIndex; i < MathUtils.Min(array.Count, length); i++) {
                bytes[i * 4 + 3] = (byte)(array[i] & 0xFF);
                bytes[i * 4 + 2] = (byte)((array[i] >> 8) & 0xFF);
                bytes[i * 4 + 1] = (byte)((array[i] >> 16) & 0xFF);
                bytes[i * 4] = (byte)((array[i] >> 24) & 0xFF);
            }
            return bytes;
        }

        public override byte[] Data2Bytes(int startIndex = 0, int length = int.MaxValue) {
            return m_cachedBytes = UintList2Bytes(Data, startIndex, length);
        }

        public static short[] UintList2Shorts(List<uint> array) {
            short[] shorts = new short[array.Count * 2];
            for (int i = 0; i < array.Count; i++) {
                shorts[i * 2 + 1] = (short)(array[i] & 0xFFFF);
                shorts[i * 2] = (short)((array[i] >> 16) & 0xFFFF);
            }
            return shorts;
        }

        public override short[] Data2Shorts() => UintList2Shorts(Data);

        public override void String2Data(string str, int width = int.MaxValue, int _ = 0) {
            Data = String2UintList(str, width);
        }

        public static List<uint> String2UintList(string str, int maxCount = int.MaxValue) {
            string[] strings = str.Split(',');
            return strings.Select(number => uint.Parse(number, NumberStyles.HexNumber, null)).ToList().GetRange(0, MathUtils.Min(strings.Length, maxCount));
        }

        public override string Data2String() => UintList2String(Data);

        public static string UintList2String(List<uint> array, int maxCount = int.MaxValue) {
            StringBuilder stringBuilder = new StringBuilder();
            maxCount = MathUtils.Min(array.Count, maxCount);
            for (int i = 0; i < maxCount - 1; i++) {
                stringBuilder.Append(array[i].ToString("X", null));
                stringBuilder.Append(',');
            }
            stringBuilder.Append(array[maxCount - 1].ToString("X", null));
            return stringBuilder.ToString();
        }
    }
}