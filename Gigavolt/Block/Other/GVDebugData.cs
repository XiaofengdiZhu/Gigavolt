namespace Game {
    public class GVDebugData : IEditableItemData {
        public string Data = "1.00";

        public IEditableItemData Copy() => new GVDebugData { Data = Data };

        public void LoadString(string data) {
            Data = data;
        }

        public string SaveString() => Data;
    }
}