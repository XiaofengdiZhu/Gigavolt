using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Engine;
using Engine.Media;

namespace Game {
    public class GVMemoryBankData : GVArrayData, IEditableItemData {
        public uint[] m_data;
        public uint m_width;
        public uint m_height;

        public uint[] Data {
            get => m_data;
            set {
                m_updateTime = DateTime.Now;
                m_dataChanged = true;
                m_data = value;
                m_isDataInitialized = value != null;
            }
        }

        public bool m_dataChanged;

        public GVMemoryBankData() {
            m_ID = GVStaticStorage.GetUniqueGVMBID();
            m_worldDirectory = null;
            m_data = null;
            m_isDataInitialized = false;
            m_updateTime = DateTime.Now;
        }

        public GVMemoryBankData(uint ID, string worldDirectory, uint[] image = null, uint width = 0, uint height = 0, uint lastOutput = 0) {
            m_ID = ID;
            m_worldDirectory = worldDirectory;
            m_data = image;
            m_width = width;
            m_height = height;
            m_isDataInitialized = image != null;
            LastOutput = lastOutput;
            GVStaticStorage.GVMBIDDataDictionary[m_ID] = this;
            m_updateTime = DateTime.Now;
        }

        public virtual uint LastOutput { get; set; }

        public override uint Read(uint index) {
            if (m_isDataInitialized && index < Data.Length) {
                return Data[index];
            }
            return 0u;
        }

        public uint Read(uint col, uint row) {
            if (m_isDataInitialized
                && col < m_width
                && row < m_height) {
                return Data[row * m_width + col];
            }
            return 0;
        }

        public override void Write(uint index, uint data) {
            if (m_isDataInitialized && index < Data.Length) {
                Data[index] = data;
                m_updateTime = DateTime.Now;
                m_dataChanged = true;
            }
        }

        public void Write(uint col, uint row, uint data) {
            if (m_isDataInitialized
                && col < m_width
                && row < m_height) {
                Write(row * m_width + col, data);
            }
        }

        public override IEditableItemData Copy() => new GVMemoryBankData(
            m_ID,
            m_worldDirectory,
            m_isDataInitialized ? (uint[])Data.Clone() : null,
            m_width,
            m_height,
            LastOutput
        );

        public override void LoadData() {
            if (m_worldDirectory != null) {
                try {
                    Image2Data(Image.Load($"{m_worldDirectory}/GVMB/{m_ID.ToString("X", null)}.png", ImageFileFormat.Png));
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
                    Image.Save(GetImage(), $"{m_worldDirectory}/GVMB/{m_ID.ToString("X", null)}.png", ImageFileFormat.Png, true);
                    m_dataChanged = false;
                }
                catch (Exception ex) {
                    Log.Error(ex);
                }
            }
            return stringBuilder.ToString();
        }

        public static Image String2Image(string data, int width = 0, int height = 0) {
            List<uint[]> rowList = new List<uint[]>();
            int maxColLength = 0;
            string[] rows = data.Split(';');
            foreach (string row in rows) {
                string[] cols = row.Split(',');
                if (cols.Length > maxColLength) {
                    maxColLength = cols.Length;
                }
                uint[] uints = new uint[cols.Length];
                for (int i = 0; i < cols.Length; i++) {
                    if (cols[i].Length > 0) {
                        uints[i] = uint.Parse(cols[i], NumberStyles.HexNumber, null);
                    }
                }
                rowList.Add(uints);
            }
            Image image = new Image(width == 0 ? maxColLength : width, height == 0 ? rows.Length : height);
            for (int i = 0; i < image.Height; i++) {
                if (i == rowList.Count) {
                    break;
                }
                for (int j = 0; j < rowList[i].Length; j++) {
                    if (j == image.Width) {
                        break;
                    }
                    image.SetPixel(j, i, new Color(rowList[i][j]));
                }
            }
            return image;
        }

