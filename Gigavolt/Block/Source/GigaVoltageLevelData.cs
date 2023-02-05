using Engine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    public class GigaVoltageLevelData : IEditableItemData
    {

        public uint Data = uint.MaxValue;

        public IEditableItemData Copy()
        {
            return new GigaVoltageLevelData
            {
                Data = Data
            };
        }

        public void LoadString(string data)
        {
            Data = uint.Parse(data, System.Globalization.NumberStyles.HexNumber,null);
        }

        public string SaveString()
        {
            return Data.ToString("X", null);
        }
    }
}
