namespace Game {
    public class GVDebugData : IEditableItemData {
        public float Speed = 1f;
        public bool DisplayStepFloatingButtons;
        public bool KeyboardControl;
        public bool PreventChunkFromBeingFree;
        public bool LoadChunkInAdvance;

        public IEditableItemData Copy() => new GVDebugData { Speed = Speed };

        public void LoadString(string data) {
            string[] array = data.Split(';');
            if (array.Length > 0) {
                Speed = float.Parse(array[0]);
                if (array.Length > 1) {
                    GVStaticStorage.DisplayVoltage = bool.Parse(array[1]);
                    if (array.Length > 2) {
                        PreventChunkFromBeingFree = bool.Parse(array[2]);
                        if (array.Length > 3) {
                            DisplayStepFloatingButtons = bool.Parse(array[3]);
                            if (array.Length > 4) {
                                KeyboardControl = bool.Parse(array[4]);
                                if (array.Length > 5) {
                                    GVStaticStorage.WheelPanelEnabled = bool.Parse(array[5]);
                                    if (array.Length > 6) {
                                        LoadChunkInAdvance = bool.Parse(array[6]);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public string SaveString() => $"{Speed:F2};{GVStaticStorage.DisplayVoltage};{PreventChunkFromBeingFree};{DisplayStepFloatingButtons};{KeyboardControl};{GVStaticStorage.WheelPanelEnabled};{LoadChunkInAdvance}";
    }
}