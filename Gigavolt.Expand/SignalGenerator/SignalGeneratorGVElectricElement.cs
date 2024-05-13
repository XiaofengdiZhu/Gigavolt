using Engine;

namespace Game {
    public class SignalGeneratorGVElectricElement : RotateableGVElectricElement {
        public readonly SubsystemTerrain m_subsystemTerrain;
        public uint m_output;
        public uint m_rightTopInput;
        public uint m_rightBottomInput;
        public uint m_leftTopInput;
        public uint m_leftBottomInput;
        public uint m_bottomBottomInput;

        public SignalGeneratorGVElectricElement(SubsystemGVElectricity subsystemGVElectric, GVCellFace[] cellFaces) : base(subsystemGVElectric, cellFaces) => m_subsystemTerrain = SubsystemGVElectricity.SubsystemTerrain;

        public override uint GetOutputVoltage(int face) => m_output;

        public override bool Simulate() {
            uint output = m_output;
            uint rightTopInput = m_rightTopInput;
            uint rightBottomInput = m_rightBottomInput;
            uint leftTopInput = m_leftTopInput;
            uint leftBottomInput = m_leftBottomInput;
            uint bottomBottomInput = m_bottomBottomInput;
            m_output = 0u;
            m_rightTopInput = 0u;
            m_rightBottomInput = 0u;
            m_leftTopInput = 0u;
            m_leftBottomInput = 0u;
            m_bottomBottomInput = 0u;
            int rotation = Rotation;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue) {
                        switch (connectorDirection) {
                            case GVElectricConnectorDirection.Right:
                                if (connection.CellFace == CellFaces[0]) {
                                    m_rightBottomInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                }
                                else {
                                    m_rightTopInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                }
                                break;
                            case GVElectricConnectorDirection.Bottom:
                                m_bottomBottomInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                break;
                            case GVElectricConnectorDirection.Left:
                                if (connection.CellFace == CellFaces[0]) {
                                    m_leftBottomInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                }
                                else {
                                    m_leftTopInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                }
                                break;
                        }
                    }
                }
            }
            return m_output != output;
        }

        public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner) {
            GVCellFace bottomCellFace = CellFaces[0];
            int face = bottomCellFace.Face;
            int data = Terrain.ExtractData(m_subsystemTerrain.Terrain.GetCellValue(bottomCellFace.X, bottomCellFace.Y, bottomCellFace.Z));
            int rotation = RotateableMountedGVElectricElementBlock.GetRotation(data);
            Point3 nextUp = GVSignalGeneratorBlock.m_upPoint3[face * 4 + rotation + 1] + bottomCellFace.Point;
            if (Terrain.ExtractContents(m_subsystemTerrain.Terrain.GetCellValue(nextUp.X, nextUp.Y, nextUp.Z)) == 0) {
                Point3 faceDirection = -CellFace.FaceToPoint3(face);
                int faceValue = m_subsystemTerrain.Terrain.GetCellValue(nextUp.X + faceDirection.X, nextUp.Y + faceDirection.Y, nextUp.Z + faceDirection.Z);
                Block block = BlocksManager.Blocks[Terrain.ExtractContents(faceValue)];
                if ((block.IsCollidable_(faceValue) && !block.IsFaceTransparent(m_subsystemTerrain, face, faceValue))
                    || (face == 4 && block is FenceBlock)) {
                    m_subsystemTerrain.ChangeCell(nextUp.X, nextUp.Y, nextUp.Z, Terrain.MakeBlockValue(GVSignalGeneratorBlock.Index, 0, RotateableMountedGVElectricElementBlock.SetRotation(GVSignalGeneratorBlock.SetIsTopPart(data, true), rotation + 1)));
                    Rotation = rotation + 1;
                    Point3 upDirection = GVSignalGeneratorBlock.m_upPoint3[face * 4 + rotation];
                    m_subsystemTerrain.ChangeCell(upDirection.X + bottomCellFace.X, upDirection.Y + bottomCellFace.Y, upDirection.Z + bottomCellFace.Z, 0);
                    return true;
                }
            }
            return false;
        }
    }
}