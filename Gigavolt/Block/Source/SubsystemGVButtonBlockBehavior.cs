namespace Game {
    public class SubsystemGVButtonBlockBehavior : SubsystemGVEditableItemBehavior<GVButtonData> {
        public override int[] HandledBlocks => [GVBlocksManager.GetBlockIndex<GVButtonBlock>()];

        public SubsystemGVButtonBlockBehavior() : base(GVBlocksManager.GetBlockIndex<GVButtonBlock>()) { }

        public override int GetIdFromValue(int value) => (Terrain.ExtractData(value) >> 3) & 2047;
        public override int SetIdToValue(int value, int id) => Terrain.ReplaceData(value, (Terrain.ExtractData(value) & -16377) | ((id & 2047) << 3));

        public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer) {
            if (componentPlayer.DragHostWidget.IsDragInProgress) {
                return false;
            }
            int value = inventory.GetSlotValue(slotIndex);
            int count = inventory.GetSlotCount(slotIndex);
            int id = GetIdFromValue(value);
            GVButtonData blockData = GetItemData(id, true);
            DialogsManager.ShowDialog(
                componentPlayer.GuiWidget,
                new EditGVButtonDialog(
                    blockData,
                    delegate {
                        inventory.RemoveSlotItems(slotIndex, count);
                        inventory.AddSlotItems(slotIndex, SetIdToValue(value, StoreItemDataAtUniqueId(blockData, id)), count);
                    }
                )
            );
            return true;
        }

        public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer) {
            int id = GetIdFromValue(value);
            GVButtonData blockData = GetItemData(id, true);
            DialogsManager.ShowDialog(
                componentPlayer.GuiWidget,
                new EditGVButtonDialog(
                    blockData,
                    delegate { SubsystemTerrain.ChangeCell(x, y, z, SetIdToValue(value, StoreItemDataAtUniqueId(blockData, id))); }
                )
            );
            return true;
        }
    }
}