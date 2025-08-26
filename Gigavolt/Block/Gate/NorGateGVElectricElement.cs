namespace Game {
    public class NorGateGVElectricElement : RotateableGVElectricElement {
        public uint m_voltage = uint.MaxValue;

        public NorGateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, uint subterrainId) : base(
            subsystemGVElectricity,
            cellFace,
            subterrainId
        ) { }

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override bool Simulate() {
            float voltage = m_voltage;
            uint num = 0u;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    num |= connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                }
            }
            m_voltage = ~num;
            return m_voltage != voltage;
        }
    }
}