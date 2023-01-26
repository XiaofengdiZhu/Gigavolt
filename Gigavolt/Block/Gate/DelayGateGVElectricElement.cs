namespace Game
{
    public class DelayGateGVElectricElement : BaseDelayGateGVElectricElement
    {
        public int? m_delaySteps;

        public int m_lastDelayCalculationStep;

        public static int[] m_delaysByPredecessorsCount = new int[3]
        {
            20,
            80,
            400
        };

        public override int DelaySteps
        {
            get
            {
                if (SubsystemGVElectricity.CircuitStep - m_lastDelayCalculationStep > 50)
                {
                    m_delaySteps = null;
                }
                if (!m_delaySteps.HasValue)
                {
                    int count = 0;
                    CountDelayPredecessors(this, ref count);
                    m_delaySteps = m_delaysByPredecessorsCount[count];
                    m_lastDelayCalculationStep = SubsystemGVElectricity.CircuitStep;
                }
                return m_delaySteps.Value;
            }
        }

        public DelayGateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace)
            : base(subsystemGVElectricity, cellFace)
        {
        }

        public static void CountDelayPredecessors(DelayGateGVElectricElement delayGate, ref int count)
        {
            if (count < 2)
            {
                foreach (GVElectricConnection connection in delayGate.Connections)
                {
                    if (connection.ConnectorType == GVElectricConnectorType.Input)
                    {
                        var delayGateGVElectricElement = connection.NeighborGVElectricElement as DelayGateGVElectricElement;
                        if (delayGateGVElectricElement != null)
                        {
                            count++;
                            CountDelayPredecessors(delayGateGVElectricElement, ref count);
                            break;
                        }
                    }
                }
            }
        }
    }
}
