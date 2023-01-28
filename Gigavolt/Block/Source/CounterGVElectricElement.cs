using Engine;

namespace Game
{
    public class CounterGVElectricElement : RotateableGVElectricElement
    {
        public SubsystemGVCounterBlockBehavior m_subsystemGVCounterBlockBehavior;
        public bool m_plusAllowed = true;

        public bool m_minusAllowed = true;

        public bool m_resetAllowed = true;

        public uint m_counter;

        public bool m_overflow;

        public CounterGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace)
            : base(subsystemGVElectricity, cellFace)
        {
            m_subsystemGVCounterBlockBehavior = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVCounterBlockBehavior>(throwOnError: true);
            GigaVoltageLevelData blockData = m_subsystemGVCounterBlockBehavior.GetBlockData(cellFace.Point);
            uint overflowVoltage = blockData == null ? 0u : blockData.Data;
            uint? num = subsystemGVElectricity.ReadPersistentVoltage(cellFace.Point);
            if (num.HasValue)
            {
                if (num.Value == overflowVoltage - 0x12345678)
                {
                    m_overflow = true;
                    m_counter = 0u;
                }else if(num.Value == overflowVoltage + 0x12345678)
                {
                    m_overflow = true;
                    m_counter = overflowVoltage - 1;
                }
                else
                {
                    m_overflow = false;
                    m_counter = num.Value;
                }
            }
        }

        public override uint GetOutputVoltage(int face)
        {
            GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, Rotation, face);
            if (connectorDirection.HasValue)
            {
                if (connectorDirection.Value == GVElectricConnectorDirection.Top)
                {
                    return m_counter;
                }
                if (connectorDirection.Value == GVElectricConnectorDirection.Bottom)
                {
                    return m_overflow ? uint.MaxValue : 0u;
                }
            }
            return 0u;
        }

        public override bool Simulate()
        {
            uint counter = m_counter;
            bool overflow = m_overflow;
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            int rotation = Rotation;
            GigaVoltageLevelData blockData = m_subsystemGVCounterBlockBehavior.GetBlockData(CellFaces[0].Point);
            uint overflowVoltage = blockData == null ? 0u : blockData.Data;
            foreach (GVElectricConnection connection in Connections)
            {
                if (connection.ConnectorType != GVElectricConnectorType.Output && connection.NeighborConnectorType != 0)
                {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue)
                    {
                        if (connectorDirection == GVElectricConnectorDirection.Right)
                        {
                            flag = IsSignalHigh(connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace));
                        }
                        else if (connectorDirection == GVElectricConnectorDirection.Left)
                        {
                            flag2 = IsSignalHigh(connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace));
                        }
                        else if (connectorDirection == GVElectricConnectorDirection.In)
                        {
                            flag3 = IsSignalHigh(connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace));
                        }
                    }
                }
            }
            if (flag && m_plusAllowed)
            {
                m_plusAllowed = false;
                if (m_counter < overflowVoltage-1)
                {
                    m_counter++;
                    m_overflow = false;
                }
                else
                {
                    m_counter = 0u;
                    m_overflow = true;
                }
            }
            else if (flag2 && m_minusAllowed)
            {
                m_minusAllowed = false;
                if (m_counter > 0u && m_counter < overflowVoltage)
                {
                    m_counter--;
                    m_overflow = false;
                }
                else
                {
                    m_counter = overflowVoltage-1;
                    m_overflow = true;
                }
            }
            else if (flag3 && m_resetAllowed)
            {
                m_counter = 0u;
                m_overflow = false;
            }
            if (!flag)
            {
                m_plusAllowed = true;
            }
            if (!flag2)
            {
                m_minusAllowed = true;
            }
            if (!flag3)
            {
                m_resetAllowed = true;
            }
            if (m_counter != counter || m_overflow != overflow)
            {
                uint storeVoltage = m_counter;
                if(m_counter==0 && m_overflow)
                {
                    storeVoltage = overflowVoltage - 0x12345678u;
                }
                else if(m_counter==overflowVoltage && m_overflow)
                {
                    storeVoltage = overflowVoltage + 0x12345678u;
                }
                else
                {
                    storeVoltage = m_counter;
                }
                SubsystemGVElectricity.WritePersistentVoltage(CellFaces[0].Point, storeVoltage);
                return true;
            }
            return false;
        }
    }
}
