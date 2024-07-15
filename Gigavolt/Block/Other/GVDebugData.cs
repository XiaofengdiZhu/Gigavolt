namespace Game {
    public class GVDebugData : IEditableItemData {
        public string Data = "1.00";

        public IEditableItemData Copy() => new GVDebugData { Data = Data };

        public void LoadString(string data) {
            Data = "1.00";
        }

        public string SaveString() => "1.00";
    }
}