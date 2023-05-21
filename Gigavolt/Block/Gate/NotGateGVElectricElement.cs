namespace Game {
    public class NotGateGVElectricElement : RotateableGVElectricElement {
        public uint m_voltage;

        public NotGateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace) : base(subsystemGVElectricity, cellFace) { }

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override bool Simulate() {
            uint voltage = m_voltage;
            uint num = 0;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    num = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                    break;
                }
            }
            m_voltage = ~num;
            return m_voltage != voltage;
        }
    }
}