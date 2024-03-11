namespace Game {
    public class VolatileFourDimensionalMemoryBankGVElectricElement : RotateableGVElectricElement {
        public SubsystemGVVolatileFourDimensionalMemoryBankBlockBehavior m_SubsystemGVMemoryBankBlockBehavior;

        public uint m_voltage;
        public bool m_writeAllowed;
        public bool m_clockAllowed;

        public VolatileFourDimensionalMemoryBankGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace) : base(subsystemGVElectricity, cellFace) => m_SubsystemGVMemoryBankBlockBehavior = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVVolatileFourDimensionalMemoryBankBlockBehavior>(true);

        public override void OnAdded() { }

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override bool Simulate() {
            uint voltage = m_voltage;
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            uint num = 0u;
            uint num2 = 0u;
            uint num3 = 0u;
            int rotation = Rotation;
            bool hasInput = false;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue) {
                        if (connectorDirection == GVElectricConnectorDirection.Right) {
                            num2 = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                            hasInput = true;
                        }
                        else if (connectorDirection == GVElectricConnectorDirection.Left) {
                            num3 = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                            hasInput = true;
                        }
                        else if (connectorDirection == GVElectricConnectorDirection.Bottom) {
                            uint num4 = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                            flag = num4 >= 8u;
                            flag3 = num4 > 0u && num4 < 8u;
                            flag2 = true;
                            hasInput = true;
                        }
                        else if (connectorDirection == GVElectricConnectorDirection.In) {
                            num = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                            hasInput = true;
                        }
                    }
                }
            }
            GVVolatileFourDimensionalMemoryBankData memoryBankData = m_SubsystemGVMemoryBankBlockBehavior.GetBlockData(CellFaces[0].Point);
            if (memoryBankData != null) {
                int x = (int)(num2 & 0xffffu);
                int y = (int)(num2 >> 16);
                int z = (int)(num3 & 0xffffu);
                int w = (int)(num3 >> 16);
                if (flag2) {
                    if (flag && m_clockAllowed) {
                        m_clockAllowed = false;
                        m_voltage = memoryBankData.Read(x, y, z, w);
                    }
                    else if (flag3 && m_writeAllowed) {
                        m_writeAllowed = false;
                        memoryBankData.Write(
                            x,
                            y,
                            z,
                            w,
                            num
                        );
                    }
                }
                else {
                    m_voltage = memoryBankData.Read(x, y, z, w);
                }
                if (!flag) {
                    m_clockAllowed = true;
                }
                if (!flag3) {
                    m_writeAllowed = true;
                }
                if (!hasInput) {
                    m_voltage = memoryBankData.m_ID;
                }
            }
            return m_voltage != voltage;
        }
    }
}