using Engine;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVInventoryControllerBlockBehavior : SubsystemBlockBehavior {
        public SubsystemBlockEntities m_subsystemBlockEntities;
        public override int[] HandledBlocks => [GVInventoryControllerBlock.Index];

        public override bool OnUse(Ray3 ray, ComponentMiner componentMiner) {
            TerrainRaycastResult? terrainRaycastResult = componentMiner.Raycast<TerrainRaycastResult>(ray, RaycastMode.Interaction);
            if (terrainRaycastResult != null
                && m_subsystemBlockEntities.GetBlockEntity(terrainRaycastResult.Value.CellFace.X, terrainRaycastResult.Value.CellFace.Y, terrainRaycastResult.Value.CellFace.Z)?.Entity.FindComponent<ComponentInventoryBase>() != null
                && componentMiner.Place(terrainRaycastResult.Value, GVInventoryControllerBlock.Index)) {
                IInventory inventory = componentMiner.Inventory;
                inventory.RemoveSlotItems(inventory.ActiveSlotIndex, 1);
                return true;
            }
            return false;
        }

        public override void Load(ValuesDictionary valuesDictionary) {
            m_subsystemBlockEntities = Project.FindSubsystem<SubsystemBlockEntities>(true);
            base.Load(valuesDictionary);
        }
    }
}