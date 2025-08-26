namespace Game {
    public class SubsystemGVDataModifierProjectileBlockBehavior : SubsystemBlockBehavior {
        public override int[] HandledBlocks => [GVBlocksManager.GetBlockIndex<GVDataModifierProjectileBlock>()];

        public override bool OnHitAsProjectile(CellFace? cellFace, ComponentBody componentBody, WorldItem worldItem) {
            if (cellFace.HasValue) {
                SubsystemTerrain.ChangeCell(
                    cellFace.Value.X,
                    cellFace.Value.Y,
                    cellFace.Value.Z,
                    Terrain.MakeBlockValue(
                        SubsystemTerrain.Terrain.GetCellValueFast(cellFace.Value.X, cellFace.Value.Y, cellFace.Value.Z),
                        SubsystemTerrain.Terrain.GetCellLightFast(cellFace.Value.X, cellFace.Value.Y, cellFace.Value.Z),
                        Terrain.ExtractData(worldItem.Value)
                    )
                );
                return true;
            }
            return false;
        }

        public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer) {
            bool isDragInProgress = componentPlayer.DragHostWidget.IsDragInProgress;
            if (isDragInProgress) {
                return false;
            }
            int value = inventory.GetSlotValue(slotIndex);
            int count = inventory.GetSlotCount(slotIndex);
            DialogsManager.ShowDialog(
                componentPlayer.GuiWidget,
                new EditGVDataModifierProjectileDialog(
                    Terrain.ExtractData(value),
                    delegate(int data) {
                        int value2 = Terrain.ReplaceData(value, data);
                        inventory.RemoveSlotItems(slotIndex, count);
                        inventory.AddSlotItems(slotIndex, value2, count);
                    }
                )
            );
            return true;
        }
    }
}