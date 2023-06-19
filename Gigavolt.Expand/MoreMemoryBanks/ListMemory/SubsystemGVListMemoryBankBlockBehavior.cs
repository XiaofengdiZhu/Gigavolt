using Engine;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVListMemoryBankBlockBehavior : SubsystemEditableItemBehavior<GVListMemoryBankData> {
        public SubsystemGameInfo m_subsystemGameInfo;

        public override void Load(ValuesDictionary valuesDictionary) {
            base.Load(valuesDictionary);
            m_subsystemGameInfo = Project.FindSubsystem<SubsystemGameInfo>(true);
        }

        public override int[] HandledBlocks => new[] { GVListMemoryBankBlock.Index };

        public SubsystemGVListMemoryBankBlockBehavior() : base(GVListMemoryBankBlock.Index) { }

        public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer) {
            bool isDragInProgress = componentPlayer.DragHostWidget.IsDragInProgress;
            if (isDragInProgress) {
                return false;
            }
            int value = inventory.GetSlotValue(slotIndex);
            int count = inventory.GetSlotCount(slotIndex);
            int id = Terrain.ExtractData(value);
            GVListMemoryBankData memoryBankData = GetItemData(id);
            memoryBankData = memoryBankData ?? new GVListMemoryBankData(GVStaticStorage.GetUniqueGVMBID(), m_subsystemGameInfo.DirectoryName);
            if (memoryBankData.m_worldDirectory == null) {
                memoryBankData.m_worldDirectory = m_subsystemGameInfo.DirectoryName;
                memoryBankData.LoadData();
            }
            DialogsManager.ShowDialog(
                componentPlayer.GuiWidget,
                new EditGVListMemoryBankDialog(
                    memoryBankData,
                    delegate {
                        int data = StoreItemDataAtUniqueId(memoryBankData);
                        int value2 = Terrain.ReplaceData(value, data);
                        inventory.RemoveSlotItems(slotIndex, count);
                        inventory.AddSlotItems(slotIndex, value2, count);
                    }
                )
            );
            return true;
        }

        public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer) {
            GVListMemoryBankData memoryBankData = GetBlockData(new Point3(x, y, z)) ?? new GVListMemoryBankData(GVStaticStorage.GetUniqueGVMBID(), m_subsystemGameInfo.DirectoryName);
            if (memoryBankData.m_worldDirectory == null) {
                memoryBankData.m_worldDirectory = m_subsystemGameInfo.DirectoryName;
                memoryBankData.LoadData();
                SetBlockData(new Point3(x, y, z), memoryBankData);
            }
            DialogsManager.ShowDialog(
                componentPlayer.GuiWidget,
                new EditGVListMemoryBankDialog(
                    memoryBankData,
                    delegate {
                        SetBlockData(new Point3(x, y, z), memoryBankData);
                        int face = ((GVListMemoryBankBlock)BlocksManager.Blocks[GVListMemoryBankBlock.Index]).GetFace(value);
                        SubsystemGVElectricity subsystemGVElectricity = SubsystemTerrain.Project.FindSubsystem<SubsystemGVElectricity>(true);
                        GVElectricElement GVElectricElement = subsystemGVElectricity.GetGVElectricElement(x, y, z, face);
                        if (GVElectricElement != null) {
                            subsystemGVElectricity.QueueGVElectricElementForSimulation(GVElectricElement, subsystemGVElectricity.CircuitStep + 1);
                        }
                    }
                )
            );
            return true;
        }
    }
}