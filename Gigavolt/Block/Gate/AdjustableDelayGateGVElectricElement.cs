namespace Game
{
    public class AdjustableDelayGateGVElectricElement : BaseDelayGateGVElectricElement
    {
        public int m_delaySteps;

        public override int DelaySteps => m_delaySteps;

        public AdjustableDelayGateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace)
            : base(subsystemGVElectricity, cellFace)
        {
            int data = Terrain.ExtractData(subsystemGVElectricity.SubsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z));
            m_delaySteps = GVAdjustableDelayGateBlock.GetDelay(data);
        }
    }
}
