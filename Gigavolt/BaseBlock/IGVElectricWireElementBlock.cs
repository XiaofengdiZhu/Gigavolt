namespace Game {
    public interface IGVElectricWireElementBlock : IGVElectricElementBlock {
        int GetConnectedWireFacesMask(int value, int face);
    }
}