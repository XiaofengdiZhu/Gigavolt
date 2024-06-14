namespace Game {
    public class GVRealTimeClockBlock : RotateableMountedGVElectricElementBlock {
        public const int Index = 849;

        public GVRealTimeClockBlock() : base("Models/Gates", "RealTimeClock", 0.5f) { }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z, uint subterrainId) => new RealTimeClockGVElectricElement(subsystemGVElectricity, new GVCellFace(x, y, z, GetFace(value)), subterrainId);

        public override GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem, int value, int face, int connectorFace, int x, int y, int z, uint subterrainId) {
            int data = Terrain.ExtractData(value);
            if (GetFace(value) == face) {
                GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(GetFace(value), GetRotation(data), connectorFace);
                if (connectorDirection == GVElectricConnectorDirection.Top
                    || connectorDirection == GVElectricConnectorDirection.Right
                    || connectorDirection == GVElectricConnectorDirection.Left
                    || connectorDirection == GVElectricConnectorDirection.Bottom) {
                    return GVElectricConnectorType.Output;
                }
                if (connectorDirection == GVElectricConnectorDirection.In) {
                    return GVElectricConnectorType.Input;
                }
            }
            return null;
        }
    }
}