using System.Collections.Generic;

namespace Game
{
    public abstract class BaseDelayGateGVElectricElement : RotateableGVElectricElement
    {
        public uint m_voltage;

        public uint m_lastStoredVoltage;

        public Dictionary<int, uint> m_voltagesHistory = new Dictionary<int, uint>();

        public abstract int DelaySteps
        {
            get;
        }

        public BaseDelayGateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace)
            : base(subsystemGVElectricity, cellFace)
        {
        }

        public override uint GetOutputVoltage(int face)
        {
            return m_voltage;
        }

        public override bool Simulate()
        {
            uint voltage = m_voltage;
            int delaySteps = DelaySteps;
            uint num = 0;
            foreach (GVElectricConnection connection in Connections)
            {
                if (connection.ConnectorType != GVElectricConnectorType.Output && connection.NeighborConnectorType != 0)
                {
                    num = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                    break;
                }
            }
            if (delaySteps > 0)
            {
                if (m_voltagesHistory.TryGetValue(SubsystemGVElectricity.CircuitStep, out uint value))
                {
                    m_voltage = value;
                    m_voltagesHistory.Remove(SubsystemGVElectricity.CircuitStep);
                }
                if (num != m_lastStoredVoltage)
                {
                    m_lastStoredVoltage = num;
                    if (m_voltagesHistory.Count < 300)
                    {
                        m_voltagesHistory[SubsystemGVElectricity.CircuitStep + DelaySteps] = num;
                        SubsystemGVElectricity.QueueGVElectricElementForSimulation(this, SubsystemGVElectricity.CircuitStep + DelaySteps);
                    }
                }
            }
            else
            {
                m_voltage = num;
            }
            return m_voltage != voltage;
        }
    }
}
