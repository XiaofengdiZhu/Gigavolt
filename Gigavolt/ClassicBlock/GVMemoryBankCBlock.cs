using GameEntitySystem;

namespace Game {
    public class GVMemoryBankCBlock : RotateableMountedGVElectricElementBlock, IGVCustomWheelPanelBlock {
        public const int Index = 811;

        public GVMemoryBankCBlock() : base("Models/Gates", "MemoryBank", 0.875f) { }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity,
            int value,
            int x,
            int y,
            int z,
            uint subterrainId) => new MemoryBankGVCElectricElement(
            subsystemGVElectricity,
            new GVCellFace(x, y, z, GetFace(value)),
            value,
            subterrainId
        );

        public override GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem,
            int value,
            int face,
            int connectorFace,
            int x,
            int y,
            int z,
            Terrain terrain) {
            int data = Terrain.ExtractData(value);
            if (GetFace(value) == face) {
                GVElectricConnectorDirection? connectorDirection =
                    SubsystemGVElectricity.GetConnectorDirection(GetFace(value), GetRotation(data), connectorFace);
                switch (connectorDirection) {
                    case GVElectricConnectorDirection.Right:
                    case GVElectricConnectorDirection.Left:
                    case GVElectricConnectorDirection.Bottom:
                    case GVElectricConnectorDirection.In: return GVElectricConnectorType.Input;
                    case GVElectricConnectorDirection.Top: return GVElectricConnectorType.Output;
                }
            }
            return null;
        }

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain,
            ComponentMiner componentMiner,
            int value,
            TerrainRaycastResult raycastResult) {
            SubsystemGVMemoryBankCBlockBehavior subsystem = subsystemTerrain.Project.FindSubsystem<SubsystemGVMemoryBankCBlockBehavior>(true);
            if (subsystem.GetIdFromValue(value) == 0) {
                value = subsystem.SetIdToValue(value, subsystem.StoreItemDataAtUniqueId(new MemoryBankData()));
            }
            return base.GetPlacementValue(subsystemTerrain, componentMiner, value, raycastResult);
        }

        public int GetCustomCopyBlock(Project project, int centerValue) {
            SubsystemGVMemoryBankCBlockBehavior subsystem = project.FindSubsystem<SubsystemGVMemoryBankCBlockBehavior>(true);
            int id = subsystem.GetIdFromValue(centerValue);
            return id == 0
                ? centerValue
                : subsystem.SetIdToValue(centerValue, subsystem.StoreItemDataAtUniqueId((MemoryBankData)subsystem.GetItemData(id).Copy()));
        }
    }
}