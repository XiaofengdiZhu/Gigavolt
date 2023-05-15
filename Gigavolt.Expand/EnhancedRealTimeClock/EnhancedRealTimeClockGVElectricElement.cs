using System;
using System.Linq;
using Engine;

namespace Game
{
    public class EnhancedRealTimeClockGVElectricElement : RotateableGVElectricElement
    {

        public uint m_input = 0u;
        public uint[] m_outputs = new uint[] { 0u, 0u, 0u, 0u };
        int circuitAdd = 1;

        public EnhancedRealTimeClockGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace)
            : base(subsystemGVElectricity, cellFace)
        {
        }

        public override uint GetOutputVoltage(int face)
        {
            GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, Rotation, face);
            if (connectorDirection.HasValue)
            {
                if (connectorDirection.Value == GVElectricConnectorDirection.Top)
                {
                    return m_outputs[0];
                }

                if (connectorDirection.Value == GVElectricConnectorDirection.Right)
                {
                    return m_outputs[1];
                }

                if (connectorDirection.Value == GVElectricConnectorDirection.Bottom)
                {
                    return m_outputs[2];
                }

                if (connectorDirection.Value == GVElectricConnectorDirection.Left)
                {
                    return m_outputs[3];
                }
            }

            return 0u;
        }

        public override bool Simulate()
        {
            bool noInput = true;
            uint input = m_input;
            uint[] outputs = (uint[])m_outputs.Clone();
            int rotation = Rotation;
            foreach (GVElectricConnection connection in Connections)
            {
                if (connection.ConnectorType != GVElectricConnectorType.Output && connection.NeighborConnectorType != GVElectricConnectorType.Input)
                {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
                    if (connectorDirection == GVElectricConnectorDirection.In)
                    {
                        noInput = false;
                        m_input = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                    }
                }
            }

            DateTime now = DateTime.Now;
            if (noInput)
            {
                m_input = 0u;
            }
            if (m_input != input)
            {
                switch (m_input)
                {
                    case 1:
                        circuitAdd = (int)MathUtils.Ceiling((DateTime.Today.AddDays(1) - now).TotalSeconds * 100);
                        break;
                    default:
                        circuitAdd = 1;
                        break;
                }
            }

            SubsystemGVElectricity.QueueGVElectricElementForSimulation(this, MathUtils.Max(SubsystemGVElectricity.FrameStartCircuitStep + circuitAdd, SubsystemGVElectricity.CircuitStep + 1));

            switch (m_input)
            {
                case 0:
                    m_outputs[0] = (uint)now.Hour;
                    m_outputs[1] = (uint)now.Minute;
                    m_outputs[2] = (uint)now.Second;
                    m_outputs[3] = (uint)now.Millisecond;
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
                default:
                    m_outputs[0] = 0u;
                    m_outputs[1] = 0u;
                    m_outputs[2] = 0u;
                    m_outputs[3] = 0u;
                    break;
            }

            return m_outputs.SequenceEqual(outputs);
        }
    }
}
