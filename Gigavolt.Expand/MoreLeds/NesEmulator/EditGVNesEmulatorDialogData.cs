namespace Game {
    public class EditGVNesEmulatorDialogData : IEditableItemData {
        public string Data = "nestest";

        public IEditableItemData Copy() => new EditGVNesEmulatorDialogData { Data = Data };

        public void LoadString(string data) {
            Data = data;
        }

        public string SaveString() => Data;
    }
}