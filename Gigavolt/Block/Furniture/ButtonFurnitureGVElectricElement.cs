using Engine;

namespace Game {
    public class ButtonFurnitureGVElectricElement : FurnitureGVElectricElement {
        public uint m_voltage;
        public ButtonFurnitureElectricElement m_originalElement;

        public ButtonFurnitureGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, Point3 point, uint subterrainId) : base(subsystemGVElectricity, point, subterrainId) { }

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override bool Simulate() {
            uint voltage = m_voltage;
            m_voltage = (m_originalElement?.m_voltage ?? 0u) > 0u ? uint.MaxValue : 0u;
            SubsystemGVElectricity.QueueGVElectricElementForSimulation(this, SubsystemGVElectricity.CircuitStep + 1);
            return m_voltage != voltage;
        }

        public override void OnAdded() {
            GVCellFace cellFace = CellFaces[0];
            if (SubsystemGVElectricity.Project.FindSubsystem<SubsystemElectricity>(true).GetElectricElement(cellFace.X, cellFace.Y, cellFace.Z, cellFace.Face) is ButtonFurnitureElectricElement element) {
                m_originalElement = element;
            }
        }

        public override void OnRemoved() {
            m_originalElement = null;
        }
    }
}