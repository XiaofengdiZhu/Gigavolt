using System;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVDetonatorCBlock : MountedGVElectricElementBlock {
        public const int Index = 836;

        public BlockMesh m_standaloneBlockMesh = new BlockMesh();

        public BlockMesh[] m_blockMeshesByData = new BlockMesh[6];

        public BoundingBox[][] m_collisionBoxesByData = new BoundingBox[6][];

        public override void Initialize() {
            Model model = ContentManager.Get<Model>("Models/Detonator");
            Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Detonator").ParentBone);
            for (int i = 0; i < 6; i++) {
                int num = i;
                Matrix m = i >= 4 ? i != 4 ? Matrix.CreateRotationX((float)Math.PI) * Matrix.CreateTranslation(0.5f, 1f, 0.5f) : Matrix.CreateTranslation(0.5f, 0f, 0.5f) : Matrix.CreateRotationX((float)Math.PI / 2f) * Matrix.CreateTranslation(0f, 0f, -0.5f) * Matrix.CreateRotationY(i * (float)Math.PI / 2f) * Matrix.CreateTranslation(0.5f, 0.5f, 0.5f);
                m_blockMeshesByData[num] = new BlockMesh();
                m_blockMeshesByData[num]
                .AppendModelMeshPart(
                    model.FindMesh("Detonator").MeshParts[0],
                    boneAbsoluteTransform * m,
                    false,
                    false,
                    false,
                    false,
                    Color.White
                );
                m_collisionBoxesByData[num] = new[] { m_blockMeshesByData[num].CalculateBoundingBox() };
            }
            Matrix m2 = Matrix.CreateRotationY(-(float)Math.PI / 2f) * Matrix.CreateRotationZ((float)Math.PI / 2f);
            m_standaloneBlockMesh.AppendModelMeshPart(
                model.FindMesh("Detonator").MeshParts[0],
                boneAbsoluteTransform * m2,
                false,
                false,
                false,
                false,
                Color.White
            );
        }

        public override int GetFace(int value) => Terrain.ExtractData(value) & 7;

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult) {
            BlockPlacementData result = default;
            result.Value = Terrain.ReplaceData(value, raycastResult.CellFace.Face);
            result.CellFace = raycastResult.CellFace;
            return result;
        }

        public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value) {
            int num = Terrain.ExtractData(value);
            if (num >= m_collisionBoxesByData.Length) {
                return null;
            }
            return m_collisionBoxesByData[num];
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) {
            int num = Terrain.ExtractData(value);
            if (num < m_blockMeshesByData.Length) {
                generator.GenerateMeshVertices(
                    this,
                    x,
                    y,
                    z,
                    m_blockMeshesByData[num],
                    Color.White,
                    null,
                    geometry.SubsetOpaque
                );
                GenerateGVWireVertices(
                    generator,
                    value,
                    x,
                    y,
                    z,
                    GetFace(value),
                    0.125f,
                    Vector2.Zero,
                    geometry.SubsetOpaque
                );
            }
        }

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData) {
            BlocksManager.DrawMeshBlock(
                primitivesRenderer,
                m_standaloneBlockMesh,
                color,
                4f * size,
                ref matrix,
                environmentData
            );
        }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z) => new DetonatorGVCElectricElement(subsystemGVElectricity, new CellFace(x, y, z, GetFace(value)));

        public override GVElectricConnectorType? GetGVConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z) {
            int face2 = GetFace(value);
            if (face == face2
                && SubsystemGVElectricity.GetConnectorDirection(face2, 0, connectorFace).HasValue) {
                return GVElectricConnectorType.Input;
            }
            return null;
        }
    }
}