using System;

namespace Game {
    public class TargetGVElectricElement : MountedGVElectricElement {
        public uint m_voltage;

        public uint m_score;

        public TargetGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) { }

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override bool Simulate() {
            uint voltage = m_voltage;
            if (m_score > 0u) {
                m_voltage = m_score;
                m_score = 0u;
                SubsystemGVElectricity.QueueGVElectricElementForSimulation(this, SubsystemGVElectricity.CircuitStep + 50);
            }
            else {
                m_voltage = 0u;
            }
            return m_voltage != voltage;
        }

        public override void OnHitByProjectile(CellFace cellFace, WorldItem worldItem) {
            if (m_score == 0u
                && m_voltage == 0u) {
                float distance;
                if (cellFace.Face is 0 or 2) {
                    float num = worldItem.Position.X - cellFace.X - 0.5f;
                    float num2 = worldItem.Position.Y - cellFace.Y - 0.5f;
                    distance = MathF.Sqrt(num * num + num2 * num2);
                }
                else {
                    float num4 = worldItem.Position.Z - cellFace.Z - 0.5f;
                    float num5 = worldItem.Position.Y - cellFace.Y - 0.5f;
                    distance = MathF.Sqrt(num4 * num4 + num5 * num5);
                }
                if (distance <= 0.5f) {
                    m_score = (uint)((1f - distance * 2f) * uint.MaxValue);
                }
                SubsystemGVElectricity.QueueGVElectricElementForSimulation(this, SubsystemGVElectricity.CircuitStep + 1);
            }
        }
    }
}