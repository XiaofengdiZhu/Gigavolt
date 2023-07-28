namespace Game {
    public class FenceGateGVElectricElement : GVElectricElement {
        public SubsystemGVFenceGateBlockBehavior m_subsystem;
        public int m_lastChangeCircuitStep;

        public bool m_needsReset;

        public uint m_voltage;

        public FenceGateGVElectricElement(SubsystemGVElectricity subsystemElectricity, CellFace cellFace) : base(subsystemElectricity, cellFace) {
            m_subsystem = subsystemElectricity.Project.FindSubsystem<SubsystemGVFenceGateBlockBehavior>(true);
            m_lastChangeCircuitStep = SubsystemGVElectricity.CircuitStep;
            m_needsReset = true;
        }

        public override bool Simulate() {
            uint voltage = m_voltage;
            m_voltage = 0;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    m_voltage |= connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                }
            }
            if (m_voltage != voltage) {
                CellFace cellFace = CellFaces[0];
                m_subsystem.OpenGate(cellFace.X, cellFace.Y, cellFace.Z, MathUint.ToInt(m_voltage));
            }
            return false;
        }
    }
}