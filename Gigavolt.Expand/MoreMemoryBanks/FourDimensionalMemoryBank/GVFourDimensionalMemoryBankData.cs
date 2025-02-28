using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Engine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using Image = Engine.Media.Image;

namespace Game {
    public class GVFourDimensionalMemoryBankData : GVArrayData, IEditableItemData {
        public Dictionary<int, Image<Rgba32>> m_data;
        public int m_xLength;
        public int m_yLength;
        public int m_xyProduct;
        public int m_zLength;
        public int m_xyzProduct;
        public int m_wLength;
        public int m_totalLength;
        public int m_xOffset;
        public int m_yOffset;
        public int m_zOffset;
        public int m_wOffset;
        public int m_xSize;
        public int m_ySize;
        public bool m_dataChanged;
        public static readonly Configuration DefaultImageConfiguration = new(new WebpConfigurationModule());
        public static readonly DecoderOptions DefaultImageDecoderOptions = new() { Configuration = DefaultImageConfiguration };
        public override string ExportExtension => ".GVFDMB";

        public Dictionary<int, Image<Rgba32>> Data {
            get => m_data;
            set {
                if (value != m_data) {
                    m_data = value;
                    m_updateTime = DateTime.Now;
                    if (m_isDataInitialized) {
                        m_dataChanged = true;
                        m_xOffset = 0;
                        m_yOffset = 0;
                        m_zOffset = 0;
                        m_wOffset = 0;
                        m_xSize = 0;
                        m_ySize = 0;
                    }
                    m_isDataInitialized = value != null;
                }
            }
        }

        public GVFourDimensionalMemoryBankData() {
            ID = GVStaticStorage.GetUniqueGVMBID();
            m_worldDirectory = null;
            m_data = null;
            m_isDataInitialized = false;
            m_updateTime = DateTime.Now;
        }

        public GVFourDimensionalMemoryBankData(uint ID, string worldDirectory, Dictionary<int, Image<Rgba32>> image = null, int xLength = 0, int yLength = 0, int zLength = 0, int wLength = 0, uint lastOutput = 0) {
            this.ID = ID;
            m_worldDirectory = worldDirectory;
            m_data = image;
            m_xLength = xLength;
            m_yLength = yLength;
            m_xyProduct = xLength * yLength;
            m_zLength = zLength;
            m_xyzProduct = m_xyProduct * zLength;
            m_wLength = wLength;
            m_totalLength = m_xyzProduct * wLength;
            m_isDataInitialized = image != null;
            LastOutput = lastOutput;
            m_updateTime = DateTime.Now;
        }

        public virtual uint LastOutput { get; set; }

        public override uint Read(uint index) {
            int intIndex = MathUint.ToIntWithClamp(index);
            if (m_isDataInitialized && intIndex < m_totalLength) {
                int w = intIndex / m_xyzProduct;
                if (Data.TryGetValue(w, out Image<Rgba32> image)) {
                    int wRemainder = intIndex % m_xyzProduct;
                    int z = wRemainder / m_xyProduct;
                    ImageFrame<Rgba32> frame = image.Frames[z];
                    int zRemainder = wRemainder % m_xyProduct;
                    int y = zRemainder / m_xLength;
                    int x = zRemainder % m_xLength;
                    return frame[x, y].PackedValue;
                }
            }
            return 0u;
        }

        public uint Read(int x, int y, int z, int w) {
            if (m_isDataInitialized
                && x < m_xLength
                && y < m_yLength
                && z < m_zLength
                && w < m_wLength
                && Data.TryGetValue(w, out Image<Rgba32> image)) {
                return image.Frames[z][x, y].PackedValue;
            }
            return 0;
        }

        public override void Write(uint index, uint data) {
            int intIndex = MathUint.ToIntWithClamp(index);
            if (m_isDataInitialized && intIndex < m_totalLength) {
                int w = intIndex / m_xyzProduct;
                int wRemainder = intIndex % m_xyzProduct;
                int z = wRemainder / m_xyProduct;
                int zRemainder = wRemainder % m_xyProduct;
                int y = zRemainder / m_xLength;
                int x = zRemainder % m_xLength;
                Write(
                    x,
                    y,
                    z,
                    w,
                    data
                );
            }
        }

