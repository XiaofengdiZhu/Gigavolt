namespace Game {
    public class AdjustableDelayGateGVElectricElement : BaseDelayGateGVElectricElement {
        public readonly int m_delaySteps;

        public override int DelaySteps => m_delaySteps;

        public AdjustableDelayGateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, int value, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) => m_delaySteps = GVAdjustableDelayGateBlock.GetDelay(Terrain.ExtractData(value));
    }
}