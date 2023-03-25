using Engine;

namespace Game
{
    public class SubsystemGVTruthTableCircuitBlockBehavior : SubsystemEditableItemBehavior<GVTruthTableData>
    {
        public override int[] HandledBlocks => new int[1]
        {
            GVTruthTableCircuitBlock.Index
        };

        public SubsystemGVTruthTableCircuitBlockBehavior()
            : base(GVTruthTableCircuitBlock.Index)
        {
        }

        public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer)
        {
            if (componentPlayer.DragHostWidget.IsDragInProgress) return false;
            int value = inventory.GetSlotValue(slotIndex);
            int count = inventory.GetSlotCount(slotIndex);
            int id = Terrain.ExtractData(value);
            GVTruthTableData truthTableData = GetItemData(id);
            truthTableData = truthTableData != null ? (GVTruthTableData)truthTableData.Copy() : new GVTruthTableData();
            DialogsManager.ShowDialog(componentPlayer.GuiWidget, new EditGVTruthTableDialog(truthTableData, delegate
            {
                int data = StoreItemDataAtUniqueId(truthTableData);
                int value2 = Terrain.ReplaceData(value, data);
                inventory.RemoveSlotItems(slotIndex, count);
                inventory.AddSlotItems(slotIndex, value2, count);
            }));
            return true;
        }

        public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer)
        {
            GVTruthTableData truthTableData = GetBlockData(new Point3(x, y, z)) ?? new GVTruthTableData();
            DialogsManager.ShowDialog(componentPlayer.GuiWidget, new EditGVTruthTableDialog(truthTableData, delegate
            {
                SetBlockData(new Point3(x, y, z), truthTableData);
                int face = ((GVTruthTableCircuitBlock)BlocksManager.Blocks[GVTruthTableCircuitBlock.Index]).GetFace(value);
                SubsystemGVElectricity subsystemGVElectricity = SubsystemTerrain.Project.FindSubsystem<SubsystemGVElectricity>(throwOnError: true);
                GVElectricElement GVElectricElement = subsystemGVElectricity.GetGVElectricElement(x, y, z, face);
                if (GVElectricElement != null)
                {
                    subsystemGVElectricity.QueueGVElectricElementForSimulation(GVElectricElement, subsystemGVElectricity.CircuitStep + 1);
                }
            }));
            return true;
        }
    }
}
