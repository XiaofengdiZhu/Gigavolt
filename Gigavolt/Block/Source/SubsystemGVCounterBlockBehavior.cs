using Engine;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVCounterBlockBehavior : SubsystemGVEditableItemBehavior<GigaVoltageLevelData> {
        public SubsystemGVElectricity m_subsystemGVElectricity;
        public override int[] HandledBlocks => new[] { GVCounterBlock.Index };

        public SubsystemGVCounterBlockBehavior() : base(GVCounterBlock.Index) { }

        public override int GetIdFromValue(int value) => (Terrain.ExtractData(value) >> 5) & 4095;
        public override int SetIdToValue(int value, int id) => Terrain.ReplaceData(value, (Terrain.ExtractData(value) & -131041) | ((id & 4095) << 5));

        public override void Load(ValuesDictionary valuesDictionary) {
            base.Load(valuesDictionary);
            m_subsystemGVElectricity = Project.FindSubsystem<SubsystemGVElectricity>(true);
        }

        public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer) {
            if (componentPlayer.DragHostWidget.IsDragInProgress) {
                return false;
            }
            int value = inventory.GetSlotValue(slotIndex);
            int count = inventory.GetSlotCount(slotIndex);
            int id = GetIdFromValue(value);
            GigaVoltageLevelData blockData = GetItemData(id) ?? new GigaVoltageLevelData { Data = 0u };
            DialogsManager.ShowDialog(
                componentPlayer.GuiWidget,
                new EditGVCounterDialog(
                    blockData,
                    null,
                    _ => {
                        inventory.RemoveSlotItems(slotIndex, count);
                        inventory.AddSlotItems(slotIndex, SetIdToValue(value, StoreItemDataAtUniqueId(blockData, id)), count);
                    }
                )
            );
            return true;
        }

        public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer) {
            int id = GetIdFromValue(value);
            GigaVoltageLevelData blockData = GetItemData(id) ?? new GigaVoltageLevelData { Data = 0u };
            CounterGVElectricElement electricElement = (CounterGVElectricElement)m_subsystemGVElectricity.GetGVElectricElement(x, y, z, (Terrain.ExtractData(value) >> 2) & 7);
            if (electricElement != null) {
                DialogsManager.ShowDialog(
                    componentPlayer.GuiWidget,
                    new EditGVCounterDialog(
                        blockData,
                        electricElement,
                        current => {
                            m_subsystemGVElectricity.WritePersistentVoltage(new Point3(x, y, z), current);
                            SubsystemTerrain.ChangeCell(x, y, z, SetIdToValue(value, StoreItemDataAtUniqueId(blockData, id)));
                            electricElement.m_counter = current;
                            electricElement.m_edited = true;
                        }
                    )
                );
            }
            return true;
        }
    }
}