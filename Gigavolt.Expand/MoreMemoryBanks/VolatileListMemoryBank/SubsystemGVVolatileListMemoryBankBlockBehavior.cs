using System;
using Engine;

namespace Game {
    public class SubsystemGVVolatileListMemoryBankBlockBehavior : SubsystemGVEditableItemBehavior<GVVolatileListMemoryBankData> {
        public override int[] HandledBlocks => [BlocksManager.GetBlockIndex<GVVolatileListMemoryBankBlock>()];

        public SubsystemGVVolatileListMemoryBankBlockBehavior() : base(BlocksManager.GetBlockIndex<GVVolatileListMemoryBankBlock>()) { }

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
                GVVolatileListMemoryBankData memoryBankData = GetItemData(id, true);
                DialogsManager.ShowDialog(
                    componentPlayer.GuiWidget,
                    new EditGVVolatileListMemoryBankDialog(
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
            GVVolatileListMemoryBankData memoryBankData = GetItemData(id, true);
            DialogsManager.ShowDialog(componentPlayer.GuiWidget, new EditGVVolatileListMemoryBankDialog(memoryBankData, () => { SubsystemTerrain.ChangeCell(x, y, z, SetIdToValue(value, StoreItemDataAtUniqueId(memoryBankData, id))); }));
            return true;
        }
    }
}