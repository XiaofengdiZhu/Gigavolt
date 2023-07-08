namespace Game {
    public class DoorGVElectricElement : GVElectricElement {
        public SubsystemGVDoorBlockBehavior m_subsystem;
        public int m_lastChangeCircuitStep;

        public bool m_needsReset;

        public uint m_voltage;

        public DoorGVElectricElement(SubsystemGVElectricity subsystemElectricity, CellFace cellFace) : base(subsystemElectricity, cellFace) {
            m_subsystem = subsystemElectricity.Project.FindSubsystem<SubsystemGVDoorBlockBehavior>(true);
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
                m_subsystem.OpenDoor(cellFace.X, cellFace.Y, cellFace.Z, MathUint.ToInt(m_voltage));
            }
            return false;
        }
    }
}