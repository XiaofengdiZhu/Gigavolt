namespace Game
{
    public class GVDebugData : IEditableItemData
    {

        public string Data = "1.00";

        public IEditableItemData Copy()
        {
            return new GVDebugData
            {
                Data = Data
            };
        }

        public void LoadString(string data)
        {
            Data = data;
        }

        public string SaveString()
        {
            return Data;
        }
    }
}