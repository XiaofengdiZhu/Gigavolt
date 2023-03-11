using Engine;

namespace Game
{
    public class AnalogToDigitalConverterGVElectricElement : RotateableGVElectricElement
    {
        public uint m_bits;
        public int m_type;
        public uint maxOutput;

        public AnalogToDigitalConverterGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace, int value)
            : base(subsystemGVElectricity, cellFace)
        {
            m_type = GVAnalogToDigitalConverterBlock.GetType(Terrain.ExtractData(value));
            switch (m_type)
            {
                case 1: maxOutput = 3u; break;
                case 2: maxOutput = 15u; break;
                case 3: maxOutput = 255u; break;
                default: maxOutput = 1u; break;
            }
        }

        public override uint GetOutputVoltage(int face)
        {
            GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, Rotation, face);
            if (connectorDirection.HasValue)
            {
                if (connectorDirection.Value == GVElectricConnectorDirection.Top)
                {
                    return m_bits &= maxOutput;
                }
                if (connectorDirection.Value == GVElectricConnectorDirection.Right)
                {
                    uint output;
                    switch (m_type)
                    {
                        case 1: output = (m_bits >> 2) & maxOutput; break;
                        case 2: output = (m_bits >> 4) & maxOutput; break;
                        case 3: output = (m_bits >> 8) & maxOutput; break;
                        default: output = (m_bits >> 1) & maxOutput; break;
                    }
                    return output;
                }
                if (connectorDirection.Value == GVElectricConnectorDirection.Bottom)
                {
                    uint output;
                    switch (m_type)
                    {
                        case 1: output = (m_bits >> 4) & maxOutput; break;
                        case 2: output = (m_bits >> 8) & maxOutput; break;
                        case 3: output = (m_bits >> 16) & maxOutput; break;
                        default: output = (m_bits >> 2) & maxOutput; break;
                    }
                    return output;
                }
                if (connectorDirection.Value == GVElectricConnectorDirection.Left)
                {
                    uint output;
                    switch (m_type)
                    {
                        case 1: output = (m_bits >> 6) & maxOutput; break;
                        case 2: output = (m_bits >> 12) & maxOutput; break;
                        case 3: output = (m_bits >> 24) & maxOutput; break;
                        default: output = (m_bits >> 3) & maxOutput; break;
                    }
                    return output;
                }
            }
            return 0u;
        }

        public override bool Simulate()
        {
            uint bits = m_bits;
            int rotation = Rotation;
            foreach (GVElectricConnection connection in Connections)
            {
                if (connection.ConnectorType != GVElectricConnectorType.Output && connection.NeighborConnectorType != 0)
                {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue && connectorDirection.Value == GVElectricConnectorDirection.In)
                    {
                        m_bits = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                    }
                }
            }
            return m_bits != bits;
        }
    }
}