        public void Write(int x, int y, int z, int w, uint data) {
            if (m_isDataInitialized
                && x < m_xLength
                && y < m_yLength
                && z < m_zLength
                && w < m_wLength) {
                if (Data.TryGetValue(w, out Image<Rgba32> image)
                    && image.Frames[z][x, y].PackedValue != data) {
                    image.Frames[z][x, y] = new Rgba32(data);
                    m_updateTime = DateTime.Now;
                    m_dataChanged = true;
                }
                else if (w < m_wLength) {
                    image = new Image<Rgba32>(DefaultImageConfiguration, m_xLength, m_yLength);
                    image.Metadata.GetWebpMetadata().FileFormat = WebpFileFormatType.Lossless;
                    while (image.Frames.Count < m_zLength) {
                        image.Frames.AddFrame(image.Frames.RootFrame);
                    }
                    image.Frames[z][x, y] = new Rgba32(data);
                    Data.Add(w, image);
                    m_updateTime = DateTime.Now;
                    m_dataChanged = true;
                }
            }
        }

        public override IEditableItemData Copy() => Copy(GVStaticStorage.GetUniqueGVMBID());

        public override IEditableItemData Copy(uint id) => new GVFourDimensionalMemoryBankData(
            id,
            m_worldDirectory,
            m_isDataInitialized ? Data.Select(pair => new KeyValuePair<int, Image<Rgba32>>(pair.Key, pair.Value.Clone())).ToDictionary() : null,
            m_xLength,
            m_yLength,
            m_zLength,
            m_wLength
        );

        public override void LoadData() {
            if (m_worldDirectory != null) {
                try {
                    string directory = $"{m_worldDirectory}/GVFDMB/{ID:X}";
                    if (Storage.DirectoryExists(directory)) {
                        List<string> files = Storage.ListFileNames(directory).ToList();
                        HashSet<int> hashSet = [];
                        foreach (int key in Data.Keys) {
                            string file = $"{key:X}.webp";
                            string path = $"{directory}/{file}";
                            if (Storage.FileExists(path)) {
                                files.Remove(file);
                                Image<Rgba32> image;
                                using (Stream stream = Storage.OpenFile(path, OpenFileMode.Read)) {
                                    image = SixLabors.ImageSharp.Image.Load<Rgba32>(DefaultImageDecoderOptions, stream);
                                    stream.Close();
                                }
                                if (image.Frames.Count == m_zLength
                                    && image.Width == m_xLength
                                    && image.Height == m_yLength) {
                                    Data[key] = image;
                                }
                                else {
                                    hashSet.Add(key);
                                    Storage.DeleteFile(path);
                                }
                            }
                            else {
                                hashSet.Add(key);
                            }
                        }
                        foreach (int key in hashSet) {
                            Data.Remove(key);
                        }
                        foreach (string file in files) {
                            Storage.DeleteFile($"{directory}/{file}");
                        }
                    }
                }
                catch (Exception ex) {
                    Log.Error(ex);
                }
            }
        }

