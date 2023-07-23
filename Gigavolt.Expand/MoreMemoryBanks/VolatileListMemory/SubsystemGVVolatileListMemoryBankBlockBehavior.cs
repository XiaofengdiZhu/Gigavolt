using System;
using Engine;

namespace Game {
    public class SubsystemGVVolatileListMemoryBankBlockBehavior : SubsystemEditableItemBehavior<GVVolatileListMemoryBankData> {
        public override int[] HandledBlocks => new[] { GVVolatileListMemoryBankBlock.Index };

        public SubsystemGVVolatileListMemoryBankBlockBehavior() : base(GVVolatileListMemoryBankBlock.Index) { }

        public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer) {
            try {
                bool isDragInProgress = componentPlayer.DragHostWidget.IsDragInProgress;
                if (isDragInProgress) {
                    return false;
                }
                int value = inventory.GetSlotValue(slotIndex);
                int count = inventory.GetSlotCount(slotIndex);
                int id = Terrain.ExtractData(value);
                GVVolatileListMemoryBankData memoryBankData = GetItemData(id);
                memoryBankData = memoryBankData ?? new GVVolatileListMemoryBankData(GVStaticStorage.GetUniqueGVMBID());
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
            }
            catch (Exception e) {
                Log.Error(e);
            }
            return true;
        }

        public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer) {
            try {
                GVVolatileListMemoryBankData memoryBankData = GetBlockData(new Point3(x, y, z)) ?? new GVVolatileListMemoryBankData(GVStaticStorage.GetUniqueGVMBID());
                DialogsManager.ShowDialog(
                    componentPlayer.GuiWidget,
                    new EditGVListMemoryBankDialog(
                        memoryBankData,
                        delegate {
                            SetBlockData(new Point3(x, y, z), memoryBankData);
                            int face = ((GVVolatileListMemoryBankBlock)BlocksManager.Blocks[GVVolatileListMemoryBankBlock.Index]).GetFace(value);
                            SubsystemGVElectricity subsystemGVElectricity = SubsystemTerrain.Project.FindSubsystem<SubsystemGVElectricity>(true);
                            GVElectricElement GVElectricElement = subsystemGVElectricity.GetGVElectricElement(x, y, z, face);
                            if (GVElectricElement != null) {
                                subsystemGVElectricity.QueueGVElectricElementForSimulation(GVElectricElement, subsystemGVElectricity.CircuitStep + 1);
                            }
                        }
                    )
                );
            }
            catch (Exception e) {
                Log.Error(e);
            }
            return true;
        }
    }
}