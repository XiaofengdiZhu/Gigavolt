namespace Game {
    public class SubsystemGVBatteryBlockBehavior : SubsystemGVEditableItemBehavior<GigaVoltageLevelData> {
        public override int[] HandledBlocks => new[] { GVBatteryBlock.Index };
        public SubsystemGVBatteryBlockBehavior() : base(GVBatteryBlock.Index) { }

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
                new EditGigaVoltageLevelDialog(
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
            GigaVoltageLevelData blockData = GetItemData(id, true);
            DialogsManager.ShowDialog(
                componentPlayer.GuiWidget,
                new EditGigaVoltageLevelDialog(
                    blockData,
                    voltage => {
                        SubsystemTerrain.Terrain.SetCellValueFast(x, y, z, SetIdToValue(value, StoreItemDataAtUniqueId(blockData, id)));
                        SubsystemGVElectricity subsystemGVElectricity = SubsystemTerrain.Project.FindSubsystem<SubsystemGVElectricity>(true);
                        BatteryGVElectricElement electricElement = (BatteryGVElectricElement)subsystemGVElectricity.GetGVElectricElement(x, y, z, 4);
                        if (electricElement != null) {
                            electricElement.m_voltage = voltage;
                            electricElement.m_edited = true;
                            subsystemGVElectricity.QueueGVElectricElementForSimulation(electricElement, subsystemGVElectricity.CircuitStep + 1);
                        }
                    }
                )
            );
            return true;
        }
    }
}