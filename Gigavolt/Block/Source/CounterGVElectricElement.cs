namespace Game {
    public class CounterGVElectricElement : RotateableGVElectricElement {
        public readonly SubsystemGVCounterBlockBehavior m_subsystemGVCounterBlockBehavior;

        public bool m_plusAllowed = true;
        public bool m_minusAllowed = true;
        public bool m_resetAllowed = true;
        public uint m_counter;
        public bool m_overflow;
        public bool m_edited;
        public readonly GVCounterData m_blockData;

        public CounterGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, GVCellFace cellFace, uint subterrainId) : base(
            subsystemGVElectricity,
            cellFace,
            subterrainId
        ) {
            m_subsystemGVCounterBlockBehavior = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVCounterBlockBehavior>(true);
            m_blockData = m_subsystemGVCounterBlockBehavior.GetItemData(m_subsystemGVCounterBlockBehavior.GetIdFromValue(value));
            uint overflowVoltage = m_blockData?.Overflow ?? 0u;
            uint initialVoltage = m_blockData?.Initial ?? 0u;
            uint? num = subsystemGVElectricity.ReadPersistentVoltage(cellFace.Point, SubterrainId);
            if (num.HasValue) {
                if (num.Value == overflowVoltage - 0x12345678) {
                    m_overflow = true;
                    m_counter = initialVoltage;
                }
                else if (num.Value == overflowVoltage + 0x12345678) {
                    m_overflow = true;
                    m_counter = overflowVoltage - 1;
                }
                else {
                    m_overflow = false;
                    m_counter = num.Value;
                }
            }
            if (SubsystemGVElectricity.GetGVElectricElement(cellFace.X, cellFace.Y, cellFace.Z, cellFace.Face, subterrainId)
                is CounterGVElectricElement { m_edited: true } electricElement) {
                m_counter = electricElement.m_counter;
                m_edited = true;
            }
            if (m_counter < initialVoltage) {
                m_counter = initialVoltage;
            }
        }

        public override uint GetOutputVoltage(int face) {
            GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, Rotation, face);
            if (connectorDirection.HasValue) {
                switch (connectorDirection.Value) {
                    case GVElectricConnectorDirection.Top: return m_counter;
                    case GVElectricConnectorDirection.Bottom: return m_overflow ? uint.MaxValue : 0u;
                }
            }
            return 0u;
        }

        public override bool Simulate() {
            if (m_edited) {
                m_edited = false;
                return true;
            }
            uint counter = m_counter;
            bool overflow = m_overflow;
            bool flag = false;
            bool flag2 = false;
            bool flag3 = false;
            int rotation = Rotation;
            uint overflowVoltage = m_blockData?.Overflow ?? 0u;
            uint initialVoltage = m_blockData?.Initial ?? 0u;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    GVElectricConnectorDirection? connectorDirection =
                        SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue) {
                        if (connectorDirection == GVElectricConnectorDirection.Right) {
                            flag = IsSignalHigh(connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace));
                        }
                        else if (connectorDirection == GVElectricConnectorDirection.Left) {
                            flag2 = IsSignalHigh(connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace));
                        }
                        else if (connectorDirection == GVElectricConnectorDirection.In) {
                            flag3 = IsSignalHigh(connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace));
                        }
                    }
                }
            }
            if (flag && m_plusAllowed) {
                m_plusAllowed = false;
                if (m_counter < overflowVoltage - 1) {
                    m_counter++;
                    m_overflow = false;
                }
                else {
                    m_counter = initialVoltage;
                    m_overflow = true;
                }
            }
            else if (flag2 && m_minusAllowed) {
                m_minusAllowed = false;
                if (m_counter > initialVoltage
                    && (overflowVoltage == 0u || m_counter < overflowVoltage)) {
                    m_counter--;
                    m_overflow = false;
                }
                else {
                    m_counter = overflowVoltage - 1;
                    m_overflow = true;
                }
            }
            else if (flag3 && m_resetAllowed) {
                m_counter = initialVoltage;
                m_overflow = false;
            }
            if (!flag) {
                m_plusAllowed = true;
            }
            if (!flag2) {
                m_minusAllowed = true;
            }
            if (!flag3) {
                m_resetAllowed = true;
            }
            if (m_counter != counter
                || m_overflow != overflow) {
                uint storeVoltage;
                if (m_counter == initialVoltage && m_overflow) {
                    storeVoltage = overflowVoltage - 0x12345678u;
                }
                else if (m_counter == overflowVoltage && m_overflow) {
                    storeVoltage = overflowVoltage + 0x12345678u;
                }
                else {
                    storeVoltage = m_counter;
                }
                SubsystemGVElectricity.WritePersistentVoltage(CellFaces[0].Point, storeVoltage, SubterrainId);
                return true;
            }
            return false;
        }
    }
}