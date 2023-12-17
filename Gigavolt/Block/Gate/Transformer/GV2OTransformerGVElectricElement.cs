namespace Game {
    public class GV2OTransformerGVElectricElement : RotateableGVElectricElement {
        public uint m_voltage;
        readonly SubsystemElectricity subsystemElectricity;

        public GV2OTransformerGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace) : base(subsystemGVElectricity, cellFace) => subsystemElectricity = SubsystemGVElectricity.Project.FindSubsystem<SubsystemElectricity>(true);

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override bool Simulate() {
            uint voltage = m_voltage;
            uint input = 0u;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    input = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                    break;
                }
            }
            m_voltage = input & 0xFu;
            if (m_voltage != voltage) {
                GVCellFace cellFace = CellFaces[0];
                ElectricElement electricElement = subsystemElectricity.GetElectricElement(cellFace.X, cellFace.Y, cellFace.Z, cellFace.Face);
                if (electricElement != null) {
                    subsystemElectricity.QueueElectricElementForSimulation(electricElement, subsystemElectricity.CircuitStep + 1);
                }
            }
            return false;
        }
    }
}