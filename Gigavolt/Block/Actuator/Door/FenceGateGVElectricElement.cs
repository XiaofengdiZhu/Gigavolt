namespace Game {
    public class FenceGateGVElectricElement : GVElectricElement {
        public SubsystemGVFenceGateBlockBehavior m_subsystem;
        public int m_lastChangeCircuitStep;

        public bool m_needsReset;

        public uint m_voltage;

        public FenceGateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) {
            m_subsystem = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVFenceGateBlockBehavior>(true);
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
                GVCellFace cellFace = CellFaces[0];
                m_subsystem.OpenGate(
                    cellFace.X,
                    cellFace.Y,
                    cellFace.Z,
                    SubterrainId,
                    MathUint.ToIntWithClamp(m_voltage)
                );
            }
            return false;
        }
    }
}