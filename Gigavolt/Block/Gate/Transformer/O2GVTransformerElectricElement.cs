namespace Game {
    public class O2GVTransformerElectricElement : RotateableElectricElement {
        public float m_voltage;
        readonly SubsystemGVElectricity subsystemGVElectricity;

        public O2GVTransformerElectricElement(SubsystemElectricity subsystemElectricity, CellFace cellFace) : base(subsystemElectricity, cellFace) =>
            subsystemGVElectricity = SubsystemElectricity.Project.FindSubsystem<SubsystemGVElectricity>(true);

        public override float GetOutputVoltage(int face) => m_voltage;

        public override bool Simulate() {
            float voltage = m_voltage;
            float input = 0u;
            foreach (ElectricConnection connection in Connections) {
                if (connection.ConnectorType != ElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    input = connection.NeighborElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                    break;
                }
            }
            m_voltage = input;
            if (m_voltage != voltage) {
                CellFace cellFace = CellFaces[0];
                GVElectricElement electricElement = subsystemGVElectricity.GetGVElectricElement(
                    cellFace.X,
                    cellFace.Y,
                    cellFace.Z,
                    cellFace.Face,
                    0u
                );
                if (electricElement != null) {
                    subsystemGVElectricity.QueueGVElectricElementForSimulation(electricElement, subsystemGVElectricity.CircuitStep + 1);
                }
            }
            return false;
        }
    }
}