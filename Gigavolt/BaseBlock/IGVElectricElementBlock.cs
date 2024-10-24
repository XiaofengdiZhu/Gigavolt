namespace Game {
    public interface IGVElectricElementBlock {
        GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z, uint subterrainId) => null;
        GVElectricElement[] CreateGVElectricElements(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z, uint subterrainId) => null;

        GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem, int value, int face, int connectorFace, int x, int y, int z, Terrain terrain);

        int GetConnectionMask(int value);
    }
}