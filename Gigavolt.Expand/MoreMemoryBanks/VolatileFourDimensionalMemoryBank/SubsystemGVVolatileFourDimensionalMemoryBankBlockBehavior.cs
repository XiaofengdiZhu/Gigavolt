﻿namespace Game {
    public class
        SubsystemGVVolatileFourDimensionalMemoryBankBlockBehavior : SubsystemGVEditableItemBehavior<GVVolatileFourDimensionalMemoryBankData> {
        public override int[] HandledBlocks => [GVBlocksManager.GetBlockIndex<GVVolatileFourDimensionalMemoryBankBlock>()];
        public SubsystemGVVolatileFourDimensionalMemoryBankBlockBehavior() : base(GVBlocksManager.GetBlockIndex<GVVolatileListMemoryBankBlock>()) { }

        public override int GetIdFromValue(int value) => (Terrain.ExtractData(value) >> 5) & 8191;

        public override int SetIdToValue(int value, int id) =>
            Terrain.ReplaceData(value, (Terrain.ExtractData(value) & -262113) | ((id & 8191) << 5));

        public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer) {
            bool isDragInProgress = componentPlayer.DragHostWidget.IsDragInProgress;
            if (isDragInProgress) {
                return false;
            }
            int value = inventory.GetSlotValue(slotIndex);
            int count = inventory.GetSlotCount(slotIndex);
            int id = GetIdFromValue(value);
            GVVolatileFourDimensionalMemoryBankData memoryBankData = GetItemData(id, true);
            DialogsManager.ShowDialog(
                componentPlayer.GuiWidget,
                new EditGVVolatileFourDimensionalMemoryBankDialog(
                    memoryBankData,
                    delegate {
                        inventory.RemoveSlotItems(slotIndex, count);
                        inventory.AddSlotItems(slotIndex, SetIdToValue(value, StoreItemDataAtUniqueId(memoryBankData, id)), count);
                    }
                )
            );
            return true;
        }

        public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer) {
            int id = GetIdFromValue(value);
            GVVolatileFourDimensionalMemoryBankData memoryBankData = GetItemData(id, true);
            DialogsManager.ShowDialog(
                componentPlayer.GuiWidget,
                new EditGVVolatileFourDimensionalMemoryBankDialog(
                    memoryBankData,
                    () => { SubsystemTerrain.ChangeCell(x, y, z, SetIdToValue(value, StoreItemDataAtUniqueId(memoryBankData, id))); }
                )
            );
            return true;
        }
    }
}