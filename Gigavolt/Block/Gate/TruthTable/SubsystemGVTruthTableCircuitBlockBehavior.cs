namespace Game {
    public class SubsystemGVTruthTableCircuitBlockBehavior : SubsystemGVEditableItemBehavior<GVTruthTableData> {
        public override int[] HandledBlocks => [GVTruthTableCircuitBlock.Index];

        public SubsystemGVTruthTableCircuitBlockBehavior() : base(GVTruthTableCircuitBlock.Index) { }

        public override int GetIdFromValue(int value) => (Terrain.ExtractData(value) >> 5) & 8191;
        public override int SetIdToValue(int value, int id) => Terrain.ReplaceData(value, (Terrain.ExtractData(value) & -262113) | ((id & 8191) << 5));

        public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer) {
            if (componentPlayer.DragHostWidget.IsDragInProgress) {
                return false;
            }
            int value = inventory.GetSlotValue(slotIndex);
            int count = inventory.GetSlotCount(slotIndex);
            int id = GetIdFromValue(value);
            GVTruthTableData truthTableData = GetItemData(id, true);
            DialogsManager.ShowDialog(
                componentPlayer.GuiWidget,
                new EditGVTruthTableDialog(
                    truthTableData,
                    () => {
                        inventory.RemoveSlotItems(slotIndex, count);
                        inventory.AddSlotItems(slotIndex, SetIdToValue(value, StoreItemDataAtUniqueId(truthTableData, id)), count);
                    }
                )
            );
            return true;
        }

        public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer) {
            int id = GetIdFromValue(value);
            GVTruthTableData truthTableData = GetItemData(id, true);
            DialogsManager.ShowDialog(componentPlayer.GuiWidget, new EditGVTruthTableDialog(truthTableData, () => { SubsystemTerrain.ChangeCell(x, y, z, SetIdToValue(value, StoreItemDataAtUniqueId(truthTableData, id))); }));
            return true;
        }
    }
}