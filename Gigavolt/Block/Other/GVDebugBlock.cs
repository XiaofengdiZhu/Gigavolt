using System;
using System.Collections.Generic;
using System.Globalization;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVDebugBlock : Block {
        public const int Index = 842;

        public BlockMesh[] m_standaloneBlockMeshes = new BlockMesh[2];

        public BlockMesh[] m_blockMeshes = new BlockMesh[2];

        public BoundingBox[][] m_collisionBoxes = new BoundingBox[2][];

        public override void Initialize() {
            Model model = ContentManager.Get<Model>("Models/Graves");
            for (int i = 0; i < 2; i++) {
                int variant = 1;
                float radians = GetRotation(i) == 0 ? 0f : (float)Math.PI / 2f;
                string name = "Grave" + (variant % 4 + 1).ToString(CultureInfo.InvariantCulture);
                Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh(name).ParentBone);
                m_blockMeshes[i] = new BlockMesh();
                m_blockMeshes[i]
                .AppendModelMeshPart(
                    model.FindMesh(name).MeshParts[0],
                    boneAbsoluteTransform * Matrix.CreateRotationY(radians) * Matrix.CreateTranslation(0.5f, 0f, 0.5f),
                    false,
                    false,
                    false,
                    false,
                    Color.White
                );
                m_standaloneBlockMeshes[i] = new BlockMesh();
                m_standaloneBlockMeshes[i]
                .AppendModelMeshPart(
                    model.FindMesh(name).MeshParts[0],
                    boneAbsoluteTransform * Matrix.CreateTranslation(0f, -0.5f, 0f),
                    false,
                    false,
                    false,
                    false,
                    Color.White
                );
                m_collisionBoxes[i] = new[] { m_blockMeshes[i].CalculateBoundingBox() };
            }
            base.Initialize();
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) {
            int num = Terrain.ExtractData(value);
            if (num < m_blockMeshes.Length) {
                generator.GenerateMeshVertices(
                    color: Color.White,
                    block: this,
                    x: x,
                    y: y,
                    z: z,
                    blockMesh: m_blockMeshes[num],
                    matrix: Matrix.Identity,
                    subset: geometry.SubsetOpaque
                );
            }
        }

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData) {
            int num = Terrain.ExtractData(value);
            if (num < m_blockMeshes.Length) {
                BlocksManager.DrawMeshBlock(
                    primitivesRenderer,
                    m_standaloneBlockMeshes[num],
                    color,
                    size,
                    ref matrix,
                    environmentData
                );
            }
        }

        public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value) {
            int num = Terrain.ExtractData(value);
            if (num < m_collisionBoxes.Length) {
                return m_collisionBoxes[num];
            }
            return base.GetCustomCollisionBoxes(terrain, value);
        }

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult) {
            int data = Terrain.ExtractData(value);
            Vector3 forward = Matrix.CreateFromQuaternion(componentMiner.ComponentCreature.ComponentCreatureModel.EyeRotation).Forward;
            float num = MathUtils.Abs(Vector3.Dot(forward, Vector3.UnitX));
            BlockPlacementData result;
            if (MathUtils.Abs(Vector3.Dot(forward, Vector3.UnitZ)) > num) {
                result = default;
                result.Value = Terrain.MakeBlockValue(Index, 0, SetRotation(data, 0));
                result.CellFace = raycastResult.CellFace;
                return result;
            }
            result = default;
            result.Value = Terrain.MakeBlockValue(Index, 0, SetRotation(data, 1));
            result.CellFace = raycastResult.CellFace;
            return result;
        }

        public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris) {
            showDebris = true;
            dropValues.Add(new BlockDropValue { Value = Terrain.MakeBlockValue(Index, 0, 0), Count = 1 });
        }

        public static int GetRotation(int data) => data & 1;

        public static int SetRotation(int data, int rotation) => (data & -2) | (rotation & 1);
    }
}