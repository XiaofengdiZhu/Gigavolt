namespace Game {
    public class TruthTableCCircuitGVElectricElement : RotateableGVElectricElement {
        public readonly SubsystemGVTruthTableCircuitCBlockBehavior m_subsystemTruthTableCircuitBlockBehavior;

        public TruthTableData m_data;
        public uint m_voltage;

        public TruthTableCCircuitGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, int value, uint subterrainId) :
            base(subsystemGVElectricity, cellFace, subterrainId) {
            m_subsystemTruthTableCircuitBlockBehavior =
                subsystemGVElectricity.Project.FindSubsystem<SubsystemGVTruthTableCircuitCBlockBehavior>(true);
            m_data = m_subsystemTruthTableCircuitBlockBehavior.GetItemData(m_subsystemTruthTableCircuitBlockBehavior.GetIdFromValue(value));
        }

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override bool Simulate() {
            if (m_data == null) {
                return false;
            }
            uint voltage = m_voltage;
            uint num = 0;
            int rotation = Rotation;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    GVElectricConnectorDirection? connectorDirection =
                        SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue) {
                        if (connectorDirection == GVElectricConnectorDirection.Top) {
                            if (IsSignalHigh(connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace))) {
                                num |= 1u;
                            }
                        }
                        else if (connectorDirection == GVElectricConnectorDirection.Right) {
                            if (IsSignalHigh(connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace))) {
                                num |= 2u;
                            }
                        }
                        else if (connectorDirection == GVElectricConnectorDirection.Bottom) {
                            if (IsSignalHigh(connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace))) {
                                num |= 4u;
                            }
                        }
                        else if (connectorDirection == GVElectricConnectorDirection.Left
                            && IsSignalHigh(connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace))) {
                            num |= 8u;
                        }
                    }
                }
            }
            m_voltage = m_data.Data[num] > 0u ? uint.MaxValue : 0u;
            return m_voltage != voltage;
        }
    }
}