using Engine.Graphics;
using GameEntitySystem;

namespace Game {
    public class GVVolatileFourDimensionalMemoryBankBlock : GVFourDimensionalMemoryBankBlock {
        public new const int Index = 892;

        public override void Initialize() {
            RotateableMountedGVElectricElementBlockInitialize();
            m_texture = ContentManager.Get<Texture2D>("Textures/GVVolatileFourDimensionalMemoryBankBlock");
        }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity,
            int value,
            int x,
            int y,
            int z,
            uint subterrainId) => new VolatileFourDimensionalMemoryBankGVElectricElement(
            subsystemGVElectricity,
            new GVCellFace(x, y, z, GetFace(value)),
            value,
            subterrainId
        );

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain,
            ComponentMiner componentMiner,
            int value,
            TerrainRaycastResult raycastResult) {
            SubsystemGVVolatileFourDimensionalMemoryBankBlockBehavior subsystem =
                subsystemTerrain.Project.FindSubsystem<SubsystemGVVolatileFourDimensionalMemoryBankBlockBehavior>(true);
            if (subsystem.GetIdFromValue(value) == 0) {
                value = subsystem.SetIdToValue(value, subsystem.StoreItemDataAtUniqueId(new GVVolatileFourDimensionalMemoryBankData()));
            }
            return base.GetPlacementValue(subsystemTerrain, componentMiner, value, raycastResult);
        }

        public override int GetCustomCopyBlock(Project project, int centerValue) {
            SubsystemGVVolatileFourDimensionalMemoryBankBlockBehavior subsystem =
                project.FindSubsystem<SubsystemGVVolatileFourDimensionalMemoryBankBlockBehavior>(true);
            int id = subsystem.GetIdFromValue(centerValue);
            return id == 0
                ? centerValue
                : subsystem.SetIdToValue(
                    centerValue,
                    subsystem.StoreItemDataAtUniqueId((GVVolatileFourDimensionalMemoryBankData)subsystem.GetItemData(id).Copy())
                );
        }
    }
}