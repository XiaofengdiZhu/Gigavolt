namespace Game {
    public abstract class MountedGVElectricElementBlock : GVBaseBlock, IGVElectricElementBlock {
        public abstract int GetFace(int value);

        public virtual GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z, uint subterrainId) => null;
        public virtual GVElectricElement[] CreateGVElectricElements(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z, uint subterrainId) => null;

        public abstract GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem, int value, int face, int connectorFace, int x, int y, int z, Terrain terrain);

        public virtual int GetConnectionMask(int value) => int.MaxValue;

        public override bool IsMovableByPiston(int value, int pistonFace, int y, out bool isEnd) {
            isEnd = true;
            Block block = BlocksManager.Blocks[Terrain.ExtractContents(value)];
            return ((MountedGVElectricElementBlock)block).GetFace(value) == pistonFace;
        }
    }
}