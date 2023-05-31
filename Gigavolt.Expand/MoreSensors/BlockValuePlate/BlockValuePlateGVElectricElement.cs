using Engine;

namespace Game {
    public class BlockValuePlateGVElectricElement : MountedGVElectricElement {
        public uint m_voltage;

        public int m_lastPressFrameIndex;

        public int m_value;

        public BlockValuePlateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace) : base(subsystemGVElectricity, cellFace) { }

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override bool Simulate() {
            uint voltage = m_voltage;
            if (m_value > 0f
                && Time.FrameIndex - m_lastPressFrameIndex < 2) {
                m_voltage = (uint)m_value;
                SubsystemGVElectricity.QueueGVElectricElementForSimulation(this, SubsystemGVElectricity.CircuitStep + 10);
            }
            else {
                m_voltage = 0u;
                m_value = 0;
            }
            return m_voltage != voltage;
        }

        public override void OnHitByProjectile(CellFace cellFace, WorldItem worldItem) {
            m_lastPressFrameIndex = Time.FrameIndex;
            if (worldItem.Value != m_value) {
                m_value = worldItem.Value;
                CellFace cellFace1 = CellFaces[0];
                SubsystemGVElectricity.SubsystemAudio.PlaySound(
                    "Audio/BlockPlaced",
                    1f,
                    0.3f,
                    new Vector3(cellFace1.X, cellFace1.Y, cellFace1.Z),
                    2.5f,
                    true
                );
                SubsystemGVElectricity.QueueGVElectricElementForSimulation(this, SubsystemGVElectricity.CircuitStep + 1);
            }
        }
    }
}