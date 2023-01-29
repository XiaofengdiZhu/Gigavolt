namespace Game
{
    public class GVDigitalToAnalogConverterBlock : RotateableMountedGVElectricElementBlock
    {
        public const int Index = 880;

        public GVDigitalToAnalogConverterBlock()
            : base("Models/Gates", "DigitalToAnalogConverter", 0.375f)
        {
        }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z)
        {
            return new DigitalToAnalogConverterGVElectricElement(subsystemGVElectricity, new CellFace(x, y, z, GetFace(value)));
        }

        public override GVElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
        {
            int data = Terrain.ExtractData(value);
            if (GetFace(value) == face)
            {
                GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(GetFace(value), GetRotation(data), connectorFace);
                if (connectorDirection == GVElectricConnectorDirection.In)
                {
                    return GVElectricConnectorType.Output;
                }
                if (connectorDirection == GVElectricConnectorDirection.Bottom || connectorDirection == GVElectricConnectorDirection.Top || connectorDirection == GVElectricConnectorDirection.Right || connectorDirection == GVElectricConnectorDirection.Left)
                {
                    return GVElectricConnectorType.Input;
                }
            }
            return null;
        }
    }
}
