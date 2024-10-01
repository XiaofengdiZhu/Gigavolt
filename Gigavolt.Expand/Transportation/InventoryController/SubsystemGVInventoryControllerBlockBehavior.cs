using Engine;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVInventoryControllerBlockBehavior : SubsystemBlockBehavior {
        public SubsystemBlockEntities m_subsystemBlockEntities;
        public override int[] HandledBlocks => [BlocksManager.GetBlockIndex<GVInventoryControllerBlock>()];

        public override bool OnUse(Ray3 ray, ComponentMiner componentMiner) {
            TerrainRaycastResult? terrainRaycastResult = componentMiner.Raycast<TerrainRaycastResult>(ray, RaycastMode.Interaction);
            if (terrainRaycastResult != null
                && m_subsystemBlockEntities.GetBlockEntity(terrainRaycastResult.Value.CellFace.X, terrainRaycastResult.Value.CellFace.Y, terrainRaycastResult.Value.CellFace.Z)?.Entity.FindComponent<ComponentInventoryBase>() != null
                && componentMiner.Place(terrainRaycastResult.Value, GVBlocksManager.GetBlockIndex<GVInventoryControllerBlock>())) {
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