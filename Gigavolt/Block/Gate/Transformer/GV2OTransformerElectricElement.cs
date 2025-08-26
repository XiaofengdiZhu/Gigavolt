namespace Game {
    public class GV2OTransformerElectricElement : RotateableElectricElement {
        public float m_voltage;
        readonly SubsystemGVElectricity subsystemGVElectricity;

        public GV2OTransformerElectricElement(SubsystemElectricity subsystemElectricity, CellFace cellFace) : base(subsystemElectricity, cellFace) =>
            subsystemGVElectricity = SubsystemElectricity.Project.FindSubsystem<SubsystemGVElectricity>(true);

        public override float GetOutputVoltage(int face) => m_voltage;

        public override bool Simulate() {
            float voltage = m_voltage;
            CellFace cellFace = CellFaces[0];
            GVElectricElement GVElectricElement = subsystemGVElectricity.GetGVElectricElement(cellFace.X, cellFace.Y, cellFace.Z, cellFace.Face, 0u);
            if (GVElectricElement != null) {
                m_voltage = GVElectricElement.GetOutputVoltage(0) / 15f;
            }
            return m_voltage != voltage;
        }
    }
}