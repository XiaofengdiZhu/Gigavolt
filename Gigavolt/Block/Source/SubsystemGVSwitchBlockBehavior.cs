using System.Diagnostics;

namespace Game {
    public class SubsystemGVSwitchBlockBehavior : SubsystemGVEditableItemBehavior<GigaVoltageLevelData> {
        public override int[] HandledBlocks => new[] { GVSwitchBlock.Index };
        public SubsystemGVSwitchBlockBehavior() : base(GVSwitchBlock.Index) { }

        public override int GetIdFromValue(int value) => (Terrain.ExtractData(value) >> 4) & 1023;
        public override int SetIdToValue(int value, int id) => Terrain.ReplaceData(value, (Terrain.ExtractData(value) & -16369) | ((id & 1023) << 4));

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
                        int newId = StoreItemDataAtUniqueId(blockData, id);
                        int newValue = SetIdToValue(value, newId);
                        SubsystemTerrain.ChangeCell(x, y, z, newValue);
                        Debug.WriteLine($"old value {value}, old id {id}, new value {newValue}, new id {newId}; debug new id {GetIdFromValue(newValue)}");
                    }
                )
            );
            return true;
        }
    }
}