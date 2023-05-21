using Engine;

namespace Game {
    public class TargetGVCElectricElement : MountedGVElectricElement {
        public uint m_voltage;

        public uint m_score;

        public TargetGVCElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace) : base(subsystemGVElectricity, cellFace) { }

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override bool Simulate() {
            uint voltage = m_voltage;
            if (m_score > 0u) {
                m_voltage = m_score + 7;
                m_score = 0u;
                SubsystemGVElectricity.QueueGVElectricElementForSimulation(this, SubsystemGVElectricity.CircuitStep + 50);
            }
            else {
                m_voltage = 0u;
            }
            return m_voltage != voltage;
        }

        public override void OnHitByProjectile(CellFace cellFace, WorldItem worldItem) {
            if (m_score == 0
                && !IsSignalHigh(m_voltage)) {
                if (cellFace.Face == 0
                    || cellFace.Face == 2) {
                    float num = worldItem.Position.X - cellFace.X - 0.5f;
                    float num2 = worldItem.Position.Y - cellFace.Y - 0.5f;
                    float num3 = MathUtils.Sqrt(num * num + num2 * num2);
                    m_score = MathUint.Clamp((uint)MathUtils.Round(8f * (1f - num3 / 0.707f)), 1, 8);
                }
                else {
                    float num4 = worldItem.Position.Z - cellFace.Z - 0.5f;
                    float num5 = worldItem.Position.Y - cellFace.Y - 0.5f;
                    float num6 = MathUtils.Sqrt(num4 * num4 + num5 * num5);
                    m_score = MathUint.Clamp((uint)MathUtils.Round(8f * (1f - num6 / 0.5f)), 1, 8);
                }
                SubsystemGVElectricity.QueueGVElectricElementForSimulation(this, SubsystemGVElectricity.CircuitStep + 1);
            }
        }
    }
}