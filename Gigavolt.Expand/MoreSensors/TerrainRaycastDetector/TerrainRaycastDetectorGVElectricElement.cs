using Engine;

namespace Game {
    public class TerrainRaycastDetectorGVElectricElement : RotateableGVElectricElement {
        readonly Terrain m_terrain;
        uint m_rightInput;
        uint m_leftInput;
        uint m_topOutput;
        uint m_bottomOutput;
        uint m_inOutput;

        public TerrainRaycastDetectorGVElectricElement(SubsystemGVElectricity subsystemGVElectric, CellFace cellFace) : base(subsystemGVElectric, cellFace) => m_terrain = SubsystemGVElectricity.SubsystemTerrain.Terrain;

        public override uint GetOutputVoltage(int face) {
            GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, Rotation, face);
            if (connectorDirection.HasValue) {
                switch (connectorDirection.Value) {
                    case GVElectricConnectorDirection.Top: return m_topOutput;
                    case GVElectricConnectorDirection.Bottom: return m_bottomOutput;
                    case GVElectricConnectorDirection.In: return m_inOutput;
                }
            }
            return 0u;
        }

        public override bool Simulate() {
            uint rightInput = m_rightInput;
            m_rightInput = 0u;
            uint length = 0u;
            bool detectData = false;
            bool skipFluid = false;
            uint leftInput = m_leftInput;
            m_leftInput = 0u;
            int specifiedContent = 0;
            int specifiedData = 0;
            uint topOutput = m_topOutput;
            uint bottomOutput = m_bottomOutput;
            uint inOutput = m_inOutput;
            int rotation = Rotation;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue) {
                        if (connectorDirection == GVElectricConnectorDirection.Right) {
                            m_rightInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                            length = m_rightInput & 0xFFFu;
                            detectData = m_rightInput >> 12 == 1u;
                            skipFluid = m_rightInput >> 13 == 1u;
                        }
                        else if (connectorDirection == GVElectricConnectorDirection.Left) {
                            m_leftInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                            specifiedContent = Terrain.ExtractContents((int)m_leftInput);
                            specifiedData = Terrain.ExtractData((int)m_leftInput);
                        }
                    }
                }
            }
            if (length == 0) {
                m_inOutput = 0u;
                m_topOutput = 0u;
                m_bottomOutput = 0u;
                return m_inOutput != inOutput || m_topOutput != topOutput || m_bottomOutput != bottomOutput;
            }
            if (m_rightInput == rightInput
                && m_leftInput == leftInput) {
                return false;
            }
            if (specifiedContent == 0) {
                detectData = false;
            }
            Point3 originPosition = CellFaces[0].Point;
            Point3 direction = CellFace.FaceToPoint3(CellFaces[0].Face);
            int notZeroValue = 0;
            int detected = 0;
            int i = 1;
            for (; i <= length; i++) {
                Point3 position = originPosition + direction * i;
                if (position.Y < 0
                    && position.Y >= 256) {
                    break;
                }
                TerrainChunk chunkAtCell = m_terrain.GetChunkAtCell(position.X, position.Z);
                if (chunkAtCell == null) {
                    break;
                }
                int value = chunkAtCell.GetCellValueFast(position.X & 15, position.Y, position.Z & 15);
                int content = Terrain.ExtractContents(value);
                if (content == 0
                    || (skipFluid && (content == WaterBlock.Index || content == MagmaBlock.Index))) {
                    if (detected > 0) {
                        break;
                    }
                    continue;
                }
                if (specifiedContent > 0) {
                    if (content == specifiedContent
                        && (!detectData || Terrain.ExtractData(value) == specifiedData)) {
                        detected++;
                        if (notZeroValue == 0) {
                            notZeroValue = value;
                        }
                    }
                    else if (detected > 0) {
                        break;
                    }
                }
                else {
                    if (notZeroValue == 0) {
                        notZeroValue = value;
                    }
                    specifiedContent = content;
                    detected++;
                }
            }
            m_inOutput = (uint)detected;
            if (detected > 0) {
                m_topOutput = (uint)notZeroValue;
                m_bottomOutput = (uint)(i - detected);
            }
            else {
                m_topOutput = 0u;
                m_bottomOutput = 0u;
            }
            return m_inOutput != inOutput || m_topOutput != topOutput || m_bottomOutput != bottomOutput;
        }
    }
}