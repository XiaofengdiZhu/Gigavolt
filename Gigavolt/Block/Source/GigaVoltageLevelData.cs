using System.Globalization;

namespace Game {
    public class GigaVoltageLevelData : IEditableItemData {
        public uint Data = uint.MaxValue;

        public IEditableItemData Copy() => new GigaVoltageLevelData { Data = Data };

        public void LoadString(string data) {
            Data = uint.Parse(data, NumberStyles.HexNumber, null);
        }

        public string SaveString() => Data.ToString("X", null);
    }
}