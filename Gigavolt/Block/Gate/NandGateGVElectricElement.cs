namespace Game {
    public class NandGateGVElectricElement : RotateableGVElectricElement {
        public uint m_voltage = uint.MaxValue;

        public NandGateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, uint subterrainId) : base(
            subsystemGVElectricity,
            cellFace,
            subterrainId
        ) { }

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override bool Simulate() {
            uint voltage = m_voltage;
            uint num = 0u;
            uint num2 = uint.MaxValue;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    num2 &= connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                    num++;
                }
            }
            m_voltage = num >= 2u ? ~num2 : uint.MaxValue;
            return m_voltage != voltage;
        }
    }
}