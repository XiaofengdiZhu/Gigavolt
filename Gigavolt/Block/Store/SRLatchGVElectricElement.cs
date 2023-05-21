namespace Game {
    public class SRLatchGVElectricElement : RotateableGVElectricElement {
        public bool m_setAllowed = true;

        public bool m_resetAllowed = true;

        public bool m_clockAllowed = true;

        public uint m_voltage;

        public SRLatchGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace) : base(subsystemGVElectricity, cellFace) {
            uint? num = subsystemGVElectricity.ReadPersistentVoltage(cellFace.Point);
            if (num.HasValue) {
                m_voltage = num.Value;
            }
        }

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override bool Simulate() {
            uint voltage = m_voltage;
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            bool flag4 = false;
            int rotation = Rotation;
            uint sVoltage = 0u;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue) {
                        if (connectorDirection == GVElectricConnectorDirection.Right) {
                            flag2 = IsSignalHigh(connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace));
                        }
                        else if (connectorDirection == GVElectricConnectorDirection.Left) {
                            sVoltage = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                            flag = sVoltage > 0;
                        }
                        else if (connectorDirection == GVElectricConnectorDirection.Bottom) {
                            flag3 = IsSignalHigh(connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace));
                            flag4 = true;
                        }
                    }
                }
            }
            if (flag4) {
                if (flag3 && m_clockAllowed) {
                    m_clockAllowed = false;
                    if (flag && flag2) {
                        m_voltage = m_voltage == 0u ? sVoltage : 0u;
                    }
                    else if (flag) {
                        m_voltage = sVoltage;
                    }
                    else if (flag2) {
                        m_voltage = 0u;
                    }
                }
            }
            else if (flag && m_setAllowed) {
                m_setAllowed = false;
                m_voltage = sVoltage;
            }
            else if (flag2 && m_resetAllowed) {
                m_resetAllowed = false;
                m_voltage = 0u;
            }
            if (!flag3) {
                m_clockAllowed = true;
            }
            if (!flag) {
                m_setAllowed = true;
            }
            if (!flag2) {
                m_resetAllowed = true;
            }
            if (m_voltage != voltage) {
                SubsystemGVElectricity.WritePersistentVoltage(CellFaces[0].Point, m_voltage);
                return true;
            }
            return false;
        }
    }
}