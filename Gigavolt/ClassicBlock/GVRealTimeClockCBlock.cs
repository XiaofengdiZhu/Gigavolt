namespace Game {
    public class GVRealTimeClockCBlock : RotateableMountedGVElectricElementBlock {
        public const int Index = 813;

        public GVRealTimeClockCBlock() : base("Models/Gates", "RealTimeClock", 0.5f) { }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z) => new RealTimeClockGVCElectricElement(subsystemGVElectricity, new CellFace(x, y, z, GetFace(value)));

        public override GVElectricConnectorType? GetGVConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z) {
            int data = Terrain.ExtractData(value);
            if (GetFace(value) == face) {
                GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(GetFace(value), GetRotation(data), connectorFace);
                if (connectorDirection == GVElectricConnectorDirection.Top
                    || connectorDirection == GVElectricConnectorDirection.Right
                    || connectorDirection == GVElectricConnectorDirection.Left
                    || connectorDirection == GVElectricConnectorDirection.Bottom
                    || connectorDirection == GVElectricConnectorDirection.In) {
                    return GVElectricConnectorType.Output;
                }
            }
            return null;
        }
    }
}