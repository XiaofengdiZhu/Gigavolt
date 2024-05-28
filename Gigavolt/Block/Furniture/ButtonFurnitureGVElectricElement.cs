using Engine;

namespace Game {
    public class ButtonFurnitureGVElectricElement : FurnitureGVElectricElement {
        public uint m_voltage;

        public bool m_wasPressed;

        public ButtonFurnitureGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, Point3 point) : base(subsystemGVElectricity, point) { }

        public void Press() {
            if (!m_wasPressed
                && !IsSignalHigh(m_voltage)) {
                m_wasPressed = true;
                GVCellFace cellFace = CellFaces[0];
                SubsystemGVElectricity.SubsystemAudio.PlaySound(
                    "Audio/Click",
                    1f,
                    0f,
                    new Vector3(cellFace.X, cellFace.Y, cellFace.Z),
                    2f,
                    true
                );
                SubsystemGVElectricity.QueueGVElectricElementForSimulation(this, SubsystemGVElectricity.CircuitStep + 1);
            }
        }

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override bool Simulate() {
            uint voltage = m_voltage;
            if (m_wasPressed) {
                m_wasPressed = false;
                m_voltage = uint.MaxValue;
                SubsystemGVElectricity.QueueGVElectricElementForSimulation(this, SubsystemGVElectricity.CircuitStep + 10);
            }
            else {
                m_voltage = 0u;
            }
            return m_voltage != voltage;
        }

        public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner) {
            Press();
            return true;
        }

        public override void OnHitByProjectile(CellFace cellFace, WorldItem worldItem) {
            Press();
        }
    }
}