using Engine;

namespace Game
{
	public class SubsystemGVBatteryBlockBehavior : SubsystemEditableItemBehavior<GigaVoltageLevelData>
    {
		public override int[] HandledBlocks => new int[1] { GVBatteryBlock.Index };
		public SubsystemGVBatteryBlockBehavior() : base(GVBatteryBlock.Index) { }
		public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer)
		{
			if (componentPlayer.DragHostWidget.IsDragInProgress) return false;
			int value = inventory.GetSlotValue(slotIndex);
			int count = inventory.GetSlotCount(slotIndex);
			int id = Terrain.ExtractData(value)>>1;
            GigaVoltageLevelData blockData = GetItemData(id);
            blockData = blockData != null ? (GigaVoltageLevelData)blockData.Copy() : new GigaVoltageLevelData();
            DialogsManager.ShowDialog(componentPlayer.GuiWidget, new EditGigaVoltageLevelDialog(blockData, delegate
			{
                int data = StoreItemDataAtUniqueId(blockData);
                int value2 = Terrain.ReplaceData(value, data<<1);
                inventory.RemoveSlotItems(slotIndex, count);
                inventory.AddSlotItems(slotIndex, value2, count);
            }));
			return true;
		}

		public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer)
		{
            GigaVoltageLevelData blockData = GetBlockData(new Point3(x, y, z)) ?? new GigaVoltageLevelData();
            DialogsManager.ShowDialog(componentPlayer.GuiWidget, new EditGigaVoltageLevelDialog(blockData, delegate
            {
                SetBlockData(new Point3(x, y, z), blockData);
                SubsystemGVElectricity subsystemGVElectricity = SubsystemTerrain.Project.FindSubsystem<SubsystemGVElectricity>(throwOnError: true);
                GVElectricElement electricElement = subsystemGVElectricity.GetGVElectricElement(x, y, z, 4);
                if (electricElement != null)
                {
                    subsystemGVElectricity.QueueGVElectricElementForSimulation(electricElement, subsystemGVElectricity.CircuitStep + 1);
                }
            }));
			return true;
		}
	}
}
