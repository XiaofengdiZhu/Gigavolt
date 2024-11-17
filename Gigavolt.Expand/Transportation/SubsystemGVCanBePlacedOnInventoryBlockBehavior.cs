using Engine;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVCanBePlacedOnInventoryBlockBehavior : SubsystemBlockBehavior {
        public SubsystemBlockEntities m_subsystemBlockEntities;
        public override int[] HandledBlocks => [GVBlocksManager.GetBlockIndex<GVInventoryControllerBlock>(), GVBlocksManager.GetBlockIndex<GVInventoryFetcherBlock>()];

        public override bool OnUse(Ray3 ray, ComponentMiner componentMiner) {
            TerrainRaycastResult? terrainRaycastResult = componentMiner.Raycast<TerrainRaycastResult>(ray, RaycastMode.Interaction);
            if (terrainRaycastResult != null
                && m_subsystemBlockEntities.GetBlockEntity(terrainRaycastResult.Value.CellFace.X, terrainRaycastResult.Value.CellFace.Y, terrainRaycastResult.Value.CellFace.Z)?.Entity.FindComponent<ComponentInventoryBase>() != null) {
                IInventory inventory = componentMiner.Inventory;
                if (componentMiner.Place(terrainRaycastResult.Value, inventory.GetSlotValue(inventory.ActiveSlotIndex))) {
                    inventory.RemoveSlotItems(inventory.ActiveSlotIndex, 1);
                    return true;
                }
            }
            return false;
        }

        public override void Load(ValuesDictionary valuesDictionary) {
            m_subsystemBlockEntities = Project.FindSubsystem<SubsystemBlockEntities>(true);
            base.Load(valuesDictionary);
        }
    }
}