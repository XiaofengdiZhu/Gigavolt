using System;
using System.Linq;
using Engine;

namespace Game {
    public class JavascriptMicrocontrollerGVElectricElement : RotateableGVElectricElement {
        public static readonly uint[] DefaultOutputs = { 0, 0, 0, 0, 0 };
        public uint[] m_inputs = (uint[])DefaultOutputs.Clone();
        public uint[] m_outputs = (uint[])DefaultOutputs.Clone();
        public bool m_blockDataInitiated;
        public GVJavascriptMicrocontrollerData m_blockData;
        public SubsystemGVJavascriptMicrocontrollerBlockBehavior m_subsystemJavascriptMicrocontrollerBlockBehavior;
        public SubsystemTerrain m_subsystemTerrain;

        public JavascriptMicrocontrollerGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace) : base(subsystemGVElectricity, cellFace) {
            m_subsystemJavascriptMicrocontrollerBlockBehavior = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVJavascriptMicrocontrollerBlockBehavior>(true);
            m_subsystemTerrain = subsystemGVElectricity.Project.FindSubsystem<SubsystemTerrain>(true);
        }

        public override uint GetOutputVoltage(int face) {
            if (m_blockDataInitiated) {
                GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, Rotation, face);
                if (connectorDirection.HasValue) {
                    int direction = (int)connectorDirection.Value;
                    if (m_blockData.m_portsDefinition[direction] == 1) {
                        return m_outputs[direction];
                    }
                }
            }
            return 0u;
        }

        public override bool Simulate() {
            if (!m_blockDataInitiated) {
                m_blockData = m_subsystemJavascriptMicrocontrollerBlockBehavior.GetBlockData(CellFaces[0].Point);
            }
            if (m_blockData == null) {
                m_blockDataInitiated = false;
                return false;
            }
            m_blockDataInitiated = true;
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
            if (m_inputs.SequenceEqual(lastInputs)) {
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
                TerrainChunk chunkAtCell = m_subsystemTerrain.Terrain.GetChunkAtCell(CellFaces[0].X, CellFaces[0].Z);
                ++chunkAtCell.ModificationCounter;
                m_subsystemTerrain.TerrainUpdater.DowngradeChunkNeighborhoodState(chunkAtCell.Coords, 1, TerrainChunkState.InvalidLight, true);
                m_subsystemTerrain.m_modifiedCells[CellFaces[0].Point] = true;
            }
            return m_outputs.SequenceEqual(lastOutputs);
        }
    }
}