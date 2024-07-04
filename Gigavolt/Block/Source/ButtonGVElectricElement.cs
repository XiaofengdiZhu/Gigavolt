using Engine;

namespace Game {
    public class ButtonGVElectricElement : MountedGVElectricElement {
        public readonly SubsystemGVButtonBlockBehavior m_subsystemGVButtonBlockBehavior;

        public readonly GVButtonData m_blockData;
        public uint m_voltage;
        public bool m_wasPressed;

        public ButtonGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, GVCellFace cellFace, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) {
            m_subsystemGVButtonBlockBehavior = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVButtonBlockBehavior>(true);
            m_blockData = m_subsystemGVButtonBlockBehavior.GetItemData(m_subsystemGVButtonBlockBehavior.GetIdFromValue(value));
        }

        public void Press() {
            if (!m_wasPressed
                && m_voltage == 0u) {
                m_wasPressed = true;
                GVCellFace cellFace = CellFaces[0];
                Vector3 position = new(cellFace.X + 0.5f, cellFace.Y + 0.5f, cellFace.Z + 0.5f);
                SubsystemGVElectricity.SubsystemAudio.PlaySound(
                    "Audio/Click",
                    1f,
                    0f,
                    SubterrainId == 0 ? position : Vector3.Transform(position, GVStaticStorage.GVSubterrainSystemDictionary[SubterrainId].GlobalTransform),
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
                m_voltage = m_blockData?.GigaVoltageLevel ?? uint.MaxValue;
                SubsystemGVElectricity.QueueGVElectricElementForSimulation(this, SubsystemGVElectricity.CircuitStep + (m_blockData?.Duration ?? 10));
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