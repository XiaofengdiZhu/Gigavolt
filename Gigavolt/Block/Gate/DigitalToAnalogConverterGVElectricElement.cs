namespace Game {
    public class DigitalToAnalogConverterGVElectricElement : RotateableGVElectricElement {
        public readonly int m_type;
        public readonly bool m_classic;
        public uint m_voltage;
        public readonly uint maxInput;

        public DigitalToAnalogConverterGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, int value, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) {
            int data = Terrain.ExtractData(value);
            m_type = GVDigitalToAnalogConverterBlock.GetType(data);
            m_classic = GVDigitalToAnalogConverterBlock.GetClassic(data);
            maxInput = m_type switch {
                1 => 3u,
                2 => 15u,
                3 => 255u,
                _ => 1u
            };
        }

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override bool Simulate() {
            uint voltage = m_voltage;
            m_voltage = 0u;
            int rotation = Rotation;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    uint inputVoltage = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                    bool isSignalHigh = IsSignalHigh(inputVoltage);
                    inputVoltage &= maxInput;
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue) {
                        switch (connectorDirection.Value) {
                            case GVElectricConnectorDirection.Top:
                                if (m_classic) {
                                    if (isSignalHigh) {
                                        m_voltage += 1u;
                                    }
                                }
                                else {
                                    m_voltage |= inputVoltage;
                                }
                                break;
                            case GVElectricConnectorDirection.Right:
                                if (m_classic) {
                                    if (isSignalHigh) {
                                        m_voltage += 2u;
                                    }
                                }
                                else {
                                    inputVoltage <<= m_type switch {
                                        1 => 2,
                                        2 => 4,
                                        3 => 8,
                                        _ => 1
                                    };
                                    m_voltage |= inputVoltage;
                                }
                                break;
                            case GVElectricConnectorDirection.Bottom:
                                if (m_classic) {
                                    if (isSignalHigh) {
                                        m_voltage += 4u;
                                    }
                                }
                                else {
                                    inputVoltage <<= m_type switch {
                                        1 => 4,
                                        2 => 8,
                                        3 => 16,
                                        _ => 2
                                    };
                                    m_voltage |= inputVoltage;
                                }
                                break;
                            case GVElectricConnectorDirection.Left:
                                if (m_classic) {
                                    if (isSignalHigh) {
                                        m_voltage += 8u;
                                    }
                                }
                                else {
                                    inputVoltage <<= m_type switch {
                                        1 => 6,
                                        2 => 12,
                                        3 => 24,
                                        _ => 3
                                    };
                                    m_voltage |= inputVoltage;
                                }
                                break;
                        }
                    }
                }
            }
            return m_voltage != voltage;
        }
    }
}