namespace Game
{
    public class GVCounterBlock : RotateableMountedGVElectricElementBlock
    {
        public const int Index = 884;

        public GVCounterBlock()
            : base("Models/Gates", "Counter", 0.5f)
        {
        }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z)
        {
            return new CounterGVElectricElement(subsystemGVElectricity, new CellFace(x, y, z, GetFace(value)));
        }

        public override GVElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
        {
            int data = Terrain.ExtractData(value);
            if (GetFace(value) == face)
            {
                GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(GetFace(value), GetRotation(data), connectorFace);
                if (connectorDirection == GVElectricConnectorDirection.Right || connectorDirection == GVElectricConnectorDirection.Left || connectorDirection == GVElectricConnectorDirection.In)
                {
                    return GVElectricConnectorType.Input;
                }
                if (connectorDirection == GVElectricConnectorDirection.Top || connectorDirection == GVElectricConnectorDirection.Bottom)
                {
                    return GVElectricConnectorType.Output;
                }
            }
            return null;
        }
    }
}
