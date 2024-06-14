using System;
using Engine;
using Engine.Graphics;

namespace Game {
    public abstract class RotateableMountedGVElectricElementBlock : MountedGVElectricElementBlock {
        public string m_modelName;

        public string m_meshName;

        public float m_centerBoxSize;

        public readonly BlockMesh[] m_blockMeshes = new BlockMesh[24];

        public readonly BlockMesh m_standaloneBlockMesh = new();

        public readonly BoundingBox[][] m_collisionBoxes = new BoundingBox[24][];

        public RotateableMountedGVElectricElementBlock(string modelName, string meshName, float centerBoxSize) {
            m_modelName = modelName;
            m_meshName = meshName;
            m_centerBoxSize = centerBoxSize;
        }

        public override void Initialize() {
            RotateableMountedGVElectricElementBlockInitialize();
        }

        public void RotateableMountedGVElectricElementBlockInitialize() {
            ModelMesh modelMesh = ContentManager.Get<Model>(m_modelName).FindMesh(m_meshName);
            ModelMeshPart modelMeshPart = modelMesh.MeshParts[0];
            Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(modelMesh.ParentBone);
            for (int i = 0; i < 6; i++) {
                float radians;
                bool flag;
                if (i < 4) {
                    radians = i * (float)Math.PI / 2f;
                    flag = false;
                }
                else if (i == 4) {
                    radians = -(float)Math.PI / 2f;
                    flag = true;
                }
                else {
                    radians = (float)Math.PI / 2f;
                    flag = true;
                }
                for (int j = 0; j < 4; j++) {
                    int num = (i << 2) + j;
                    Matrix m = Matrix.CreateRotationX((float)Math.PI / 2f) * Matrix.CreateRotationZ(-j * (float)Math.PI / 2f) * Matrix.CreateTranslation(0f, 0f, -0.5f) * (flag ? Matrix.CreateRotationX(radians) : Matrix.CreateRotationY(radians)) * Matrix.CreateTranslation(0.5f, 0.5f, 0.5f);
                    BlockMesh blockMesh = new();
                    blockMesh.AppendModelMeshPart(
                        modelMeshPart,
                        boneAbsoluteTransform * m,
                        false,
                        false,
                        false,
                        false,
                        Color.White
                    );
                    m_blockMeshes[num] = blockMesh;
                    m_collisionBoxes[num] = [blockMesh.CalculateBoundingBox()];
                }
            }
            Matrix m2 = Matrix.CreateRotationY(-(float)Math.PI / 2f) * Matrix.CreateRotationZ((float)Math.PI / 2f);
            m_standaloneBlockMesh.AppendModelMeshPart(
                modelMeshPart,
                boneAbsoluteTransform * m2,
                false,
                false,
                false,
                false,
                Color.White
            );
        }

        public override int GetFace(int value) => (Terrain.ExtractData(value) >> 2) & 7;
        public static int GetFaceFromDataStatic(int data) => (data >> 2) & 7;

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

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) {
            int num = Terrain.ExtractData(value) & 0x1F;
            generator.GenerateMeshVertices(
                this,
                x,
                y,
                z,
                m_blockMeshes[num],
                Color.White,
                null,
                geometry.SubsetOpaque
            );
            GVBlockGeometryGenerator.GenerateGVWireVertices(
                generator,
                value,
                x,
                y,
                z,
                GetFace(value),
                m_centerBoxSize,
                Vector2.Zero,
                geometry.SubsetOpaque
            );
        }

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult) {
            int rotation = 0;
            if (raycastResult.CellFace.Face >= 4) {
                Vector3 forward = Matrix.CreateFromQuaternion(componentMiner.ComponentCreature.ComponentCreatureModel.EyeRotation).Forward;
                float num = Vector3.Dot(forward, Vector3.UnitZ);
                float num2 = Vector3.Dot(forward, Vector3.UnitX);
                float num3 = Vector3.Dot(forward, -Vector3.UnitZ);
                float num4 = Vector3.Dot(forward, -Vector3.UnitX);
                float max = MathUtils.Max(num, num2, num3, num4);
                if (num == max) {
                    rotation = 2;
                }
                else if (num2 == max) {
                    rotation = 1;
                }
                else if (num3 == max) {
                    rotation = 0;
                }
                else if (num4 == max) {
                    rotation = 3;
                }
            }
            int num5 = Terrain.ExtractData(value);
            num5 &= -29;
            num5 |= raycastResult.CellFace.Face << 2;
            BlockPlacementData result = default;
            result.Value = Terrain.MakeBlockValue(BlockIndex, 0, SetRotation(num5, rotation));
            result.CellFace = raycastResult.CellFace;
            return result;
        }

        public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value) {
            int num = Terrain.ExtractData(value) & 0x1F;
            return m_collisionBoxes[num];
        }

        public static int GetRotation(int data) => data & 3;

        public static int SetRotation(int data, int rotation) => (data & -4) | (rotation & 3);
    }
}