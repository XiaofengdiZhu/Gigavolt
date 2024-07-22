using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using Engine;
using Engine.Media;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using Image = Engine.Media.Image;

namespace Game {
    public class GVMemoryBankData : GVArrayData, IEditableItemData {
        public uint[] m_data;
        public uint m_width;
        public uint m_height;
        public bool m_dataChanged;

        public static readonly PngEncoder DefaultPngEncoder = new() { ColorType = PngColorType.RgbWithAlpha, BitDepth = PngBitDepth.Bit8, TransparentColorMode = PngTransparentColorMode.Preserve };

        public uint[] Data {
            get => m_data;
            set {
                if (value != m_data) {
                    m_data = value;
                    m_updateTime = DateTime.Now;
                    if (m_isDataInitialized) {
                        m_dataChanged = true;
                    }
                    m_isDataInitialized = value != null;
                }
            }
        }

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
            int intIndex = MathUint.ToIntWithClamp(index);
            if (m_isDataInitialized && intIndex < Data.Length) {
                return Data[intIndex];
            }
            return 0u;
        }

        public virtual uint Read(uint col, uint row) {
            if (m_isDataInitialized
                && col < m_width
                && row < m_height) {
                return Data[row * m_width + col];
            }
            return 0;
        }

        public override void Write(uint index, uint data) {
            int intIndex = MathUint.ToIntWithClamp(index);
            if (m_isDataInitialized && intIndex < Data.Length) {
                Data[intIndex] = data;
                m_updateTime = DateTime.Now;
                m_dataChanged = true;
            }
        }

        public virtual void Write(uint col, uint row, uint data) {
            if (m_isDataInitialized
                && col < m_width
                && row < m_height) {
                Write(row * m_width + col, data);
            }
        }

        public override IEditableItemData Copy() => new GVMemoryBankData(
            GVStaticStorage.GetUniqueGVMBID(),
            m_worldDirectory,
            m_isDataInitialized ? (uint[])Data.Clone() : null,
            m_width,
            m_height
        );

