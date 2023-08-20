using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Engine;
using Engine.Media;

namespace Game {
    public class GVListMemoryBankData : GVArrayData, IEditableItemData {
        public List<uint> m_data;
        public bool m_dataChanged;
        public uint m_width;
        public uint m_height;
        public uint m_offset;

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
            m_data = new List<uint>();
            m_isDataInitialized = true;
            m_updateTime = DateTime.Now;
        }

        public GVListMemoryBankData(uint ID, string worldDirectory, List<uint> image = null, uint lastOutput = 0) {
            m_ID = ID;
            m_worldDirectory = worldDirectory;
            m_data = image;
            m_isDataInitialized = image != null;
            GVStaticStorage.GVMBIDDataDictionary[m_ID] = this;
            m_updateTime = DateTime.Now;
            LastOutput = lastOutput;
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
                    string path = $"{m_worldDirectory}/GVLMB/{m_ID.ToString("X", null)}.bin";
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
            if (array.Length >= 5) {
                m_width = uint.Parse(array[1]);
                m_height = uint.Parse(array[2]);
                m_offset = uint.Parse(array[3]);
                LastOutput = uint.Parse(array[4], NumberStyles.HexNumber, null);
            }
        }

        public override string SaveString() => SaveString(true);

        public string SaveString(bool saveLastOutput) {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(m_ID.ToString("X", null));
            stringBuilder.Append($";{m_width};{m_height};{m_offset}");
            if (saveLastOutput) {
                stringBuilder.Append(';');
                stringBuilder.Append(LastOutput.ToString("X", null));
            }
            if (m_isDataInitialized && m_dataChanged) {
                try {
                    Stream stream = Storage.OpenFile($"{m_worldDirectory}/GVLMB/{m_ID.ToString("X", null)}.bin", OpenFileMode.CreateOrOpen);
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
            m_width = 0u;
            m_height = 0u;
            m_offset = 0u;
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

        public override byte[] Data2Bytes(int startIndex = 0, int length = int.MaxValue) => m_isDataInitialized ? UintList2Bytes(Data, startIndex, length) : null;

        public static short[] UintList2Shorts(List<uint> array) {
            short[] shorts = new short[array.Count * 2];
            for (int i = 0; i < array.Count; i++) {
                shorts[i * 2 + 1] = (short)(array[i] & 0xFFFF);
                shorts[i * 2] = (short)((array[i] >> 16) & 0xFFFF);
            }
            return shorts;
        }

        public override short[] Data2Shorts() => m_isDataInitialized ? UintList2Shorts(Data) : null;

        public override void String2Data(string str, int width = int.MaxValue, int _ = 0) {
            Data = String2UintList(str, width);
            m_width = 0u;
            m_height = 0u;
            m_offset = 0u;
        }

        public static List<uint> String2UintList(string str, int maxCount = int.MaxValue) {
            string[] strings = str.Split(',');
            return strings.Select(number => uint.Parse(number, NumberStyles.HexNumber, null)).ToList().GetRange(0, MathUtils.Min(strings.Length, maxCount));
        }

        public override string Data2String() => m_isDataInitialized ? UintList2String(Data) : null;

        public static string UintList2String(List<uint> array, int maxCount = int.MaxValue) {
            StringBuilder stringBuilder = new StringBuilder();
            maxCount = MathUtils.Min(array.Count, maxCount);
            for (int i = 0; i < maxCount - 1; i++) {
                stringBuilder.Append(array[i].ToString("X", null));
                stringBuilder.Append(',');
            }
            if (maxCount >= 1) {
                stringBuilder.Append(array[maxCount - 1].ToString("X", null));
            }
            return stringBuilder.ToString();
        }

        public static List<uint> Shorts2UintList(short[] shorts) {
            List<uint> image = new List<uint>(shorts.Length / 2 + 1);
            for (int i = 0; i < image.Count; i++) {
                if (i * 2 >= shorts.Length) {
                    break;
                }
                if (i * 2 == shorts.Length - 1) {
                    image.Add((uint)(ushort)shorts[i * 2] << 16);
                }
                else {
                    image.Add((ushort)shorts[i * 2 + 1] | ((uint)(ushort)shorts[i * 2] << 16));
                }
            }
            return image;
        }

        public override void Shorts2Data(short[] shorts) {
            Data = Shorts2UintList(shorts);
            m_width = 0u;
            m_height = 0u;
            m_offset = 0u;
        }

        public static Image UintList2Image(List<uint> list, uint width = 0u, uint height = 0u, uint offset = 0u) {
            if (width > 0
                && height > 0) {
                Image image = new Image(MathUint.ToInt(width), MathUint.ToInt(height));
                for (int i = (int)offset; i < Math.Min(list.Count, image.Pixels.Length + (int)offset); i++) {
                    image.Pixels[i].PackedValue = list[i];
                }
                return image;
            }
            return null;
        }

        public override Image Data2Image() => m_isDataInitialized ? UintList2Image(Data, m_width, m_height, m_offset) : null;

        public static List<uint> Image2UintList(Image image) {
            return image.Pixels.Select(color => color.PackedValue).ToList();
        }

        public override void Image2Data(Image image) {
            Data = Image2UintList(image);
            m_width = (uint)image.Width;
            m_height = (uint)image.Height;
            m_offset = 0u;
        }

        public override void UintArray2Data(uint[] uints, int width = 0, int height = 0) {
            m_width = (uint)width;
            m_height = (uint)height;
            m_offset = 0u;
            Data = uints.ToList();
        }

        public override uint[] Data2UintArray() => m_isDataInitialized ? m_data.ToArray() : null;
    }
}