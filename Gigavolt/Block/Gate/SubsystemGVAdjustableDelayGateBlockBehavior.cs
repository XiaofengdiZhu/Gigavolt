namespace Game
{
    public class SubsystemGVAdjustableDelayGateBlockBehavior : SubsystemBlockBehavior
    {
        public override int[] HandledBlocks => new int[1]
        {
            GVAdjustableDelayGateBlock.Index
        };

        public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer)
        {
            int value = inventory.GetSlotValue(slotIndex);
            int count = inventory.GetSlotCount(slotIndex);
            int data = Terrain.ExtractData(value);
            int delay = GVAdjustableDelayGateBlock.GetDelay(data);
            DialogsManager.ShowDialog(componentPlayer.GuiWidget, new EditAdjustableDelayGateDialog(delay, delegate (int newDelay)
            {
                int data2 = GVAdjustableDelayGateBlock.SetDelay(data, newDelay);
                int num = Terrain.ReplaceData(value, data2);
                if (num != value)
                {
                    inventory.RemoveSlotItems(slotIndex, count);
                    inventory.AddSlotItems(slotIndex, num, 1);
                }
            }));
            return true;
        }

        public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer)
        {
            int data = Terrain.ExtractData(value);
            int delay = GVAdjustableDelayGateBlock.GetDelay(data);
            DialogsManager.ShowDialog(componentPlayer.GuiWidget, new EditAdjustableDelayGateDialog(delay, delegate (int newDelay)
            {
                int num = GVAdjustableDelayGateBlock.SetDelay(data, newDelay);
                if (num != data)
                {
                    int value2 = Terrain.ReplaceData(value, num);
                    SubsystemTerrain.ChangeCell(x, y, z, value2);
                    int face = ((GVAdjustableDelayGateBlock)BlocksManager.Blocks[GVAdjustableDelayGateBlock.Index]).GetFace(value);
                    SubsystemGVElectricity subsystemGVElectricity = Project.FindSubsystem<SubsystemGVElectricity>(throwOnError: true);
                    GVElectricElement GVElectricElement = subsystemGVElectricity.GetGVElectricElement(x, y, z, face);
                    if (GVElectricElement != null)
                    {
                        subsystemGVElectricity.QueueGVElectricElementForSimulation(GVElectricElement, subsystemGVElectricity.CircuitStep + 1);
                    }
                }
            }));
            return true;
        }
    }
}
