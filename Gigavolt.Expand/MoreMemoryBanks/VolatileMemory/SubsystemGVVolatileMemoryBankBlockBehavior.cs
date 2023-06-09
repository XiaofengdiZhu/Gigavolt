using Engine;

namespace Game {
    public class SubsystemGVVolatileMemoryBankBlockBehavior : SubsystemEditableItemBehavior<GVVolatileMemoryBankData> {
        public override int[] HandledBlocks => new[] { GVVolatileMemoryBankBlock.Index };

        public SubsystemGVVolatileMemoryBankBlockBehavior() : base(GVVolatileMemoryBankBlock.Index) { }

        public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer) {
            bool isDragInProgress = componentPlayer.DragHostWidget.IsDragInProgress;
            if (isDragInProgress) {
                return false;
            }
            int value = inventory.GetSlotValue(slotIndex);
            int count = inventory.GetSlotCount(slotIndex);
            int id = Terrain.ExtractData(value);
            GVVolatileMemoryBankData memoryBankData = GetItemData(id);
            memoryBankData = memoryBankData != null ? (GVVolatileMemoryBankData)memoryBankData.Copy() : new GVVolatileMemoryBankData(GVStaticStorage.GetUniqueGVMBID());
            DialogsManager.ShowDialog(
                componentPlayer.GuiWidget,
                new EditGVMemoryBankDialog(
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
            GVVolatileMemoryBankData memoryBankData = GetBlockData(new Point3(x, y, z)) ?? new GVVolatileMemoryBankData(GVStaticStorage.GetUniqueGVMBID());
            DialogsManager.ShowDialog(
                componentPlayer.GuiWidget,
                new EditGVMemoryBankDialog(
                    memoryBankData,
                    delegate {
                        SetBlockData(new Point3(x, y, z), memoryBankData);
                        int face = ((GVVolatileMemoryBankBlock)BlocksManager.Blocks[GVVolatileMemoryBankBlock.Index]).GetFace(value);
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