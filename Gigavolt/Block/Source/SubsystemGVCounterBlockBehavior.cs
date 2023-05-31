using Engine;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVCounterBlockBehavior : SubsystemEditableItemBehavior<GigaVoltageLevelData> {
        public SubsystemGVElectricity m_subsystemGVElectricity;
        public override int[] HandledBlocks => new[] { GVCounterBlock.Index };

        public SubsystemGVCounterBlockBehavior() : base(GVCounterBlock.Index) { }

        public override void Load(ValuesDictionary valuesDictionary) {
            m_subsystemGVElectricity = Project.FindSubsystem<SubsystemGVElectricity>(true);
        }

        public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer) {
            if (componentPlayer.DragHostWidget.IsDragInProgress) {
                return false;
            }
            int value = inventory.GetSlotValue(slotIndex);
            int count = inventory.GetSlotCount(slotIndex);
            int id = Terrain.ExtractData(value);
            GigaVoltageLevelData blockData = GetItemData(id);
            blockData = blockData != null ? (GigaVoltageLevelData)blockData.Copy() : new GigaVoltageLevelData { Data = 0u };
            DialogsManager.ShowDialog(
                componentPlayer.GuiWidget,
                new EditGVCounterDialog(
                    blockData,
                    null,
                    _ => {
                        int data = StoreItemDataAtUniqueId(blockData);
                        int value2 = Terrain.ReplaceData(value, data);
                        inventory.RemoveSlotItems(slotIndex, count);
                        inventory.AddSlotItems(slotIndex, value2, count);
                    }
                )
            );
            return true;
        }

        public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer) {
            GigaVoltageLevelData blockData = GetBlockData(new Point3(x, y, z)) ?? new GigaVoltageLevelData { Data = 0u };
            CounterGVElectricElement electricElement = (CounterGVElectricElement)m_subsystemGVElectricity.GetGVElectricElement(x, y, z, (Terrain.ExtractData(value) >> 2) & 7);
            if (electricElement != null) {
                DialogsManager.ShowDialog(
                    componentPlayer.GuiWidget,
                    new EditGVCounterDialog(
                        blockData,
                        electricElement,
                        current => {
                            SetBlockData(new Point3(x, y, z), blockData);
                            electricElement.m_counter = current;
                            electricElement.m_edited = true;
                            m_subsystemGVElectricity.QueueGVElectricElementForSimulation(electricElement, m_subsystemGVElectricity.CircuitStep + 1);
                        }
                    )
                );
            }
            return true;
        }
    }
}