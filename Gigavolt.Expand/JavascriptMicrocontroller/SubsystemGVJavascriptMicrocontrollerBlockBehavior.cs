using Engine;

namespace Game {
    public class SubsystemGVJavascriptMicrocontrollerBlockBehavior : SubsystemEditableItemBehavior<GVJavascriptMicrocontrollerData> {
        public override int[] HandledBlocks => new[] { GVJavascriptMicrocontrollerBlock.Index };

        public SubsystemGVJavascriptMicrocontrollerBlockBehavior() : base(GVJavascriptMicrocontrollerBlock.Index) { }

        public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer) {
            if (componentPlayer.DragHostWidget.IsDragInProgress) {
                return false;
            }
            int value = inventory.GetSlotValue(slotIndex);
            int count = inventory.GetSlotCount(slotIndex);
            int id = Terrain.ExtractData(value);
            GVJavascriptMicrocontrollerData javascriptMicrocontrollerData = GetItemData(id);
            javascriptMicrocontrollerData = javascriptMicrocontrollerData != null ? (GVJavascriptMicrocontrollerData)javascriptMicrocontrollerData.Copy() : new GVJavascriptMicrocontrollerData();
            DialogsManager.ShowDialog(
                componentPlayer.GuiWidget,
                new EditGVJavascriptMicrocontrollerDialog(
                    javascriptMicrocontrollerData,
                    delegate {
                        int data = StoreItemDataAtUniqueId(javascriptMicrocontrollerData);
                        int value2 = Terrain.ReplaceData(value, data);
                        inventory.RemoveSlotItems(slotIndex, count);
                        inventory.AddSlotItems(slotIndex, value2, count);
                    }
                )
            );
            return true;
        }

        public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer) {
            GVJavascriptMicrocontrollerData javascriptMicrocontrollerData = GetBlockData(new Point3(x, y, z)) ?? new GVJavascriptMicrocontrollerData();
            DialogsManager.ShowDialog(
                componentPlayer.GuiWidget,
                new EditGVJavascriptMicrocontrollerDialog(
                    javascriptMicrocontrollerData,
                    delegate {
                        SetBlockData(new Point3(x, y, z), javascriptMicrocontrollerData);
                        int face = ((GVJavascriptMicrocontrollerBlock)BlocksManager.Blocks[GVJavascriptMicrocontrollerBlock.Index]).GetFace(value);
                        TerrainChunk chunkAtCell = SubsystemTerrain.Terrain.GetChunkAtCell(x, z);
                        ++chunkAtCell.ModificationCounter;
                        SubsystemTerrain.TerrainUpdater.DowngradeChunkNeighborhoodState(chunkAtCell.Coords, 1, TerrainChunkState.InvalidLight, true);
                        SubsystemTerrain.m_modifiedCells[new Point3(x, y, z)] = true;
                        SubsystemGVElectricity subsystemGVElectricity = SubsystemTerrain.Project.FindSubsystem<SubsystemGVElectricity>(true);
                        GVElectricElement GVElectricElement = subsystemGVElectricity.GetGVElectricElement(x, y, z, face);
                        if (GVElectricElement != null) {
                            GVElectricElement.GetOutputVoltage(123456);
                            subsystemGVElectricity.QueueGVElectricElementForSimulation(GVElectricElement, subsystemGVElectricity.CircuitStep + 1);
                        }
                    }
                )
            );
            return true;
        }
    }
}