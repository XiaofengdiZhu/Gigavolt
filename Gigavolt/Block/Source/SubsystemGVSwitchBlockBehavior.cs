namespace Game {
    public class SubsystemGVSwitchBlockBehavior : SubsystemGVEditableItemBehavior<GigaVoltageLevelData> {
        public override int[] HandledBlocks => new[] { GVSwitchBlock.Index };
        public SubsystemGVSwitchBlockBehavior() : base(GVSwitchBlock.Index) { }

        public override int GetIdFromValue(int value) => (Terrain.ExtractData(value) >> 4) & 4095;
        public override int SetIdToValue(int value, int id) => Terrain.ReplaceData(value, (Terrain.ExtractData(value) & -65521) | ((id & 4095) << 4));

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