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
            Data = Convert.ToUInt32(data, 16);
        }

        public string SaveString()
        {
            return Convert.ToString(Data,16);
        }
    }
}