        public static uint[] String2UintArray(string data, ref int width, ref int height) {
            List<uint[]> rowList = new List<uint[]>();
            int maxColLength = 0;
            string[] rows = data.Split(';');
            foreach (string row in rows) {
                string[] cols = row.Split(',');
                if (cols.Length > maxColLength) {
                    maxColLength = cols.Length;
                }
                uint[] uints = new uint[cols.Length];
                for (int i = 0; i < cols.Length; i++) {
                    if (cols[i].Length > 0) {
                        uints[i] = uint.Parse(cols[i], NumberStyles.HexNumber, null);
                    }
                }
                rowList.Add(uints);
            }
            width = width == 0 ? maxColLength : width;
            height = height == 0 ? rows.Length : height;
            uint[] image = new uint[width * height];
            for (int i = 0; i < height; i++) {
                if (i == rowList.Count) {
                    break;
                }
                for (int j = 0; j < rowList[i].Length; j++) {
                    if (j == width) {
                        break;
                    }
                    image[i * width + j] = rowList[i][j];
                }
            }
            return image;
        }

        public override void String2Data(string data, int width = 0, int height = 0) {
            int w = width;
            int h = height;
            uint[] image = String2UintArray(data, ref w, ref h);
            if (image == null) {
                throw new Exception("该文本无法转换为指定数据");
            }
            Data = String2UintArray(data, ref w, ref h);
            m_width = (uint)w;
            m_height = (uint)h;
        }

        public static string Image2String(Image image) {
            string[] result = new string[image.Height];
            for (int i = 0; i < image.Height; i++) {
                int lastNotZero = -1;
                for (int j = image.Width - 1; j >= 0; j--) {
                    if (image.GetPixel(j, i).PackedValue > 0) {
                        lastNotZero = j;
                        break;
                    }
                }
                StringBuilder stringBuilder = new StringBuilder();
                for (int j = 0; j < lastNotZero; j++) {
                    stringBuilder.Append(image.GetPixel(j, i).PackedValue.ToString("X", null));
                    stringBuilder.Append(',');
                }
                if (lastNotZero > -1) {
                    stringBuilder.Append(image.GetPixel(lastNotZero, i).PackedValue.ToString("X", null));
                }
                result[i] = stringBuilder.ToString();
            }
            return string.Join(";", result);
        }

        public static string UintArray2String(uint[] array, uint width, uint height) {
            string[] result = new string[height];
            for (uint i = 0; i < height; i++) {
                uint lastNotZero = 0u;
                bool flag = false;
                for (uint j = width - 1; j != uint.MaxValue; j--) {
                    if (array[i * width + j] > 0) {
                        lastNotZero = j;
                        flag = true;
                        break;
                    }
                }
                StringBuilder stringBuilder = new StringBuilder();
                for (uint j = 0; flag && j < lastNotZero; j++) {
                    stringBuilder.Append(array[i * width + j].ToString("X", null));
                    stringBuilder.Append(',');
                }
                if (flag) {
                    stringBuilder.Append(array[i * width + lastNotZero].ToString("X", null));
                }
                result[i] = stringBuilder.ToString();
            }
            return string.Join(";", result);
        }

        public override string Data2String() => UintArray2String(Data, m_width, m_height);

        public static byte[] Image2Bytes(Image image, int startIndex = 0, int length = int.MaxValue) {
            byte[] bytes = new byte[image.Pixels.Length * 4];
            for (int i = startIndex; i < MathUtils.Min(image.Pixels.Length, length); i++) {
                bytes[i * 4 + 3] = (byte)(image.Pixels[i].PackedValue & 0xFF);
                bytes[i * 4 + 2] = (byte)((image.Pixels[i].PackedValue >> 8) & 0xFF);
                bytes[i * 4 + 1] = (byte)((image.Pixels[i].PackedValue >> 16) & 0xFF);
                bytes[i * 4] = (byte)((image.Pixels[i].PackedValue >> 24) & 0xFF);
            }
            return bytes;
        }

