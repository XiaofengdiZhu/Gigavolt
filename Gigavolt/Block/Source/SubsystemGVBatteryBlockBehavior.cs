namespace Game {
    public class SubsystemGVBatteryBlockBehavior : SubsystemGVEditableItemBehavior<GigaVoltageLevelData> {
        public override int[] HandledBlocks => [GVBlocksManager.GetBlockIndex<GVBatteryBlock>()];
        public SubsystemGVBatteryBlockBehavior() : base(GVBlocksManager.GetBlockIndex<GVBatteryBlock>()) { }

        public override int GetIdFromValue(int value) => (Terrain.ExtractData(value) >> 1) & 4095;
        public override int SetIdToValue(int value, int id) => Terrain.ReplaceData(value, (Terrain.ExtractData(value) & -8191) | ((id & 4095) << 1));

        public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer) {
            if (componentPlayer.DragHostWidget.IsDragInProgress) {
                return false;
            }
            int value = inventory.GetSlotValue(slotIndex);
            int count = inventory.GetSlotCount(slotIndex);
            int id = GetIdFromValue(value);
            GigaVoltageLevelData blockData = GetItemData(id, true);
            DialogsManager.ShowDialog(
                componentPlayer.GuiWidget,
                new EditGVUintDialog(
                    blockData.Data,
                    newVoltage => {
                        blockData.Data = newVoltage;
                        blockData.SaveString();
                        inventory.RemoveSlotItems(slotIndex, count);
                        inventory.AddSlotItems(slotIndex, SetIdToValue(value, StoreItemDataAtUniqueId(blockData, id)), count);
                    }
                )
            );
            return true;
        }

        public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer) {
            int id = GetIdFromValue(value);
            GigaVoltageLevelData blockData = GetItemData(id, true);
            DialogsManager.ShowDialog(
                componentPlayer.GuiWidget,
                new EditGVUintDialog(
                    blockData.Data,
                    newVoltage => {
                        blockData.Data = newVoltage;
                        blockData.SaveString();
                        SubsystemTerrain.ChangeCell(x, y, z, SetIdToValue(value, StoreItemDataAtUniqueId(blockData, id)));
                    }
                )
            );
            return true;
        }
    }
}