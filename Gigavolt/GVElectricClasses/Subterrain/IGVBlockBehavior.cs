namespace Game {
    public interface IGVBlockBehavior {
        public void OnBlockRemoved(int value, int newValue, int x, int y, int z, GVSubterrainSystem system) { }
        public void OnBlockAdded(int value, int oldValue, int x, int y, int z, GVSubterrainSystem system) { }
        public void OnBlockModified(int value, int oldValue, int x, int y, int z, GVSubterrainSystem system) { }

        public void OnNeighborBlockChanged(int x,
            int y,
            int z,
            int neighborX,
            int neighborY,
            int neighborZ,
            GVSubterrainSystem system) { }

        public void OnBlockGenerated(int value, int x, int y, int z, bool isLoaded, GVSubterrainSystem system) { }
    }
}