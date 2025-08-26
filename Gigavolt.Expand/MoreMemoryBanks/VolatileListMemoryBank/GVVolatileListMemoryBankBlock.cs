using Engine.Graphics;
using GameEntitySystem;

namespace Game {
    public class GVVolatileListMemoryBankBlock : GVListMemoryBankBlock {
        public new const int Index = 873;

        public override void Initialize() {
            RotateableMountedGVElectricElementBlockInitialize();
            m_texture = ContentManager.Get<Texture2D>("Textures/GVVolatileListMemoryBankBlock");
        }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity,
            int value,
            int x,
            int y,
            int z,
            uint subterrainId) => new VolatileListMemoryBankGVElectricElement(
            subsystemGVElectricity,
            new GVCellFace(x, y, z, GetFace(value)),
            value,
            subterrainId
        );

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain,
            ComponentMiner componentMiner,
            int value,
            TerrainRaycastResult raycastResult) {
            SubsystemGVVolatileListMemoryBankBlockBehavior subsystem =
                subsystemTerrain.Project.FindSubsystem<SubsystemGVVolatileListMemoryBankBlockBehavior>(true);
            if (subsystem.GetIdFromValue(value) == 0) {
                value = subsystem.SetIdToValue(value, subsystem.StoreItemDataAtUniqueId(new GVVolatileListMemoryBankData()));
            }
            return base.GetPlacementValue(subsystemTerrain, componentMiner, value, raycastResult);
        }

        public override int GetCustomCopyBlock(Project project, int centerValue) {
            SubsystemGVVolatileListMemoryBankBlockBehavior subsystem = project.FindSubsystem<SubsystemGVVolatileListMemoryBankBlockBehavior>(true);
            int id = subsystem.GetIdFromValue(centerValue);
            return id == 0
                ? centerValue
                : subsystem.SetIdToValue(
                    centerValue,
                    subsystem.StoreItemDataAtUniqueId((GVVolatileListMemoryBankData)subsystem.GetItemData(id).Copy())
                );
        }
    }
}