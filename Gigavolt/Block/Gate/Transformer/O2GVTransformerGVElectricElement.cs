using System;

namespace Game {
    public class O2GVTransformerGVElectricElement : RotateableGVElectricElement {
        public uint m_voltage;
        readonly SubsystemElectricity subsystemElectricity;

        public O2GVTransformerGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) => subsystemElectricity = SubsystemGVElectricity.Project.FindSubsystem<SubsystemElectricity>(true);

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override bool Simulate() {
            if (SubterrainId != 0) {
                return false;
            }
            uint voltage = m_voltage;
            GVCellFace cellFace = CellFaces[0];
            ElectricElement electricElement = subsystemElectricity.GetElectricElement(cellFace.X, cellFace.Y, cellFace.Z, cellFace.Face);
            if (electricElement != null) {
                m_voltage = (uint)MathF.Round(electricElement.GetOutputVoltage(0) * 15f);
            }
            return m_voltage != voltage;
        }
    }
}