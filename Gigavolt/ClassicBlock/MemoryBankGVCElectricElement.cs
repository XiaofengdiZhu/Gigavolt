namespace Game {
    public class MemoryBankGVCElectricElement : RotateableGVElectricElement {
        public readonly SubsystemGVMemoryBankCBlockBehavior m_subsystemMemoryBankBlockBehavior;

        public readonly MemoryBankData m_data;
        public uint m_voltage;
        public bool m_writeAllowed;
        public bool m_clockAllowed;

        public MemoryBankGVCElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, int value, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) {
            m_subsystemMemoryBankBlockBehavior = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVMemoryBankCBlockBehavior>(true);
            m_data = m_subsystemMemoryBankBlockBehavior.GetItemData(m_subsystemMemoryBankBlockBehavior.GetIdFromValue(value));
            if (m_data != null) {
                m_voltage = m_data.LastOutput;
            }
        }

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override bool Simulate() {
            if (m_data == null) {
                return false;
            }
            uint voltage = m_voltage;
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            uint num = 0u;
            uint num2 = 0;
            uint num3 = 0;
            int rotation = Rotation;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue) {
                        if (connectorDirection == GVElectricConnectorDirection.Right) {
                            num2 = MathUint.Clamp(connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace), 0, 15);
                        }
                        else if (connectorDirection == GVElectricConnectorDirection.Left) {
                            num3 = MathUint.Clamp(connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace), 0, 15);
                        }
                        else if (connectorDirection == GVElectricConnectorDirection.Bottom) {
                            uint num4 = MathUint.Clamp(connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace), 0, 15);
                            flag = num4 >= 8;
                            flag3 = num4 > 0 && num4 < 8;
                            flag2 = true;
                        }
                        else if (connectorDirection == GVElectricConnectorDirection.In) {
                            num = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                        }
                    }
                }
            }
            int address = (int)(num2 + (num3 << 4));
            if (flag2) {
                if (flag && m_clockAllowed) {
                    m_clockAllowed = false;
                    m_voltage = m_data.Read(address);
                }
                else if (flag3 && m_writeAllowed) {
                    m_writeAllowed = false;
                    m_data.Write(address, (byte)num);
                }
            }
            else {
                m_voltage = m_data.Read(address);
            }
            if (!flag) {
                m_clockAllowed = true;
            }
            if (!flag3) {
                m_writeAllowed = true;
            }
            m_data.LastOutput = (byte)m_voltage;
            return m_voltage != voltage;
        }
    }
}