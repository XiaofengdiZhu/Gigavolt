namespace Game {
    public class DigitalToAnalogConverterGVCElectricElement : RotateableGVElectricElement {
        public uint m_voltage;

        public DigitalToAnalogConverterGVCElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) { }

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override bool Simulate() {
            uint voltage = m_voltage;
            m_voltage = 0u;
            int rotation = Rotation;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0
                    && IsSignalHigh(connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace))) {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue) {
                        if (connectorDirection.Value == GVElectricConnectorDirection.Top) {
                            m_voltage += 1u;
                        }
                        if (connectorDirection.Value == GVElectricConnectorDirection.Right) {
                            m_voltage += 2u;
                        }
                        if (connectorDirection.Value == GVElectricConnectorDirection.Bottom) {
                            m_voltage += 4u;
                        }
                        if (connectorDirection.Value == GVElectricConnectorDirection.Left) {
                            m_voltage += 8u;
                        }
                    }
                }
            }
            return m_voltage != voltage;
        }
    }
}