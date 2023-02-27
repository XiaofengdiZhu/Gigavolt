using System;

namespace Game
{
    public class DigitalToAnalogConverterGVElectricElement : RotateableGVElectricElement
    {
        public int m_type;
        public uint m_voltage;
        public uint maxInput;
        public DigitalToAnalogConverterGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace, int value)
            : base(subsystemGVElectricity, cellFace)
        {
            m_type = GVDigitalToAnalogConverterBlock.GetType(Terrain.ExtractData(value));
            switch (m_type)
            {
                case 1: maxInput = 3u; break;
                case 2: maxInput = 15u; break;
                case 3: maxInput = 63u; break;
                default: maxInput = 1u; break;
            }
        }

        public override uint GetOutputVoltage(int face)
        {
            return m_voltage;
        }

        public override bool Simulate()
        {
            uint voltage = m_voltage;
            m_voltage = 0u;
            int rotation = Rotation;
            foreach (GVElectricConnection connection in Connections)
            {
                if (connection.ConnectorType != GVElectricConnectorType.Output && connection.NeighborConnectorType != 0)
                {
                    uint inputVoltage = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                    inputVoltage &= maxInput;
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue)
                    {
                        if (connectorDirection.Value == GVElectricConnectorDirection.Top)
                        {
                            m_voltage |= inputVoltage;
                        }
                        if (connectorDirection.Value == GVElectricConnectorDirection.Right)
                        {
                            switch (m_type)
                            {
                                case 1: inputVoltage <<= 2;break;
                                case 2: inputVoltage <<= 4;break;
                                case 3: inputVoltage <<= 8;break;
                                default:inputVoltage <<= 1;break;
                            }
                            m_voltage |= inputVoltage;
                        }
                        if (connectorDirection.Value == GVElectricConnectorDirection.Bottom)
                        {
                            switch (m_type)
                            {
                                case 1: inputVoltage <<= 4; break;
                                case 2: inputVoltage <<= 8; break;
                                case 3: inputVoltage <<= 16; break;
                                default: inputVoltage <<= 2; break;
                            }
                            m_voltage |= inputVoltage;
                        }
                        if (connectorDirection.Value == GVElectricConnectorDirection.Left)
                        {
                            switch (m_type)
                            {
                                case 1: inputVoltage <<= 6; break;
                                case 2: inputVoltage <<= 12; break;
                                case 3: inputVoltage <<= 24; break;
                                default: inputVoltage <<= 3; break;
                            }
                            m_voltage |= inputVoltage;
                        }
                    }
                }
            }
            return m_voltage != voltage;
        }
    }
}
