using System;
using Engine;

namespace Game {
    public class SubsystemGVVolatileMemoryBankBlockBehavior : SubsystemGVEditableItemBehavior<GVVolatileMemoryBankData> {
        public override int[] HandledBlocks => [GVVolatileMemoryBankBlock.Index];

        public SubsystemGVVolatileMemoryBankBlockBehavior() : base(GVVolatileMemoryBankBlock.Index) { }

        public override int GetIdFromValue(int value) => (Terrain.ExtractData(value) >> 5) & 8191;
        public override int SetIdToValue(int value, int id) => Terrain.ReplaceData(value, (Terrain.ExtractData(value) & -262113) | ((id & 8191) << 5));

        public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer) {
            try {
                bool isDragInProgress = componentPlayer.DragHostWidget.IsDragInProgress;
                if (isDragInProgress) {
                    return false;
                }
                int value = inventory.GetSlotValue(slotIndex);
                int count = inventory.GetSlotCount(slotIndex);
                int id = GetIdFromValue(value);
                GVVolatileMemoryBankData memoryBankData = GetItemData(id, true);
                DialogsManager.ShowDialog(
                    componentPlayer.GuiWidget,
                    new EditGVVolatileMemoryBankDialog(
                        memoryBankData,
                        delegate {
                            inventory.RemoveSlotItems(slotIndex, count);
                            inventory.AddSlotItems(slotIndex, SetIdToValue(value, StoreItemDataAtUniqueId(memoryBankData, id)), count);
                        }
                    )
                );
            }
            catch (Exception e) {
                Log.Error(e);
            }
            return true;
        }

        public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer) {
            int id = GetIdFromValue(value);
            GVVolatileMemoryBankData memoryBankData = GetItemData(id, true);
            DialogsManager.ShowDialog(componentPlayer.GuiWidget, new EditGVVolatileMemoryBankDialog(memoryBankData, () => { SubsystemTerrain.ChangeCell(x, y, z, SetIdToValue(value, StoreItemDataAtUniqueId(memoryBankData, id))); }));
            return true;
        }
    }
}