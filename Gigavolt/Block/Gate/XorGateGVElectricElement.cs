namespace Game {
    public class XorGateGVElectricElement : RotateableGVElectricElement {
        public uint m_voltage;

        public XorGateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace) : base(subsystemGVElectricity, cellFace) { }

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override bool Simulate() {
            uint voltage = m_voltage;
            uint? num = null;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    uint num2 = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                    num = !num.HasValue ? num2 : num ^= num2;
                }
            }
            m_voltage = num.HasValue ? num.Value : 0u;
            return m_voltage != voltage;
        }
    }
}