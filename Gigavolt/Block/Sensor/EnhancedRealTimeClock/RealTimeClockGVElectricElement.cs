using System;
using System.Linq;
using Engine;

namespace Game {
    public class RealTimeClockGVElectricElement : RotateableGVElectricElement {
        public uint m_input;
        public readonly uint[] m_outputs = [0u, 0u, 0u, 0u];
        public int circuitAdd = 1;
        public uint m_lastClockValue;
        public readonly SubsystemWeather m_subsystemWeather;
        public readonly SubsystemGameInfo m_subsystemGameInfo;
        public readonly SubsystemTimeOfDay m_subsystemTimeOfDay;
        public readonly bool m_classic;

        public RealTimeClockGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, uint subterrainId, bool classic) : base(subsystemGVElectricity, cellFace, subterrainId) {
            m_subsystemWeather = subsystemGVElectricity.Project.FindSubsystem<SubsystemWeather>(true);
            m_subsystemGameInfo = subsystemGVElectricity.Project.FindSubsystem<SubsystemGameInfo>(true);
            m_subsystemTimeOfDay = subsystemGVElectricity.Project.FindSubsystem<SubsystemTimeOfDay>(true);
            m_classic = classic;
        }

        public override uint GetOutputVoltage(int face) {
            GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, Rotation, face);
            if (connectorDirection.HasValue) {
                if (connectorDirection.Value == GVElectricConnectorDirection.Top) {
                    return m_classic ? GetClockValue() & 0xFu : m_outputs[0];
                }
                if (connectorDirection.Value == GVElectricConnectorDirection.Right) {
                    return m_classic ? (GetClockValue() >> 4) & 0xFu : m_outputs[1];
                }
                if (connectorDirection.Value == GVElectricConnectorDirection.Bottom) {
                    return m_classic ? (GetClockValue() >> 8) & 0xFu : m_outputs[2];
                }
                if (connectorDirection.Value == GVElectricConnectorDirection.Left) {
                    return m_classic ? (GetClockValue() >> 12) & 0xFu : m_outputs[3];
                }
                if (connectorDirection.Value == GVElectricConnectorDirection.In) {
                    return m_classic ? (GetClockValue() >> 16) & 0xFu : 0u;
                }
            }
            return 0u;
        }

        public override bool Simulate() {
            if (m_classic) {
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
            bool noInput = true;
            uint input = m_input;
            uint[] outputs = (uint[])m_outputs.Clone();
            int rotation = Rotation;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != GVElectricConnectorType.Input) {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
                    if (connectorDirection == GVElectricConnectorDirection.In) {
                        noInput = false;
                        m_input = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                    }
                }
            }
            DateTime now = DateTime.Now;
            if (noInput) {
                m_input = 0u;
            }
            if (m_input != input) {
                switch (m_input) {
                    case 1:
                        circuitAdd = (int)Math.Ceiling((DateTime.Today.AddDays(1) - now).TotalSeconds / SubsystemGVElectricity.CircuitStepDuration);
                        break;
                    case 3:
                        circuitAdd = (int)MathF.Ceiling(0.25f / SubsystemGVElectricity.CircuitStepDuration);
                        break;
                    default:
                        circuitAdd = 1;
                        break;
                }
            }
            SubsystemGVElectricity.QueueGVElectricElementForSimulation(this, SubsystemGVElectricity.CircuitStep + MathUtils.Max(circuitAdd, 1));
            switch (m_input) {
                case 0:
                    m_outputs[0] = (uint)now.Millisecond;
                    m_outputs[1] = (uint)now.Second;
                    m_outputs[2] = (uint)now.Minute;
                    m_outputs[3] = (uint)now.Hour;
                    break;
                case 1:
                    m_outputs[0] = (uint)now.DayOfWeek;
                    m_outputs[1] = (uint)now.Day;
                    m_outputs[2] = (uint)now.Month;
                    m_outputs[3] = (uint)now.Year;
                    break;
                case 2:
                    m_outputs[0] = (uint)(now.Ticks & uint.MaxValue);
                    m_outputs[1] = (uint)(now.Ticks >> 32);
                    m_outputs[2] = 0u;
                    m_outputs[3] = 0u;
                    return true;
                case 3:
                    double precipitationStartTimeLeft = Math.Ceiling(m_subsystemWeather.m_precipitationStartTime - m_subsystemGameInfo.TotalElapsedGameTime);
                    double precipitationEndTimeLeft = Math.Ceiling(m_subsystemWeather.m_precipitationEndTime - m_subsystemGameInfo.TotalElapsedGameTime);
                    m_outputs[0] = (uint)Math.Abs(precipitationStartTimeLeft);
                    m_outputs[1] = precipitationStartTimeLeft < 0 ? uint.MaxValue : 0u;
                    m_outputs[2] = (uint)Math.Abs(precipitationEndTimeLeft);
                    m_outputs[3] = precipitationEndTimeLeft < 0 ? uint.MaxValue : 0u;
                    break;
                default:
                    m_outputs[0] = 0u;
                    m_outputs[1] = 0u;
                    m_outputs[2] = 0u;
                    m_outputs[3] = 0u;
                    break;
            }
            return !m_outputs.SequenceEqual(outputs);
        }

        public uint GetClockValue() => (uint)(m_subsystemTimeOfDay.Day * 4096);
    }
}