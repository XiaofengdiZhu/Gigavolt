using System.Globalization;

namespace Game {
    public class GVButtonData : IEditableItemData {
        public uint GigaVoltageLevel = uint.MaxValue;
        public int Duration = 10;

        public IEditableItemData Copy() => new GVButtonData { GigaVoltageLevel = GigaVoltageLevel, Duration = Duration };

        public void LoadString(string data) {
            string[] strings = data.Split(';');
            GigaVoltageLevel = uint.Parse(strings[0], NumberStyles.HexNumber, null);
            if (strings.Length > 0) {
                Duration = int.Parse(strings[1]);
                if (Duration < 0) {
                    Duration = 10;
                }
            }
            else {
                Duration = 10;
            }
        }

        public string SaveString() => $"{GigaVoltageLevel:X};{Duration}";
    }
}