        public override void LoadData() {
            if (m_worldDirectory != null) {
                try {
                    string path = $"{m_worldDirectory}/GVMB/{m_ID.ToString("X", null)}.png";
                    if (Storage.FileExists(path)) {
                        Image2Data(Image.Load(path, ImageFileFormat.Png));
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
            StringBuilder stringBuilder = new();
            stringBuilder.Append(m_ID.ToString("X", null));
            if (saveLastOutput) {
                stringBuilder.Append(';');
                stringBuilder.Append(LastOutput.ToString("X", null));
            }
            if (m_isDataInitialized && m_dataChanged) {
                try {
                    using (Stream stream = Storage.OpenFile($"{m_worldDirectory}/GVMB/{m_ID.ToString("X", null)}.png", OpenFileMode.CreateOrOpen)) {
                        GetImage().m_trueImage.SaveAsPng(stream, DefaultPngEncoder);
                        stream.Flush();
                        stream.Close();
                        m_dataChanged = false;
                    }
                }
                catch (Exception ex) {
                    Log.Error(ex);
                }
            }
            return stringBuilder.ToString();
        }

        public static uint[] String2UintArray(string data, ref int width, ref int height) {
            List<uint[]> rowList = new();
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

        public override void String2Data(string str, int width = 0, int height = 0) {
            int w = width;
            int h = height;
            uint[] image = String2UintArray(str, ref w, ref h);
            Data = image ?? throw new Exception("该文本无法转换为指定数据");
            m_dataChanged = true;
            m_width = (uint)w;
            m_height = (uint)h;
        }

        public static string Image2String(Image image) {
            string[] result = new string[image.Height];
            for (int i = 0; i < image.Height; i++) {
                int lastNotZero = -1;
                for (int j = image.Width - 1; j >= 0; j--) {
                    if (image.GetPixelFast(j, i).PackedValue > 0) {
                        lastNotZero = j;
                        break;
                    }
                }
                StringBuilder stringBuilder = new();
                for (int j = 0; j < lastNotZero; j++) {
                    stringBuilder.Append(image.GetPixelFast(j, i).PackedValue.ToString("X", null));
                    stringBuilder.Append(',');
                }
                if (lastNotZero > -1) {
                    stringBuilder.Append(image.GetPixelFast(lastNotZero, i).PackedValue.ToString("X", null));
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
                StringBuilder stringBuilder = new();
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

        public override string Data2String() => m_isDataInitialized ? UintArray2String(Data, m_width, m_height) : null;

        public static byte[] UintArray2Bytes(uint[] array, int startIndex = 0, int length = int.MaxValue) {
            byte[] bytes = new byte[array.Length * 4];
            for (int i = startIndex; i < MathUtils.Min(array.Length, length); i++) {
                uint num = array[i];
                bytes[i * 4 + 3] = (byte)(num & 0xFF);
                bytes[i * 4 + 2] = (byte)((num >> 8) & 0xFF);
                bytes[i * 4 + 1] = (byte)((num >> 16) & 0xFF);
                bytes[i * 4] = (byte)(num >> 24);
            }
            return bytes;
        }

        public override byte[] Data2Bytes(int startIndex = 0, int length = int.MaxValue) => m_isDataInitialized ? UintArray2Bytes(Data, startIndex, length) : null;

        public static short[] UintArray2Shorts(uint[] array) {
            short[] shorts = new short[array.Length * 2];
            for (int i = 0; i < array.Length; i++) {
                uint num = array[i];
                shorts[i * 2 + 1] = (short)(num & 0xFFFF);
                shorts[i * 2] = (short)(num >> 16);
            }
            return shorts;
        }

        public override short[] Data2Shorts() => m_isDataInitialized ? UintArray2Shorts(Data) : null;

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
            m_height = m_width;
        }

        public static Image UintArray2Image(uint[] array, uint width = 0, uint height = 0) {
            if (array.Length == 0) {
                return null;
            }
            Image image = new(width == 0 ? array.Length : (int)width, height == 0 ? 1 : (int)height);
            int maxHeight = array.Length / image.Width + (array.Length % image.Width == 0u ? 0 : 1);
            image.ProcessPixelRows(
                accessor => {
                    Span<uint> arraySpan = array.AsSpan();
                    for (int y = 0; y < maxHeight; y++) {
                        MemoryMarshal.Cast<uint, Rgba32>(arraySpan.Slice(y * image.Width, y == maxHeight - 1 ? array.Length - y * image.Width : image.Width)).CopyTo(accessor.GetRowSpan(y));
                    }
                }
            );
            return image;
        }

        public override Image Data2Image() => m_isDataInitialized ? UintArray2Image(Data, m_width, m_height) : null;

        public static uint[] Image2UintArray(Image image) {
            uint[] pixels = new uint[image.Width * image.Height];
            image.ProcessPixelRows(
                accessor => {
                    Span<uint> pixelsSpan = pixels.AsSpan();
                    for (int y = 0; y < accessor.Height; y++) {
                        MemoryMarshal.Cast<Rgba32, uint>(accessor.GetRowSpan(y)).CopyTo(pixelsSpan.Slice(y * image.Width, image.Width));
                    }
                },
                false
            );
            return pixels;
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

        public override MemoryStream Data2Stream() {
            if (m_isDataInitialized) {
                MemoryStream stream = new();
                Image.Save(GetImage(), stream, ImageFileFormat.Png, true);
                return stream;
            }
            return null;
        }

        public override string Stream2Data(Stream stream, string extension = "") {
            Data = Stream2UintArray(stream, out m_width);
            m_height = m_width;
            return string.Empty;
        }

        public override void UintArray2Data(uint[] uints, int width = 0, int height = 0) {
            m_width = (uint)width;
            m_height = (uint)height;
            Data = uints;
        }

        public override uint[] Data2UintArray() => m_isDataInitialized ? (uint[])m_data.Clone() : null;
    }
}