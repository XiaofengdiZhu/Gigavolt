namespace Game
{
    public class GVSRLatchCBlock : RotateableMountedGVElectricElementBlock
    {
        public const int Index = 646;

        public GVSRLatchCBlock()
            : base("Models/Gates", "SRLatch", 0.375f)
        {
        }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z)
        {
            return new SRLatchGVCElectricElement(subsystemGVElectricity, new CellFace(x, y, z, GetFace(value)));
        }

        public override GVElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
        {
            int data = Terrain.ExtractData(value);
            if (GetFace(value) == face)
            {
                GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(GetFace(value), GetRotation(data), connectorFace);
                if (connectorDirection == GVElectricConnectorDirection.Right || connectorDirection == GVElectricConnectorDirection.Left || connectorDirection == GVElectricConnectorDirection.Bottom)
                {
                    return GVElectricConnectorType.Input;
                }
                if (connectorDirection == GVElectricConnectorDirection.Top || connectorDirection == GVElectricConnectorDirection.In)
                {
                    return GVElectricConnectorType.Output;
                }
            }
            return null;
        }
    }
}
