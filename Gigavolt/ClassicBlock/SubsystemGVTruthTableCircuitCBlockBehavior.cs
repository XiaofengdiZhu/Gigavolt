using Engine;

namespace Game {
    public class SubsystemGVTruthTableCircuitCBlockBehavior : SubsystemEditableItemBehavior<TruthTableData> {
        public override int[] HandledBlocks => new[] { GVTruthTableCircuitCBlock.Index };

        public SubsystemGVTruthTableCircuitCBlockBehavior() : base(GVTruthTableCircuitCBlock.Index) { }

        public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer) {
            if (componentPlayer.DragHostWidget.IsDragInProgress) {
                return false;
            }
            int value = inventory.GetSlotValue(slotIndex);
            int count = inventory.GetSlotCount(slotIndex);
            int id = Terrain.ExtractData(value);
            TruthTableData TruthTableData = GetItemData(id);
            TruthTableData = TruthTableData != null ? (TruthTableData)TruthTableData.Copy() : new TruthTableData();
            DialogsManager.ShowDialog(
                componentPlayer.GuiWidget,
                new EditTruthTableDialog(
                    TruthTableData,
                    delegate {
                        int data = StoreItemDataAtUniqueId(TruthTableData);
                        int value2 = Terrain.ReplaceData(value, data);
                        inventory.RemoveSlotItems(slotIndex, count);
                        inventory.AddSlotItems(slotIndex, value2, count);
                    }
                )
            );
            return true;
        }

        public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer) {
            TruthTableData TruthTableData = GetBlockData(new Point3(x, y, z)) ?? new TruthTableData();
            DialogsManager.ShowDialog(
                componentPlayer.GuiWidget,
                new EditTruthTableDialog(
                    TruthTableData,
                    delegate {
                        SetBlockData(new Point3(x, y, z), TruthTableData);
                        int face = ((GVTruthTableCircuitCBlock)BlocksManager.Blocks[GVTruthTableCircuitCBlock.Index]).GetFace(value);
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
            return true;
        }
    }
}