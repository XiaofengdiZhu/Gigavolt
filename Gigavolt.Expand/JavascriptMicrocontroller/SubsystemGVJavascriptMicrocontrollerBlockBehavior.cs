namespace Game {
    public class SubsystemGVJavascriptMicrocontrollerBlockBehavior : SubsystemGVEditableItemBehavior<GVJavascriptMicrocontrollerData> {
        public override int[] HandledBlocks => [GVJavascriptMicrocontrollerBlock.Index];

        public SubsystemGVJavascriptMicrocontrollerBlockBehavior() : base(GVJavascriptMicrocontrollerBlock.Index) { }

        public override int GetIdFromValue(int value) => (Terrain.ExtractData(value) >> 5) & 8191;
        public override int SetIdToValue(int value, int id) => Terrain.ReplaceData(value, (Terrain.ExtractData(value) & -262113) | ((id & 8191) << 5));

        public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer) {
            if (componentPlayer.DragHostWidget.IsDragInProgress) {
                return false;
            }
            int value = inventory.GetSlotValue(slotIndex);
            int count = inventory.GetSlotCount(slotIndex);
            int id = GetIdFromValue(value);
            GVJavascriptMicrocontrollerData javascriptMicrocontrollerData = GetItemData(id, true);
            DialogsManager.ShowDialog(
                componentPlayer.GuiWidget,
                new EditGVJavascriptMicrocontrollerDialog(
                    javascriptMicrocontrollerData,
                    () => {
                        inventory.RemoveSlotItems(slotIndex, count);
                        inventory.AddSlotItems(slotIndex, SetIdToValue(value, StoreItemDataAtUniqueId(javascriptMicrocontrollerData, id)), count);
                    }
                )
            );
            return true;
        }

        public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer) {
            int id = GetIdFromValue(value);
            GVJavascriptMicrocontrollerData javascriptMicrocontrollerData = GetItemData(id, true);
            DialogsManager.ShowDialog(componentPlayer.GuiWidget, new EditGVJavascriptMicrocontrollerDialog(javascriptMicrocontrollerData, () => { SubsystemTerrain.ChangeCell(x, y, z, SetIdToValue(value, StoreItemDataAtUniqueId(javascriptMicrocontrollerData, id))); }));
            return true;
        }
    }
}