        public override void LoadString(string data) {
            string[] array = data.Split(';');
            if (array.Length >= 1) {
                string text = array[0];
                ID = uint.Parse(text, NumberStyles.HexNumber, null);
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
            if (array.Length >= 3) {
                Data = new Dictionary<int, Image<Rgba32>>();
                string[] array2 = array[2].Split(',');
                foreach (string key in array2) {
                    if (int.TryParse(key, out int w)) {
                        Data.Add(w, null);
                    }
                }
                LoadData();
            }
            if (array.Length >= 4) {
                LastOutput = uint.Parse(array[3], NumberStyles.HexNumber, null);
            }
        }

        public override string SaveString() => SaveString(true);

        public string SaveString(bool saveLastOutput) {
            StringBuilder stringBuilder = new();
            stringBuilder.Append(ID.ToString("X"));
            stringBuilder.Append($";{m_xLength},{m_yLength},{m_zLength},{m_wLength},{m_xOffset},{m_yOffset},{m_zOffset},{m_wOffset},{m_xSize},{m_ySize};");
            stringBuilder.Append(string.Join(',', Data.Keys));
            if (saveLastOutput) {
                stringBuilder.Append(';');
                stringBuilder.Append(LastOutput.ToString("X"));
            }
            if (m_isDataInitialized && m_dataChanged) {
                string directory = $"{m_worldDirectory}/GVFDMB/{ID:X}";
                if (!Storage.DirectoryExists(directory)) {
                    Storage.CreateDirectory(directory);
                }
                List<string> files = Storage.ListFileNames(directory).ToList();
                try {
                    foreach (KeyValuePair<int, Image<Rgba32>> pair in Data) {
                        string path = $"{directory}/{pair.Key}.webp";
                        if (Storage.FileExists(path)) {
                            files.Remove(path);
                        }
                        using (Stream stream = Storage.OpenFile(path, OpenFileMode.CreateOrOpen)) {
                            pair.Value.SaveAsWebp(stream);
                            stream.Flush();
                            stream.Close();
                        }
                    }
                    foreach (string file in files) {
                        Storage.DeleteFile($"{directory}/{file}");
                    }
                    m_dataChanged = false;
                }
                catch (Exception ex) {
                    Log.Error(ex);
                }
            }
            return stringBuilder.ToString();
        }

        public void String2Data(string data, ref int xLength, ref int yLength, ref int zLength, ref int wLength) {
            Dictionary<int, List<List<List<uint>>>> result1 = new();
            int maxXLength = 0;
            int maxYLength = 0;
            int maxZLength = 0;
            int maxWLength = 0;
            string[] ws = data.Split('|');
            int wStop = Math.Min(ws.Length, wLength <= 0 ? int.MaxValue : wLength);
            for (int wIndex = 0; wIndex < wStop; wIndex++) {
                string w = ws[wIndex];
                if (w.Length > 0) {
                    List<List<List<uint>>> zList = new();
                    string[] zs = w.Split(':');
                    int zStop = Math.Min(zs.Length, zLength <= 0 ? int.MaxValue : zLength);
                    for (int zIndex = 0; zIndex < zStop; zIndex++) {
                        string z = zs[zIndex];
                        if (z.Length > 0) {
                            List<List<uint>> yList = new();
                            string[] ys = z.Split(';');
                            int yStop = Math.Min(ys.Length, yLength <= 0 ? int.MaxValue : yLength);
                            for (int yIndex = 0; yIndex < yStop; yIndex++) {
                                string y = ys[yIndex];
                                if (y.Length > 0) {
                                    List<uint> xList = new();
                                    string[] xs = y.Split(',');
                                    int xStop = Math.Min(xs.Length, xLength <= 0 ? int.MaxValue : xLength);
                                    for (int xIndex = 0; xIndex < xStop; xIndex++) {
                                        string x = xs[xIndex];
                                        if (x.Length > 0
                                            && uint.TryParse(x, NumberStyles.HexNumber, null, out uint value)) {
                                            xList.Add(value);
                                            maxXLength = Math.Max(maxXLength, xIndex + 1);
                                            maxYLength = Math.Max(maxYLength, yIndex + 1);
                                            maxZLength = Math.Max(maxZLength, zIndex + 1);
                                            maxWLength = Math.Max(maxWLength, wIndex + 1);
                                        }
                                        else {
                                            xList.Add(0);
                                        }
                                    }
                                    yList.Add(xList);
                                }
                                else {
                                    yList.Add(null);
                                }
                            }
                            zList.Add(yList);
                        }
                        else {
                            zList.Add(null);
                        }
                    }
                    result1.Add(wIndex, zList);
                }
            }
            Dictionary<int, Image<Rgba32>> result2 = new();
            int realXLength = Math.Max(maxXLength, xLength);
            int realYLength = Math.Max(maxYLength, yLength);
            int realZLength = Math.Max(maxZLength, zLength);
            int realWLength = Math.Max(maxWLength, wLength);
            if (maxXLength > 0
                && maxYLength > 0
                && maxZLength > 0
                && maxWLength > 0) {
                foreach (KeyValuePair<int, List<List<List<uint>>>> pair in result1) {
                    if (pair.Value.Count > 0) {
                        Image<Rgba32> image = new(DefaultImageConfiguration, realXLength, realYLength);
                        image.Metadata.GetWebpMetadata().FileFormat = WebpFileFormatType.Lossless;
                        while (image.Frames.Count < realZLength) {
                            image.Frames.AddFrame(image.Frames.RootFrame);
                        }
                        for (int zIndex = 0; zIndex < maxZLength; zIndex++) {
                            List<List<uint>> z = pair.Value[zIndex];
                            if (z == null) {
                                continue;
                            }
                            for (int yIndex = 0; yIndex < maxYLength; yIndex++) {
                                List<uint> y = z[yIndex];
                                if (y == null) {
                                    continue;
                                }
                                for (int xIndex = 0; xIndex < maxXLength; xIndex++) {
                                    uint x = y[xIndex];
                                    if (x == 0) {
                                        continue;
                                    }
                                    image.Frames[zIndex][xIndex, yIndex] = new Rgba32(x);
                                }
                            }
                        }
                        result2.Add(pair.Key, image);
                    }
                }
            }
            Data = result2;
            m_dataChanged = true;
            m_xLength = realXLength;
            m_yLength = realYLength;
            m_xyProduct = maxXLength * realYLength;
            m_zLength = realZLength;
            m_xyzProduct = m_xyProduct * realZLength;
            m_wLength = realWLength;
            m_totalLength = m_xyzProduct * realWLength;
        }

        public override string Data2String() {
            if (!m_isDataInitialized) {
                return null;
            }
            string[] result = new string[m_wLength];
            for (int wIndex = 0; wIndex < m_wLength; wIndex++) {
                if (Data.TryGetValue(wIndex, out Image<Rgba32> image)) {
                    string[] result2 = new string[image.Frames.Count];
                    for (int zIndex = 0; zIndex < image.Frames.Count; zIndex++) {
                        string[] result3 = new string[image.Height];
                        ImageFrame<Rgba32> frame = image.Frames[zIndex];
                        for (int yIndex = 0; yIndex < image.Height; yIndex++) {
                            int lastNotZero = -1;
                            for (int j = image.Width - 1; j >= 0; j--) {
                                if (frame[j, yIndex].PackedValue > 0) {
                                    lastNotZero = j;
                                    break;
                                }
                            }
                            StringBuilder stringBuilder = new();
                            for (int j = 0; j < lastNotZero; j++) {
                                stringBuilder.Append(frame[j, yIndex].PackedValue.ToString("X"));
                                stringBuilder.Append(',');
                            }
                            if (lastNotZero > -1) {
                                stringBuilder.Append(frame[lastNotZero, yIndex].PackedValue.ToString("X"));
                            }
                            result3[yIndex] = stringBuilder.ToString();
                        }
                        result2[zIndex] = string.Join(";", result3);
                    }
                    result[wIndex] = string.Join(":", result2);
                }
                else {
                    result[wIndex] = "";
                }
            }
            return string.Join("|", result);
        }

        public override byte[] Data2Bytes(int startIndex = 0, int length = int.MaxValue) {
            List<uint> list = Data2UintList();
            if (list == null) {
                return null;
            }
            byte[] bytes = new byte[list.Count * 4];
            for (int i = 0; i < list.Count; i++) {
                uint num = list[i];
                bytes[i * 4] = (byte)(num & 0xFF);
                bytes[i * 4 + 1] = (byte)((num >> 8) & 0xFF);
                bytes[i * 4 + 2] = (byte)((num >> 16) & 0xFF);
                bytes[i * 4 + 3] = (byte)((num >> 24) & 0xFF);
            }
            return bytes;
        }

        public override short[] Data2Shorts() {
            List<uint> list = Data2UintList();
            if (list == null) {
                return null;
            }
            short[] shorts = new short[list.Count * 2];
            for (int i = 0; i < list.Count; i++) {
                uint num = list[i];
                shorts[i * 2 + 1] = (short)(num & 0xFFFF);
                shorts[i * 2] = (short)((num >> 16) & 0xFFFF);
            }
            return shorts;
        }

        public override void Shorts2Data(short[] shorts) {
            UintList2Data(GVListMemoryBankData.Shorts2UintList(shorts));
        }

        public override Image Data2Image() {
            if (m_isDataInitialized
                && m_xSize > 0
                && m_ySize > 0
                && m_xOffset + m_xSize <= m_xLength
                && m_yOffset + m_ySize <= m_yLength
                && m_zOffset < m_zLength
                && m_wOffset < m_wLength
                && Data.TryGetValue(m_wOffset, out Image<Rgba32> image)) {
                Image newImage;
                if (m_xOffset == 0
                    && m_yOffset == 0
                    && m_xSize == m_xLength
                    && m_ySize == m_yLength) {
                    newImage = new Image(image.Frames.CloneFrame(m_zOffset));
                }
                else {
                    newImage = new Image(m_xSize, m_ySize);
                    for (int y = m_yOffset; y < m_yOffset + m_ySize; y++) {
                        for (int x = m_xOffset; x < m_xOffset + m_xSize; x++) {
                            newImage.SetPixelFast(x, y, new Rgba32(image.Frames[m_zOffset][x, y].PackedValue));
                        }
                    }
                }
                m_cachedImage?.m_trueImage.Dispose();
                return newImage;
            }
            return null;
        }

        public override void Image2Data(Image image) {
            m_xLength = image.Width;
            m_yLength = image.Height;
            Data = new Dictionary<int, Image<Rgba32>> { { 0, image.m_trueImage.Clone() } };
            m_xyProduct = m_xLength * m_yLength;
            m_zLength = image.m_trueImage.Frames.Count;
            m_xyzProduct = m_xyProduct * m_zLength;
            m_wLength = 1;
            m_totalLength = m_xyProduct;
        }

        public override MemoryStream Data2Stream() {
            if (m_isDataInitialized) {
                string comment = SaveString();
                MemoryStream stream = new();
                using (ZipArchive zip = ZipArchive.Create(stream, true)) {
                    string directory = $"{m_worldDirectory}/GVFDMB/{ID:X}";
                    List<string> files = Storage.ListFileNames(directory).ToList();
                    foreach (string file in files) {
                        using (Stream stream2 = Storage.OpenFile($"{directory}/{file}", OpenFileMode.Read)) {
                            zip.AddStream(file, stream2);
                        }
                    }
                    zip.Comment = comment;
                }
                return stream;
            }
            return null;
        }

        public override string Stream2Data(Stream stream, string extension = "") {
            if (extension.ToLower() == ".gvfdmb") {
                Dictionary<int, Image<Rgba32>> newData = new();
                string comment;
                using (ZipArchive zip = ZipArchive.Open(stream)) {
                    foreach (ZipArchiveEntry file in zip.ReadCentralDir()) {
                        string fileName = file.FilenameInZip;
                        if (fileName.EndsWith(".webp")
                            && int.TryParse(fileName.Substring(0, fileName.Length - 5), out int i)) {
                            MemoryStream memoryStream = new();
                            zip.ExtractFile(file, memoryStream);
                            memoryStream.Position = 0L;
                            newData.Add(i, SixLabors.ImageSharp.Image.Load<Rgba32>(DefaultImageDecoderOptions, memoryStream));
                            memoryStream.Close();
                        }
                    }
                    comment = zip.Comment;
                }
                Data = newData;
                string[] array = comment.Split([';'], StringSplitOptions.RemoveEmptyEntries);
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
                if (array.Length >= 4) {
                    LastOutput = uint.Parse(array[3], NumberStyles.HexNumber, null);
                }
                return LanguageControl.Get(GetType().Name, 1);
            }
            UintList2Data(GVListMemoryBankData.Stream2UintList(stream));
            return string.Empty;
        }

        public override void UintArray2Data(uint[] uints, int width = 0, int height = 0) {
            Image<Rgba32> image = new(DefaultImageConfiguration, width, height);
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    int index = y * width + x;
                    if (index >= uints.Length) {
                        break;
                    }
                    image[x, y] = new Rgba32(uints[index]);
                }
            }
            Data = new Dictionary<int, Image<Rgba32>> { { 0, image } };
            m_xLength = width;
            m_yLength = height;
            m_xyProduct = m_xLength * m_yLength;
            m_zLength = 1;
            m_xyzProduct = m_xyProduct;
            m_wLength = 1;
            m_totalLength = m_xyProduct;
            m_xOffset = 0;
            m_yOffset = 0;
            m_zOffset = 0;
            m_wOffset = 0;
            m_xSize = width;
            m_ySize = height;
        }

