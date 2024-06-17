using System;
using Game;

namespace Engine.Serialization {
    // Token: 0x020000DB RID: 219
    [HumanReadableConverter(typeof(GVPoint3))]
    public class GVPoint3HumanReadableConverter : IHumanReadableConverter {
        public string ConvertToString(object value) {
            GVPoint3 point = (GVPoint3)value;
            return $"{point.X},{point.Y},{point.Z},{point.SubterrainId}";
        }

        public object ConvertFromString(Type type, string data) {
            string[] array = data.Split(',');
            if (array.Length == 4) {
                return new GVPoint3(int.Parse(array[0]), int.Parse(array[1]), int.Parse(array[2]), uint.Parse(array[3]));
            }
            throw new Exception();
        }
    }
}