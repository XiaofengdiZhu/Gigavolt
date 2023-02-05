using Engine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Game
{
    public class GVPistonData : IEditableItemData
    {

        public int MaxExtension = 7;
        public int PullCount = 0;
        public int Speed = 3;

        public IEditableItemData Copy()
        {
            return new GVPistonData
            {
               MaxExtension = MaxExtension,
               PullCount = PullCount,
               Speed = Speed
            };
        }

        public void LoadString(string data)
        {
            string[] arr = data.Split(',');
            MaxExtension = int.Parse(arr[0], System.Globalization.NumberStyles.HexNumber, null);
            PullCount = int.Parse(arr[1], System.Globalization.NumberStyles.HexNumber, null);
            Speed = int.Parse(arr[2], System.Globalization.NumberStyles.HexNumber, null);
        }

        public string SaveString()
        {
            return string.Join(",",new string[3] {
                MaxExtension.ToString("X",null),
                PullCount.ToString("X",null),
                Speed.ToString("X",null)
            });
        }
    }
}