        public override uint[] Data2UintArray() {
            List<uint> uints = [];
            if (m_isDataInitialized) {
                for (int w = 0; w < m_wLength; w++) {
                    if (Data.TryGetValue(w, out Image<Rgba32> image)) {
                        Rgba32[] pixelArray = new Rgba32[image.Width * image.Height];
                        foreach (ImageFrame<Rgba32> frame in image.Frames) {
                            frame.CopyPixelDataTo(pixelArray);
                            uints.AddRange(pixelArray.Select(pixel => pixel.PackedValue));
                        }
                    }
                    else {
                        uints.AddRange(Enumerable.Repeat(0u, m_xyzProduct));
                    }
                }
            }
            return uints.ToArray();
        }

        public List<uint> Data2UintList(int startIndex = 0, int length = int.MaxValue) {
            if (m_isDataInitialized && startIndex < m_totalLength) {
                if (startIndex + length > m_totalLength) {
                    length = m_totalLength - startIndex;
                }
                List<uint> list = new(length);
                for (int i = startIndex; i < startIndex + length; i++) {
                    int w = i / m_xyzProduct;
                    if (Data.TryGetValue(w, out Image<Rgba32> image)) {
                        int wRemainder = i % m_xyzProduct;
                        int z = wRemainder / m_xyProduct;
                        int zRemainder = wRemainder % m_xyProduct;
                        int y = zRemainder / m_xLength;
                        int x = zRemainder % m_xLength;
                        list.Add(image.Frames[z][x, y].PackedValue);
                    }
                    else {
                        list.AddRange(Enumerable.Repeat(0u, m_xyzProduct));
                    }
                }
                return list;
            }
            return null;
        }

        public void UintList2Data(List<uint> list) {
            int width = (int)Math.Ceiling(Math.Sqrt(list.Count + 1));
            Image<Rgba32> image = new(DefaultImageConfiguration, width, width);
            for (int y = 0; y < width; y++) {
                for (int x = 0; x < width; x++) {
                    int index = y * width + x;
                    if (index >= list.Count) {
                        break;
                    }
                    image[x, y] = new Rgba32(list[index]);
                }
            }
            Data = new Dictionary<int, Image<Rgba32>> { { 0, image } };
            m_xLength = width;
            m_yLength = width;
            m_xyProduct = m_xLength * m_yLength;
            m_zLength = 1;
            m_xyzProduct = m_xyProduct;
            m_wLength = 1;
            m_totalLength = m_xyProduct;
            m_xOffset = 0;
            m_yOffset = 0;
            m_zOffset = 0;
            m_wOffset = 0;
            m_xSize = width;
            m_ySize = width;
        }
    }
}