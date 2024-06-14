namespace Game {
    public class TruthTableCCircuitGVElectricElement : RotateableGVElectricElement {
        public SubsystemGVTruthTableCircuitCBlockBehavior m_subsystemTruthTableCircuitBlockBehavior;

        public uint m_voltage;

        public TruthTableCCircuitGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) => m_subsystemTruthTableCircuitBlockBehavior = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVTruthTableCircuitCBlockBehavior>(true);

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override bool Simulate() {
            uint voltage = m_voltage;
            uint num = 0;
            int rotation = Rotation;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
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
            TruthTableData blockData = m_subsystemTruthTableCircuitBlockBehavior.GetBlockData(CellFaces[0].Point);
            m_voltage = blockData != null ? blockData.Data[num] > 0u ? uint.MaxValue : 0u : 0u;
            return m_voltage != voltage;
        }
    }
}