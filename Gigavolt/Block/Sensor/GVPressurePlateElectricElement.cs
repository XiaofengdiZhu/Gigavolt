using System;
using Engine;

namespace Game {
    public class PressurePlateGVElectricElement : MountedGVElectricElement {
        public uint m_voltage;

        public int m_lastPressFrameIndex;

        public float m_pressure;

        public PressurePlateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace) : base(subsystemGVElectricity, cellFace) { }

        public void Press(float pressure) {
            m_lastPressFrameIndex = Time.FrameIndex;
            if (pressure > m_pressure) {
                m_pressure = pressure;
                GVCellFace cellFace = CellFaces[0];
                SubsystemGVElectricity.SubsystemAudio.PlaySound(
                    "Audio/BlockPlaced",
                    1f,
                    0.3f,
                    new Vector3(cellFace.X, cellFace.Y, cellFace.Z),
                    2.5f,
                    true
                );
                SubsystemGVElectricity.QueueGVElectricElementForSimulation(this, SubsystemGVElectricity.CircuitStep + 1);
            }
        }

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override bool Simulate() {
            uint voltage = m_voltage;
            if (m_pressure > 0f
                && Time.FrameIndex - m_lastPressFrameIndex < 2) {
                m_voltage = PressureToVoltage(m_pressure);
                SubsystemGVElectricity.QueueGVElectricElementForSimulation(this, SubsystemGVElectricity.CircuitStep + 10);
            }
            else {
                if (m_voltage > 0) {
                    GVCellFace cellFace = CellFaces[0];
                    SubsystemGVElectricity.SubsystemAudio.PlaySound(
                        "Audio/BlockPlaced",
                        0.6f,
                        -0.1f,
                        new Vector3(cellFace.X, cellFace.Y, cellFace.Z),
                        2.5f,
                        true
                    );
                }
                m_voltage = 0u;
                m_pressure = 0f;
            }
            return m_voltage != voltage;
        }

        public override void OnCollide(CellFace cellFace, float velocity, ComponentBody componentBody) {
            Press(componentBody.Mass);
            componentBody.ApplyImpulse(new Vector3(0f, -2E-05f, 0f));
        }

        public override void OnHitByProjectile(CellFace cellFace, WorldItem worldItem) {
            int num = Terrain.ExtractContents(worldItem.Value);
            Block block = BlocksManager.Blocks[num];
            Press(1f * block.GetDensity(worldItem.Value));
        }

        public static uint PressureToVoltage(float pressure) => Convert.ToUInt32(pressure);
    }
}