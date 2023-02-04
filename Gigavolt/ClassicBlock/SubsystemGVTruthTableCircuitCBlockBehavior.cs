using Engine;

namespace Game
{
    public class SubsystemGVTruthTableCircuitCBlockBehavior : SubsystemEditableItemBehavior<GVTruthTableCData>
    {
        public override int[] HandledBlocks => new int[1]
        {
            788
        };

        public SubsystemGVTruthTableCircuitCBlockBehavior()
            : base(788)
        {
        }

        public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer)
        {
            if (componentPlayer.DragHostWidget.IsDragInProgress) return false;
            int value = inventory.GetSlotValue(slotIndex);
            int count = inventory.GetSlotCount(slotIndex);
            int id = Terrain.ExtractData(value);
            GVTruthTableCData TruthTableCData = GetItemData(id);
            TruthTableCData = TruthTableCData != null ? (GVTruthTableCData)TruthTableCData.Copy() : new GVTruthTableCData();
            DialogsManager.ShowDialog(componentPlayer.GuiWidget, new EditGVTruthTableCDialog(TruthTableCData, delegate
            {
                int data = StoreItemDataAtUniqueId(TruthTableCData);
                int value2 = Terrain.ReplaceData(value, data);
                inventory.RemoveSlotItems(slotIndex, count);
                inventory.AddSlotItems(slotIndex, value2, count);
            }));
            return true;
        }

        public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer)
        {
            GVTruthTableCData TruthTableCData = GetBlockData(new Point3(x, y, z)) ?? new GVTruthTableCData();
            DialogsManager.ShowDialog(componentPlayer.GuiWidget, new EditGVTruthTableCDialog(TruthTableCData, delegate
            {
                SetBlockData(new Point3(x, y, z), TruthTableCData);
                int face = ((GVTruthTableCircuitCBlock)BlocksManager.Blocks[788]).GetFace(value);
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
