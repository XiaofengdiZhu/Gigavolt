using Engine;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    public class GVTruthTableCData : IEditableItemData
    {
        public static List<char> m_hexChars = new List<char>
        {
            '0',
            '1',
            '2',
            '3',
            '4',
            '5',
            '6',
            '7',
            '8',
            '9',
            'A',
            'B',
            'C',
            'D',
            'E',
            'F'
        };

        public byte[] Data = new byte[16];

        public IEditableItemData Copy()
        {
            return new GVTruthTableCData
            {
                Data = (byte[])Data.Clone()
            };
        }

        public void LoadString(string data)
        {
            for (int i = 0; i < 16; i++)
            {
                int num = (i < data.Length) ? m_hexChars.IndexOf(char.ToUpperInvariant(data[i])) : 0;
                if (num < 0)
                {
                    num = 0;
                }
                Data[i] = (byte)num;
            }
        }

        public void LoadBinaryString(string data)
        {
            for (int i = 0; i < 16; i++)
            {
                Data[i] = (byte)((i < data.Length && data[i] != '0') ? 15 : 0);
            }
        }

        public string SaveString()
        {
            var stringBuilder = new StringBuilder();
            for (int i = 0; i < Data.Length; i++)
            {
                int index = MathUtils.Clamp(Data[i], 0, 15);
                stringBuilder.Append(m_hexChars[index]);
            }
            return stringBuilder.ToString().TrimEnd('0');
        }

        public string SaveBinaryString()
        {
            var stringBuilder = new StringBuilder();
            for (int i = 0; i < Data.Length; i++)
            {
                stringBuilder.Append((Data[i] != 0) ? '1' : '0');
            }
            return stringBuilder.ToString().TrimEnd('0');
        }
    }
}
