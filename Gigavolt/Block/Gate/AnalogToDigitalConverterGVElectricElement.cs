namespace Game {
    public class AnalogToDigitalConverterGVElectricElement : RotateableGVElectricElement {
        public uint m_bits;
        public readonly int m_type;
        public readonly bool m_classic;
        public readonly uint maxOutput;

        public AnalogToDigitalConverterGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, int value, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) {
            int data = Terrain.ExtractData(value);
            m_type = GVAnalogToDigitalConverterBlock.GetType(data);
            m_classic = GVAnalogToDigitalConverterBlock.GetClassic(data);
            maxOutput = m_type switch {
                1 => 3u,
                2 => 15u,
                3 => 255u,
                _ => 1u
            };
        }

        public override uint GetOutputVoltage(int face) {
            GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, Rotation, face);
            if (connectorDirection.HasValue) {
                switch (connectorDirection.Value) {
                    case GVElectricConnectorDirection.Top: return m_classic ? (m_bits & 1) != 0 ? uint.MaxValue : 0u : m_bits & maxOutput;
                    case GVElectricConnectorDirection.Right:
                        return m_classic ? (m_bits & 2) != 0 ? uint.MaxValue : 0u : m_type switch {
                            1 => (m_bits >> 2) & maxOutput,
                            2 => (m_bits >> 4) & maxOutput,
                            3 => (m_bits >> 8) & maxOutput,
                            _ => (m_bits >> 1) & maxOutput
                        };
                    case GVElectricConnectorDirection.Bottom:
                        return m_classic ? (m_bits & 4) != 0 ? uint.MaxValue : 0u : m_type switch {
                            1 => (m_bits >> 4) & maxOutput,
                            2 => (m_bits >> 8) & maxOutput,
                            3 => (m_bits >> 16) & maxOutput,
                            _ => (m_bits >> 2) & maxOutput
                        };
                    case GVElectricConnectorDirection.Left:
                        return m_classic ? (m_bits & 8) != 0 ? uint.MaxValue : 0u : m_type switch {
                            1 => (m_bits >> 6) & maxOutput,
                            2 => (m_bits >> 12) & maxOutput,
                            3 => (m_bits >> 24) & maxOutput,
                            _ => (m_bits >> 3) & maxOutput
                        };
                }
            }
            return 0u;
        }

        public override bool Simulate() {
            uint bits = m_bits;
            int rotation = Rotation;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue
                        && connectorDirection.Value == GVElectricConnectorDirection.In) {
                        m_bits = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                    }
                }
            }
            return m_bits != bits;
        }
    }
}