namespace Game {
    public class GVElectricConnectionPath {
        public readonly int NeighborOffsetX;

        public readonly int NeighborOffsetY;

        public readonly int NeighborOffsetZ;

        public readonly int NeighborFace;

        public readonly int ConnectorFace;

        public readonly int NeighborConnectorFace;

        public GVElectricConnectionPath(int neighborOffsetX,
            int neighborOffsetY,
            int neighborOffsetZ,
            int neighborFace,
            int connectorFace,
            int neighborConnectorFace) {
            NeighborOffsetX = neighborOffsetX;
            NeighborOffsetY = neighborOffsetY;
            NeighborOffsetZ = neighborOffsetZ;
            NeighborFace = neighborFace;
            ConnectorFace = connectorFace;
            NeighborConnectorFace = neighborConnectorFace;
        }
    }
}