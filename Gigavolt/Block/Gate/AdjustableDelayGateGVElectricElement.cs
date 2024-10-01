using System;
using Engine;

namespace Game {
    public class AdjustableDelayGateGVElectricElement : BaseDelayGateGVElectricElement {
        public readonly int m_delaySteps;

        public override int DelaySteps => m_delaySteps;

        public AdjustableDelayGateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, int value, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) => m_delaySteps = GVAdjustableDelayGateBlock.GetDelay(Terrain.ExtractData(value));

        public override bool Simulate() {
            uint voltage = m_voltage;
            uint num = 0;
            int rotation = Rotation;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue) {
                        switch (connectorDirection.Value) {
                            case GVElectricConnectorDirection.Bottom:
                                num = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                break;
                            case GVElectricConnectorDirection.In:
                                int delay = Math.Min(MathUint.ToIntWithClamp(connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace)), 0xFF);
                                if (delay != DelaySteps) {
                                    Point3 point = CellFaces[0].Point;
                                    int oldData = Terrain.ExtractData(SubsystemGVElectricity.SubsystemGVSubterrain.GetTerrain(SubterrainId).GetCellValue(point.X, point.Y, point.Z));
                                    SubsystemGVElectricity.SubsystemGVSubterrain.ChangeCell(
                                        point.X,
                                        point.Y,
                                        point.Z,
                                        SubterrainId,
                                        Terrain.MakeBlockValue(GVBlocksManager.GetBlockIndex<GVAdjustableDelayGateBlock>(), 0, GVAdjustableDelayGateBlock.SetDelay(oldData, delay))
                                    );
                                }
                                break;
                        }
                    }
                }
            }
            if (DelaySteps > 0) {
                if (m_voltagesHistory.TryGetValue(SubsystemGVElectricity.CircuitStep, out uint value)) {
                    m_voltage = value;
                    m_voltagesHistory.Remove(SubsystemGVElectricity.CircuitStep);
                }
                if (num != m_lastStoredVoltage) {
                    m_lastStoredVoltage = num;
                    if (m_voltagesHistory.Count < 300) {
                        m_voltagesHistory[SubsystemGVElectricity.CircuitStep + DelaySteps] = num;
                        SubsystemGVElectricity.QueueGVElectricElementForSimulation(this, SubsystemGVElectricity.CircuitStep + DelaySteps);
                    }
                }
            }
            else {
                m_voltage = num;
            }
            return m_voltage != voltage;
        }
    }
}