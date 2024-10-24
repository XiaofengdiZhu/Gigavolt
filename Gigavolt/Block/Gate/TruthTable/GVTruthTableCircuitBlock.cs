using GameEntitySystem;

namespace Game {
    public class GVTruthTableCircuitBlock : RotateableMountedGVElectricElementBlock, IGVCustomWheelPanelBlock {
        public const int Index = 848;

        public GVTruthTableCircuitBlock() : base("Models/Gates", "TruthTableCircuit", 0.5f) { }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z, uint subterrainId) => new TruthTableCircuitGVElectricElement(subsystemGVElectricity, new GVCellFace(x, y, z, GetFace(value)), value, subterrainId);

        public override GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem, int value, int face, int connectorFace, int x, int y, int z, Terrain terrain) {
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

        public override bool IsNonDuplicable_(int value) => Terrain.ExtractData(value) > 0;

        public int GetCustomCopyBlock(Project project, int centerValue) {
            SubsystemGVTruthTableCircuitBlockBehavior subsystem = project.FindSubsystem<SubsystemGVTruthTableCircuitBlockBehavior>(true);
            int id = subsystem.GetIdFromValue(centerValue);
            return id == 0 ? centerValue : subsystem.SetIdToValue(centerValue, subsystem.StoreItemDataAtUniqueId((GVTruthTableData)subsystem.GetItemData(id).Copy()));
        }
    }
}