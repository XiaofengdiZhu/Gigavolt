namespace Game {
    public class SubsystemGVMemoryBankCBlockBehavior : SubsystemGVEditableItemBehavior<MemoryBankData> {
        public override int[] HandledBlocks => [GVBlocksManager.GetBlockIndex<GVMemoryBankCBlock>()];

        public SubsystemGVMemoryBankCBlockBehavior() : base(GVBlocksManager.GetBlockIndex<GVMemoryBankCBlock>()) { }

        public override int GetIdFromValue(int value) => (Terrain.ExtractData(value) >> 5) & 8191;

        public override int SetIdToValue(int value, int id) =>
            Terrain.ReplaceData(value, (Terrain.ExtractData(value) & -262113) | ((id & 8191) << 5));

        public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer) {
            if (componentPlayer.DragHostWidget.IsDragInProgress) {
                return false;
            }
            int value = inventory.GetSlotValue(slotIndex);
            int count = inventory.GetSlotCount(slotIndex);
            int id = GetIdFromValue(value);
            MemoryBankData memoryBankData = GetItemData(id, true);
            if (SettingsManager.UsePrimaryMemoryBank) {
                DialogsManager.ShowDialog(
                    componentPlayer.GuiWidget,
                    new EditMemoryBankDialog(
                        memoryBankData,
                        () => {
                            inventory.RemoveSlotItems(slotIndex, count);
                            inventory.AddSlotItems(slotIndex, SetIdToValue(value, StoreItemDataAtUniqueId(memoryBankData, id)), count);
                        }
                    )
                );
            }
            else {
                DialogsManager.ShowDialog(
                    componentPlayer.GuiWidget,
                    new EditMemoryBankDialogAPI(
                        memoryBankData,
                        () => {
                            inventory.RemoveSlotItems(slotIndex, count);
                            inventory.AddSlotItems(slotIndex, SetIdToValue(value, StoreItemDataAtUniqueId(memoryBankData, id)), count);
                        }
                    )
                );
            }
            return true;
        }

        public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer) {
            int id = GetIdFromValue(value);
            MemoryBankData memoryBankData = GetItemData(id, true);
            if (SettingsManager.UsePrimaryMemoryBank) {
                DialogsManager.ShowDialog(
                    componentPlayer.GuiWidget,
                    new EditMemoryBankDialog(
                        memoryBankData,
                        () => { SubsystemTerrain.ChangeCell(x, y, z, SetIdToValue(value, StoreItemDataAtUniqueId(memoryBankData, id))); }
                    )
                );
            }
            else {
                DialogsManager.ShowDialog(
                    componentPlayer.GuiWidget,
                    new EditMemoryBankDialogAPI(
                        memoryBankData,
                        () => { SubsystemTerrain.ChangeCell(x, y, z, SetIdToValue(value, StoreItemDataAtUniqueId(memoryBankData, id))); }
                    )
                );
            }
            return true;
        }
    }
}