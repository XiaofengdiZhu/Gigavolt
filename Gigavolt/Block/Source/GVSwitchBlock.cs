using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVSwitchBlock : MountedGVElectricElementBlock {
        public const int Index = 820;

        public BlockMesh m_standaloneBlockMesh = new BlockMesh();

        public BlockMesh[] m_blockMeshesByIndex = new BlockMesh[12];

        public BoundingBox[][] m_collisionBoxesByIndex = new BoundingBox[12][];

        public override void Initialize() {
            Model model = ContentManager.Get<Model>("Models/Switch");
            Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Body").ParentBone);
            Matrix boneAbsoluteTransform2 = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Lever").ParentBone);
            for (int i = 0; i < 6; i++) {
                for (int j = 0; j < 2; j++) {
                    int num = (i << 1) | j;
                    Matrix matrix = i >= 4 ? i != 4 ? Matrix.CreateRotationX((float)Math.PI) * Matrix.CreateTranslation(0.5f, 1f, 0.5f) : Matrix.CreateTranslation(0.5f, 0f, 0.5f) : Matrix.CreateRotationX((float)Math.PI / 2f) * Matrix.CreateTranslation(0f, 0f, -0.5f) * Matrix.CreateRotationY(i * (float)Math.PI / 2f) * Matrix.CreateTranslation(0.5f, 0.5f, 0.5f);
                    Matrix matrix2 = Matrix.CreateRotationX(j == 0 ? MathUtils.DegToRad(30f) : MathUtils.DegToRad(-30f));
                    m_blockMeshesByIndex[num] = new BlockMesh();
                    m_blockMeshesByIndex[num]
                    .AppendModelMeshPart(
                        model.FindMesh("Body").MeshParts[0],
                        boneAbsoluteTransform * matrix,
                        false,
                        false,
                        false,
                        false,
                        Color.White
                    );
                    m_blockMeshesByIndex[num]
                    .AppendModelMeshPart(
                        model.FindMesh("Lever").MeshParts[0],
                        boneAbsoluteTransform2 * matrix2 * matrix,
                        false,
                        false,
                        false,
                        false,
                        Color.White
                    );
                    m_collisionBoxesByIndex[num] = new[] { m_blockMeshesByIndex[num].CalculateBoundingBox() };
                }
            }
            Matrix matrix3 = Matrix.CreateRotationY(-(float)Math.PI / 2f) * Matrix.CreateRotationZ((float)Math.PI / 2f);
            m_standaloneBlockMesh.AppendModelMeshPart(
                model.FindMesh("Body").MeshParts[0],
                boneAbsoluteTransform * matrix3,
                false,
                false,
                false,
                false,
                Color.White
            );
            m_standaloneBlockMesh.AppendModelMeshPart(
                model.FindMesh("Lever").MeshParts[0],
                boneAbsoluteTransform2 * matrix3,
                false,
                false,
                false,
                false,
                Color.White
            );
        }

        public static bool GetLeverState(int value) => (Terrain.ExtractData(value) & 1) != 0;

        public static int SetLeverState(int value, bool state) => Terrain.ReplaceData(value, state ? Terrain.ExtractData(value) | 1 : Terrain.ExtractData(value) & -2);

        public override int GetFace(int value) => (Terrain.ExtractData(value) >> 1) & 7;
        public static int SetFace(int value, int face) => Terrain.ReplaceData(value, (Terrain.ExtractData(value) & -15) | ((face & 7) << 1));

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult) {
            BlockPlacementData result = default;
            result.Value = SetFace(SetLeverState(value, false), raycastResult.CellFace.Face);
            result.CellFace = raycastResult.CellFace;
            return result;
        }

        public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris) {
            dropValues.Add(new BlockDropValue { Value = Terrain.MakeBlockValue(Index, 0, Terrain.ExtractData(SetFace(SetLeverState(oldValue, false), 0))), Count = 1 });
            showDebris = true;
        }

        public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value) {
            int num = CalculateIndex(value);
            if (num >= m_collisionBoxesByIndex.Length) {
                return null;
            }
            return m_collisionBoxesByIndex[num];
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) {
            int num = CalculateIndex(value);
            if (num < m_blockMeshesByIndex.Length) {
                generator.GenerateMeshVertices(
                    this,
                    x,
                    y,
                    z,
                    m_blockMeshesByIndex[num],
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
                    0.25f,
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
                2f * size,
                ref matrix,
                environmentData
            );
        }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z) => new SwitchGVElectricElement(subsystemGVElectricity, new CellFace(x, y, z, GetFace(value)), value);

        public override GVElectricConnectorType? GetGVConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z) {
            int face2 = GetFace(value);
            if (face == face2
                && SubsystemGVElectricity.GetConnectorDirection(face2, 0, connectorFace).HasValue) {
                return GVElectricConnectorType.Output;
            }
            return null;
        }

        public int CalculateIndex(int value) {
            int face = GetFace(value);
            bool leverState = GetLeverState(value);
            return (face << 1) | (leverState ? 1 : 0);
        }

        public override bool IsNonDuplicable_(int value) => ((Terrain.ExtractData(value) >> 4) & 4095) > 0;
    }
}