using System;
using Engine;

namespace Game {
    public class SubsystemGVVolatileFourDimensionalMemoryBankBlockBehavior : SubsystemEditableItemBehavior<GVVolatileFourDimensionalMemoryBankData> {
        public override int[] HandledBlocks => new[] { GVVolatileFourDimensionalMemoryBankBlock.Index };
        public SubsystemGVVolatileFourDimensionalMemoryBankBlockBehavior() : base(GVVolatileListMemoryBankBlock.Index) { }

        public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer) {
            try {
                bool isDragInProgress = componentPlayer.DragHostWidget.IsDragInProgress;
                if (isDragInProgress) {
                    return false;
                }
                int value = inventory.GetSlotValue(slotIndex);
                int count = inventory.GetSlotCount(slotIndex);
                int id = Terrain.ExtractData(value);
                GVVolatileFourDimensionalMemoryBankData memoryBankData = GetItemData(id);
                memoryBankData = memoryBankData ?? new GVVolatileFourDimensionalMemoryBankData(GVStaticStorage.GetUniqueGVMBID());
                DialogsManager.ShowDialog(
                    componentPlayer.GuiWidget,
                    new EditGVFourDimensionalMemoryBankDialog(
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
                GVVolatileFourDimensionalMemoryBankData memoryBankData = GetBlockData(new Point3(x, y, z)) ?? new GVVolatileFourDimensionalMemoryBankData(GVStaticStorage.GetUniqueGVMBID());
                DialogsManager.ShowDialog(
                    componentPlayer.GuiWidget,
                    new EditGVFourDimensionalMemoryBankDialog(
                        memoryBankData,
                        delegate {
                            SetBlockData(new Point3(x, y, z), memoryBankData);
                            int face = ((GVVolatileListMemoryBankBlock)BlocksManager.Blocks[GVVolatileListMemoryBankBlock.Index]).GetFace(value);
                            SubsystemGVElectricity subsystemGVElectricity = SubsystemTerrain.Project.FindSubsystem<SubsystemGVElectricity>(true);
                            GVElectricElement GVElectricElement = subsystemGVElectricity.GetGVElectricElement(
                                x,
                                y,
                                z,
                                face,
                                0
                            );
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