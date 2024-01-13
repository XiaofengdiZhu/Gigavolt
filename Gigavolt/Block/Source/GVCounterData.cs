using System.Globalization;

namespace Game {
    public class GVCounterData : IEditableItemData {
        public uint Overflow;
        public uint Initial;

        public IEditableItemData Copy() => new GVCounterData { Overflow = Overflow, Initial = Initial };

        public void LoadString(string data) {
            string[] array = data.Split(';');
            Overflow = uint.Parse(array[0], NumberStyles.HexNumber, null);
            if (array.Length > 1) {
                Initial = uint.Parse(array[1], NumberStyles.HexNumber, null);
            }
        }

        public string SaveString() => Overflow.ToString("X", null) + ";" + Initial.ToString("X", null);
    }
}