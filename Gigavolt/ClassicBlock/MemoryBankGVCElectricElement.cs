using Engine;

namespace Game
{
    public class MemoryBankGVCElectricElement : RotateableGVElectricElement
    {
        public SubsystemGVMemoryBankCBlockBehavior m_subsystemMemoryBankBlockBehavior;

        public uint m_voltage;

        public bool m_writeAllowed;

        public bool m_clockAllowed;

        public MemoryBankGVCElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace)
            : base(subsystemGVElectricity, cellFace)
        {
            m_subsystemMemoryBankBlockBehavior = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVMemoryBankCBlockBehavior>(throwOnError: true);
            MemoryBankData blockData = m_subsystemMemoryBankBlockBehavior.GetBlockData(cellFace.Point);
            Log.Information(blockData != null);
            if (blockData != null)
            {
                m_voltage = blockData.LastOutput;
            }
        }

        public override uint GetOutputVoltage(int face)
        {
            return m_voltage;
        }

        public override bool Simulate()
        {
            uint voltage = m_voltage;
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            uint num = 0u;
            uint num2 = 0;
            uint num3 = 0;
            int rotation = Rotation;
            foreach (GVElectricConnection connection in Connections)
            {
                if (connection.ConnectorType != GVElectricConnectorType.Output && connection.NeighborConnectorType != 0)
                {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue)
                    {
                        if (connectorDirection == GVElectricConnectorDirection.Right)
                        {
                            num2 = MathUint.Clamp(connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace),0,15);
                        }
                        else if (connectorDirection == GVElectricConnectorDirection.Left)
                        {
                            num3 = MathUint.Clamp(connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace), 0, 15);
                        }
                        else if (connectorDirection == GVElectricConnectorDirection.Bottom)
                        {
                            uint num4 = MathUint.Clamp(connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace), 0, 15);
                            flag = (num4 >= 8);
                            flag3 = (num4 > 0 && num4 < 8);
                            flag2 = true;
                        }
                        else if (connectorDirection == GVElectricConnectorDirection.In)
                        {
                            num = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                        }
                    }
                }
            }
            MemoryBankData memoryBankData = m_subsystemMemoryBankBlockBehavior.GetBlockData(CellFaces[0].Point);
            int address = (int)(num2 + (num3 << 4));
            Log.Information(address+" "+memoryBankData.Read(address));
            if (flag2)
            {
                if (flag && m_clockAllowed)
                {
                    m_clockAllowed = false;
                    m_voltage = ((memoryBankData != null) ? (memoryBankData.Read(address)) : 0u);
                }
                else if (flag3 && m_writeAllowed)
                {
                    m_writeAllowed = false;
                    if (memoryBankData == null)
                    {
                        memoryBankData = new MemoryBankData();
                        m_subsystemMemoryBankBlockBehavior.SetBlockData(CellFaces[0].Point, memoryBankData);
                    }
                    memoryBankData.Write(address, (byte)num);
                }
            }
            else
            {
                m_voltage = ((memoryBankData != null) ? (memoryBankData.Read(address)) : 0u);
            }
            if (!flag)
            {
                m_clockAllowed = true;
            }
            if (!flag3)
            {
                m_writeAllowed = true;
            }
            if (memoryBankData != null)
            {
                memoryBankData.LastOutput = (byte)m_voltage;
            }
            return m_voltage != voltage;
        }
    }
}
