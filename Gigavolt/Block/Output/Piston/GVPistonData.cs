using System.Globalization;

namespace Game {
    public class GVPistonData : IEditableItemData {
        public int MaxExtension = 7;
        public int PullCount = 7;
        public int Speed = 3;
        public bool Transparent;
        public bool Pulling;
        public bool Strict;

        public IEditableItemData Copy() => new GVPistonData { MaxExtension = MaxExtension, PullCount = PullCount, Speed = Speed, Transparent = Transparent, Pulling = Pulling, Strict = Strict};

        public void LoadString(string data) {
            string[] arr = data.Split(',');
            MaxExtension = arr.Length > 0 ? int.Parse(arr[0], NumberStyles.HexNumber, null) : 7;
            PullCount = arr.Length > 1 ? int.Parse(arr[1], NumberStyles.HexNumber, null) : 7;
            Speed = arr.Length > 2 ? int.Parse(arr[2], NumberStyles.HexNumber, null) : 3;
            Transparent = arr.Length > 3 && int.Parse(arr[3]) == 1;
        }

        public string SaveString() => string.Join(
            ",",
            MaxExtension.ToString("X", null),
            PullCount.ToString("X", null),
            Speed.ToString("X", null),
            Transparent ? "1" : "0"
        );
    }
}