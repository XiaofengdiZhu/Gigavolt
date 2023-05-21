namespace Game {
    public class GVTruthTableCircuitCBlock : RotateableMountedGVElectricElementBlock {
        public const int Index = 688;

        public GVTruthTableCircuitCBlock() : base("Models/Gates", "TruthTableCircuit", 0.5f) { }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z) => new TruthTableCCircuitGVElectricElement(subsystemGVElectricity, new CellFace(x, y, z, GetFace(value)));

        public override GVElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z) {
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