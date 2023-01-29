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
            MaxExtension = Convert.ToInt32(arr[0],16);
            PullCount = Convert.ToInt32(arr[1],16);
            Speed = Convert.ToInt32(arr[2],16);
        }

        public string SaveString()
        {
            return string.Join(",",new string[3] {
                Convert.ToString(MaxExtension,16),
                Convert.ToString(PullCount,16),
                Convert.ToString(Speed,16)
            });
        }
    }
}
