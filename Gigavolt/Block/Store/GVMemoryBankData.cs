using Engine;
using Engine.Media;
using GameEntitySystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    public class GVMemoryBankData : IEditableItemData
    {

        public Guid m_guid;
        public string m_worldDirectory;
        public Image Data;
        public SubsystemGVMemoryBankBlockBehavior m_subsystem;
        public GVMemoryBankData()
        {
            m_guid = Guid.NewGuid();
            m_worldDirectory = null;
            Data = null;
        }
        public GVMemoryBankData(Guid guid, SubsystemGVMemoryBankBlockBehavior subsystem, string worldDirectory, Image image=null, uint lastOutput = 0)
        {
            m_guid = guid;
            m_subsystem = subsystem;
            m_worldDirectory = worldDirectory;
            Data = image;
            LastOutput = lastOutput;
            subsystem.guidDataDictionary.Add(m_guid.ToString(), this);
        }
        public uint LastOutput
        {
            get;
            set;
        }

        public uint Read(uint col, uint row)
        {
            if (Data == null)
            {
                return 0;
            }
            int col_int = MathUint.ToInt(col);
            int row_int = MathUint.ToInt(row);
            if (col_int < Data.Width && row_int < Data.Height)
            {
                return Data.GetPixel(col_int, row_int).PackedValue;
            }
            return 0;
        }

        public void Write(uint col, uint row, uint data)
        {
            if (Data == null)
            {
                return;
            }
            int col_int = MathUint.ToInt(col);
            int row_int = MathUint.ToInt(row);
            if (col_int < Data.Width && row_int < Data.Height)
            {
                Data.SetPixel(col_int, row_int, new Color(data));
            }
        }

        public IEditableItemData Copy()
        {
            return new GVMemoryBankData(m_guid, m_subsystem, m_worldDirectory, Data == null ? null : new Image(Data), LastOutput);
        }
        public void LoadData()
        {
            if (m_worldDirectory != null)
            {
                try
                {
                    Data = Image.Load($"{m_worldDirectory}/GVMB/{m_guid}.png", ImageFileFormat.Png);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
        }
        public void LoadString(string data)
        {
            string[] array = data.Split(new char[1]
            {
                ';'
            }, StringSplitOptions.RemoveEmptyEntries);
            if (array.Length >= 1)
            {
                string text = array[0];
                m_guid = Guid.ParseExact(text, "D");
                LoadData();
            }
            if (array.Length >= 2)
            {
                LastOutput = uint.Parse(array[1], System.Globalization.NumberStyles.HexNumber, null);
            }
        }

        public string SaveString()
        {
            return SaveString(saveLastOutput: true);
        }

        public string SaveString(bool saveLastOutput)
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(m_guid.ToString());
            if (saveLastOutput)
            {
                stringBuilder.Append(';');
                stringBuilder.Append(LastOutput.ToString("X", null));
            }
            if (Data != null)
            {
                try
                {
                    Image.Save(Data, $"{m_worldDirectory}/GVMB/{m_guid}.png", ImageFileFormat.Png, true);
                }
                catch (Exception ex)
                {
                    Log.Error(ex);
                }
            }
            return stringBuilder.ToString();
        }
        public static Image String2Image(string data, int width = 0, int height = 0)
        {
            List<uint[]> rowList = new List<uint[]>();
            int maxColLength = 0;
            string[] rows = data.Split(';');
            foreach (string row in rows)
            {
                string[] cols = row.Split(',');
                if (cols.Length > maxColLength)
                {
                    maxColLength = cols.Length;
                }
                uint[] uints = new uint[cols.Length];
                for (int i = 0; i < cols.Length; i++)
                {
                    uints[i] = uint.Parse(cols[i], System.Globalization.NumberStyles.HexNumber, null);
                }
                rowList.Add(uints);
            }
            Image image = new Image(width == 0 ? maxColLength : width, height == 0 ? rows.Length : height);
            for (int i = 0; i < image.Height; i++)
            {
                if (i == rowList.Count)
                {
                    break;
                }
                for (int j = 0; j < rowList[i].Length; j++)
                {
                    if (j == image.Width)
                    {
                        break;
                    }
                    image.SetPixel(j, i, new Color(rowList[i][j]));
                }
            }
            return image;
        }

        public static string Image2String(Image image)
        {
            string[] result = new string[image.Height];
            for (int i = 0; i < image.Height; i++)
            {
                int lastNotZero = -1;
                for (int j = image.Width - 1; j >= 0; j--)
                {
                    if (image.GetPixel(j, i).PackedValue > 0)
                    {
                        lastNotZero = j;
                        break;
                    }
                }
                StringBuilder stringBuilder = new StringBuilder();
                for (int j = 0; j < lastNotZero; j++)
                {
                    stringBuilder.Append(image.GetPixel(j, i).PackedValue.ToString("X", null));
                    stringBuilder.Append(',');
                }
                if (lastNotZero > -1)
                {
                    stringBuilder.Append(image.GetPixel(lastNotZero, i).PackedValue.ToString("X", null));
                }
                result[i] = stringBuilder.ToString();
            }
            return string.Join(";", result);
        }
        public static byte[] Image2Bytes(Image image)
        {
            byte[] bytes = new byte[image.Pixels.Length * 4];
            for (int i = 0; i < image.Pixels.Length; i++)
            {
                bytes[i] = (byte)(image.Pixels[i].PackedValue & 0xFF);
                bytes[i + 1] = (byte)(image.Pixels[i].PackedValue & (0xFF << 8));
                bytes[i + 2] = (byte)(image.Pixels[i].PackedValue & (0xFF << 16));
                bytes[i + 3] = (byte)(image.Pixels[i].PackedValue & (0xFF << 24));
            }
            return bytes;
        }
    }
}