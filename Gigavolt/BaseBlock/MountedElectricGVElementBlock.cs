namespace Game {
    public abstract class MountedGVElectricElementBlock : Block, IGVElectricElementBlock {
        public abstract int GetFace(int value);

        public abstract GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z, uint subterrainId);

        public abstract GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem, int value, int face, int connectorFace, int x, int y, int z, uint subterrainId);

        public virtual int GetConnectionMask(int value) => int.MaxValue;
    }
}