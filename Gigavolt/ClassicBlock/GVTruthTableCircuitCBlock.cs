using GameEntitySystem;

namespace Game {
    public class GVTruthTableCircuitCBlock : RotateableMountedGVElectricElementBlock, IGVCustomWheelPanelBlock {
        public const int Index = 812;

        public GVTruthTableCircuitCBlock() : base("Models/Gates", "TruthTableCircuit", 0.5f) { }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z, uint subterrainId) => new TruthTableCCircuitGVElectricElement(subsystemGVElectricity, new GVCellFace(x, y, z, GetFace(value)), value, subterrainId);

        public override GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem, int value, int face, int connectorFace, int x, int y, int z, uint subterrainId) {
            int data = Terrain.ExtractData(value);
            if (GetFace(value) == face) {
                GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(GetFace(value), GetRotation(data), connectorFace);
                switch (connectorDirection) {
                    case GVElectricConnectorDirection.Right:
                    case GVElectricConnectorDirection.Left:
                    case GVElectricConnectorDirection.Bottom:
                    case GVElectricConnectorDirection.Top: return GVElectricConnectorType.Input;
                    case GVElectricConnectorDirection.In: return GVElectricConnectorType.Output;
                }
            }
            return null;
        }

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult) {
            SubsystemGVTruthTableCircuitCBlockBehavior subsystem = subsystemTerrain.Project.FindSubsystem<SubsystemGVTruthTableCircuitCBlockBehavior>(true);
            if (subsystem.GetIdFromValue(value) == 0) {
                value = subsystem.SetIdToValue(value, subsystem.StoreItemDataAtUniqueId(new TruthTableData()));
            }
            return base.GetPlacementValue(subsystemTerrain, componentMiner, value, raycastResult);
        }

        public int GetCustomCopyBlock(Project project, int centerValue) {
            SubsystemGVTruthTableCircuitCBlockBehavior subsystem = project.FindSubsystem<SubsystemGVTruthTableCircuitCBlockBehavior>(true);
            int id = subsystem.GetIdFromValue(centerValue);
            return id == 0 ? centerValue : subsystem.SetIdToValue(centerValue, subsystem.StoreItemDataAtUniqueId((TruthTableData)subsystem.GetItemData(id).Copy()));
        }
    }
}