namespace Game {
    public class AnalogToDigitalConverterGVCElectricElement : RotateableGVElectricElement {
        public uint m_bits;

        public AnalogToDigitalConverterGVCElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) { }

        public override uint GetOutputVoltage(int face) {
            GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, Rotation, face);
            if (connectorDirection.HasValue) {
                if (connectorDirection.Value == GVElectricConnectorDirection.Top) {
                    return (m_bits & 1) != 0 ? uint.MaxValue : 0u;
                }
                if (connectorDirection.Value == GVElectricConnectorDirection.Right) {
                    return (m_bits & 2) != 0 ? uint.MaxValue : 0u;
                }
                if (connectorDirection.Value == GVElectricConnectorDirection.Bottom) {
                    return (m_bits & 4) != 0 ? uint.MaxValue : 0u;
                }
                if (connectorDirection.Value == GVElectricConnectorDirection.Left) {
                    return (m_bits & 8) != 0 ? uint.MaxValue : 0u;
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