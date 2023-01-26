namespace Game
{
    public interface IGVElectricElementBlock
    {
        GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectric, int value, int x, int y, int z);

        GVElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z);

        int GetConnectionMask(int value);
    }
}