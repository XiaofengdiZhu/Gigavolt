namespace Game
{
    public class GVMemoryBankCBlock : RotateableMountedGVElectricElementBlock
    {
        public const int Index = 686;

        public GVMemoryBankCBlock()
            : base("Models/Gates", "MemoryBank", 0.875f)
        {
        }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z)
        {
            return new MemoryBankGVCElectricElement(subsystemGVElectricity, new CellFace(x, y, z, GetFace(value)));
        }

        public override GVElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
        {
            int data = Terrain.ExtractData(value);
            if (GetFace(value) == face)
            {
                GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(GetFace(value), GetRotation(data), connectorFace);
                if (connectorDirection == GVElectricConnectorDirection.Right || connectorDirection == GVElectricConnectorDirection.Left || connectorDirection == GVElectricConnectorDirection.Bottom || connectorDirection == GVElectricConnectorDirection.In)
                {
                    return GVElectricConnectorType.Input;
                }
                if (connectorDirection == GVElectricConnectorDirection.Top)
                {
                    return GVElectricConnectorType.Output;
                }
            }
            return null;
        }
    }
}