using System;

namespace Game {
    public class RealTimeClockGVCElectricElement : RotateableGVElectricElement {
        public SubsystemTimeOfDay m_subsystemTimeOfDay;

        public uint m_lastClockValue;

        public RealTimeClockGVCElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace) : base(subsystemGVElectricity, cellFace) => m_subsystemTimeOfDay = SubsystemGVElectricity.Project.FindSubsystem<SubsystemTimeOfDay>(true);

        public override uint GetOutputVoltage(int face) {
            GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, Rotation, face);
            if (connectorDirection.HasValue) {
                if (connectorDirection.Value == GVElectricConnectorDirection.Top) {
                    return GetClockValue() & 0xFu;
                }
                if (connectorDirection.Value == GVElectricConnectorDirection.Right) {
                    return (GetClockValue() >> 4) & 0xFu;
                }
                if (connectorDirection.Value == GVElectricConnectorDirection.Bottom) {
                    return (GetClockValue() >> 8) & 0xFu;
                }
                if (connectorDirection.Value == GVElectricConnectorDirection.Left) {
                    return (GetClockValue() >> 12) & 0xFu;
                }
                if (connectorDirection.Value == GVElectricConnectorDirection.In) {
                    return (GetClockValue() >> 16) & 0xFu;
                }
            }
            return 0u;
        }

        public override bool Simulate() {
            double day = m_subsystemTimeOfDay.Day;
            int num = (int)(((Math.Ceiling(day * 4096.0) + 0.5) / 4096.0 - day) * 1200.0 / 0.0099999997764825821);
            int circuitStep = Math.Max(SubsystemGVElectricity.FrameStartCircuitStep + num, SubsystemGVElectricity.CircuitStep + 1);
            SubsystemGVElectricity.QueueGVElectricElementForSimulation(this, circuitStep);
            uint clockValue = GetClockValue();
            if (clockValue != m_lastClockValue) {
                m_lastClockValue = clockValue;
                return true;
            }
            return false;
        }

        public uint GetClockValue() => (uint)(m_subsystemTimeOfDay.Day * 4096);
    }
}