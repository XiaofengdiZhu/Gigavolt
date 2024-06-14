namespace Game {
    public class GVTruthTableCircuitCBlock : RotateableMountedGVElectricElementBlock {
        public const int Index = 812;

        public GVTruthTableCircuitCBlock() : base("Models/Gates", "TruthTableCircuit", 0.5f) { }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z, uint subterrainId) => new TruthTableCCircuitGVElectricElement(subsystemGVElectricity, new GVCellFace(x, y, z, GetFace(value)), subterrainId);

        public override GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem, int value, int face, int connectorFace, int x, int y, int z, uint subterrainId) {
            int data = Terrain.ExtractData(value);
            if (GetFace(value) == face) {
                GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(GetFace(value), GetRotation(data), connectorFace);
                if (connectorDirection == GVElectricConnectorDirection.Right
                    || connectorDirection == GVElectricConnectorDirection.Left
                    || connectorDirection == GVElectricConnectorDirection.Bottom
                    || connectorDirection == GVElectricConnectorDirection.Top) {
                    return GVElectricConnectorType.Input;
                }
                if (connectorDirection == GVElectricConnectorDirection.In) {
                    return GVElectricConnectorType.Output;
                }
            }
            return null;
        }
    }
}