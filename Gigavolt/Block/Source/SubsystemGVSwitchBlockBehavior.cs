using Engine;

namespace Game
{
	public class SubsystemGVSwitchBlockBehavior : SubsystemEditableItemBehavior<GigaVoltageLevelData>
    {
		public override int[] HandledBlocks => new int[1] { 841 };
		public SubsystemGVSwitchBlockBehavior() : base(841) { }
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
                int face = ((GVSwitchBlock)BlocksManager.Blocks[841]).GetFace(value);
                SubsystemGVElectricity subsystemGVElectricity = SubsystemTerrain.Project.FindSubsystem<SubsystemGVElectricity>(throwOnError: true);
                GVElectricElement electricElement = subsystemGVElectricity.GetGVElectricElement(x, y, z, face);
                if (electricElement != null)
                {
                    subsystemGVElectricity.QueueGVElectricElementForSimulation(electricElement, subsystemGVElectricity.CircuitStep + 1);
                }
            }));
			return true;
		}
        public override void OnItemHarvested(int x, int y, int z, int blockValue, ref BlockDropValue dropValue, ref int newBlockValue)
        {
            GigaVoltageLevelData blockData = GetBlockData(new Point3(x, y, z));
            if (blockData != null)
            {
                int num = FindFreeItemId();
                m_itemsData.Add(num, (GigaVoltageLevelData)blockData.Copy());
                dropValue.Value = Terrain.ReplaceData(dropValue.Value, (num & 1023) << 1);
            }
        }
        public override void OnItemPlaced(int x, int y, int z, ref BlockPlacementData placementData, int itemValue)
        {
            int id = Terrain.ExtractData(itemValue);
            GigaVoltageLevelData itemData = GetItemData((id >> 1) & 1023);
            if (itemData != null)
            {
                m_blocksData[new Point3(x, y, z)] = (GigaVoltageLevelData)itemData.Copy();
            }
        }
    }
}
