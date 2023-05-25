using System.Globalization;

namespace Game {
    public class GVPistonData : IEditableItemData {
        public int MaxExtension = 7;
        public int PullCount;
        public int Speed = 3;

        public IEditableItemData Copy() => new GVPistonData { MaxExtension = MaxExtension, PullCount = PullCount, Speed = Speed };

        public void LoadString(string data) {
            string[] arr = data.Split(',');
            MaxExtension = int.Parse(arr[0], NumberStyles.HexNumber, null);
            PullCount = int.Parse(arr[1], NumberStyles.HexNumber, null);
            Speed = int.Parse(arr[2], NumberStyles.HexNumber, null);
        }

        public string SaveString() => string.Join(",", MaxExtension.ToString("X", null), PullCount.ToString("X", null), Speed.ToString("X", null));
    }
}