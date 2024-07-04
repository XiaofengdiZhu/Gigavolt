using System;
using System.Linq;
using Engine;

namespace Game {
    public class JavascriptMicrocontrollerGVElectricElement : RotateableGVElectricElement {
        public static readonly uint[] DefaultOutputs = [
            0,
            0,
            0,
            0,
            0
        ];

        public uint[] m_inputs = (uint[])DefaultOutputs.Clone();
        public uint[] m_outputs = (uint[])DefaultOutputs.Clone();
        public readonly GVJavascriptMicrocontrollerData m_blockData;
        public readonly SubsystemGVJavascriptMicrocontrollerBlockBehavior m_subsystemJavascriptMicrocontrollerBlockBehavior;
        public int m_executeAgainCircuitStep = -1;

        public JavascriptMicrocontrollerGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, int value, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) {
            m_subsystemJavascriptMicrocontrollerBlockBehavior = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVJavascriptMicrocontrollerBlockBehavior>(true);
            m_blockData = m_subsystemJavascriptMicrocontrollerBlockBehavior.GetItemData(m_subsystemJavascriptMicrocontrollerBlockBehavior.GetIdFromValue(value));
        }

        public override uint GetOutputVoltage(int face) {
            GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, Rotation, face);
            if (connectorDirection.HasValue) {
                int direction = (int)connectorDirection.Value;
                if (m_blockData.m_portsDefinition[direction] == 1) {
                    return m_outputs[direction];
                }
            }
            return 0u;
        }

        public override bool Simulate() {
            if (m_blockData == null) {
                return false;
            }
            bool flag = m_executeAgainCircuitStep != SubsystemGVElectricity.CircuitStep;
            m_executeAgainCircuitStep = -1;
            int rotation = Rotation;
            uint[] lastInputs = (uint[])m_inputs.Clone();
            m_inputs = (uint[])DefaultOutputs.Clone();
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue) {
                        m_inputs[(int)connectorDirection.Value] = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                    }
                }
            }
            if (flag && m_inputs.SequenceEqual(lastInputs)) {
                return false;
            }
            uint[] lastOutputs = (uint[])m_outputs.Clone();
            int[] lastPortsDefinition = (int[])m_blockData.m_portsDefinition.Clone();
            try {
                m_outputs = m_blockData.Exe(m_inputs, CellFaces[0].Point);
            }
            catch (Exception e) {
                Log.Error(e);
            }
            if (!m_blockData.m_portsDefinition.SequenceEqual(lastPortsDefinition)) {
                Point3 point = CellFaces[0].Point;
                if (SubterrainId == 0) {
                    TerrainChunk chunkAtCell = SubsystemGVElectricity.SubsystemTerrain.Terrain.GetChunkAtCell(point.X, point.Z);
                    ++chunkAtCell.ModificationCounter;
                    SubsystemGVElectricity.SubsystemTerrain.TerrainUpdater.DowngradeChunkNeighborhoodState(chunkAtCell.Coords, 1, TerrainChunkState.InvalidLight, true);
                    SubsystemGVElectricity.SubsystemTerrain.m_modifiedCells[point] = true;
                }
                else {
                    GVSubterrainSystem system = GVStaticStorage.GVSubterrainSystemDictionary[SubterrainId];
                    TerrainChunk chunkAtCell = system.Terrain.GetChunkAtCell(point.X, point.Z);
                    ++chunkAtCell.ModificationCounter;
                    system.TerrainUpdater.DowngradeChunkNeighborhoodState(chunkAtCell.Coords, 1, TerrainChunkState.InvalidLight, true);
                    system.m_modifiedCells[point] = true;
                }
            }
            if (m_blockData.m_executeAgain > 0) {
                m_executeAgainCircuitStep = SubsystemGVElectricity.CircuitStep + m_blockData.m_executeAgain;
                SubsystemGVElectricity.QueueGVElectricElementForSimulation(this, m_executeAgainCircuitStep);
            }
            m_blockData.m_executeAgain = 0;
            return !m_outputs.SequenceEqual(lastOutputs);
        }
    }
}