        public static byte[] UintArray2Bytes(uint[] array, int startIndex = 0, int length = int.MaxValue) {
            byte[] bytes = new byte[array.Length * 4];
            for (int i = startIndex; i < MathUtils.Min(array.Length, length); i++) {
                bytes[i * 4 + 3] = (byte)(array[i] & 0xFF);
                bytes[i * 4 + 2] = (byte)((array[i] >> 8) & 0xFF);
                bytes[i * 4 + 1] = (byte)((array[i] >> 16) & 0xFF);
                bytes[i * 4] = (byte)((array[i] >> 24) & 0xFF);
            }
            return bytes;
        }

        public override byte[] Data2Bytes(int startIndex = 0, int length = int.MaxValue) {
            return m_cachedBytes = UintArray2Bytes(Data, startIndex, length);
        }

        public static short[] Image2Shorts(Image image) {
            short[] shorts = new short[image.Pixels.Length * 2];
            for (int i = 0; i < image.Pixels.Length; i++) {
                shorts[i * 2 + 1] = (short)(image.Pixels[i].PackedValue & 0xFFFF);
                shorts[i * 2] = (short)((image.Pixels[i].PackedValue >> 16) & 0xFFFF);
            }
            return shorts;
        }

        public static short[] UintArray2Shorts(uint[] array) {
            short[] shorts = new short[array.Length * 2];
            for (int i = 0; i < array.Length; i++) {
                shorts[i * 2 + 1] = (short)(array[i] & 0xFFFF);
                shorts[i * 2] = (short)((array[i] >> 16) & 0xFFFF);
            }
            return shorts;
        }

        public override short[] Data2Shorts() => UintArray2Shorts(Data);

        public static uint[] Shorts2UintArray(short[] shorts, out uint width) {
            width = (uint)Math.Ceiling(Math.Sqrt(shorts.Length / 2 + 1));
            uint[] image = new uint[width * width];
            for (int i = 0; i < image.Length; i++) {
                if (i * 2 >= shorts.Length) {
                    break;
                }
                if (i * 2 == shorts.Length - 1) {
                    image[i] = (uint)(ushort)shorts[i * 2] << 16;
                }
                else {
                    image[i] = (ushort)shorts[i * 2 + 1] | ((uint)(ushort)shorts[i * 2] << 16);
                }
            }
            return image;
        }

        public override void Shorts2Data(short[] shorts) {
            Data = Shorts2UintArray(shorts, out m_width);
        }

        public static Image UintArray2Image(uint[] array, uint width = 0, uint height = 0) {
            Image image = new Image(width == 0 ? array.Length : (int)width, height == 0 ? 1 : (int)height);
            for (int i = 0; i < array.Length; i++) {
                image.Pixels[i].PackedValue = array[i];
            }
            return image;
        }

        public override Image Data2Image() => UintArray2Image(Data, m_width, m_height);

        public static uint[] Image2UintArray(Image image) {
            return image.Pixels.Select(color => color.PackedValue).ToArray();
        }

        public override void Image2Data(Image image) {
            Data = Image2UintArray(image);
            m_width = (uint)image.Width;
            m_height = (uint)image.Height;
        }

        public static uint[] Stream2UintArray(Stream stream, out uint width) {
            width = (uint)Math.Ceiling(Math.Sqrt(stream.Length / 4 + 1));
            uint[] image = new uint[width * width];
            for (int i = 0; i < stream.Length / 4 + 1; i++) {
                byte[] fourBytes = new byte[4];
                if (stream.Read(fourBytes, 0, 4) > 0) {
                    image[i] = fourBytes[3] | ((uint)fourBytes[2] << 8) | ((uint)fourBytes[1] << 16) | ((uint)fourBytes[0] << 24);
                }
            }
            return image;
        }

        public override void Stream2Data(Stream stream) {
            Data = Stream2UintArray(stream, out m_width);
        }
    }
}