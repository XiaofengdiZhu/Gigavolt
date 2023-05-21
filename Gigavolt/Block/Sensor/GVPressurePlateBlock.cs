using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVPressurePlateBlock : MountedGVElectricElementBlock {
        public const int Index = 844;

        public BlockMesh[] m_standaloneBlockMeshesByMaterial = new BlockMesh[2];

        public BlockMesh[] m_blockMeshesByData = new BlockMesh[16];

        public BoundingBox[][] m_collisionBoxesByData = new BoundingBox[16][];

        public string[] m_displayNamesByMaterial = new string[2] { "木质GV压力板", "石质GV压力板" };

        public int[] m_creativeValuesByMaterial = new int[2] { Terrain.MakeBlockValue(Index, 0, 0), Terrain.MakeBlockValue(Index, 0, 1) };

        public int[] m_textureSlotsByMaterial = new int[2] { 4, 1 };

        public override void Initialize() {
            Model model = ContentManager.Get<Model>("Models/PressurePlate");
            for (int i = 0; i < 2; i++) {
                Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("PressurePlate").ParentBone);
                int num = m_textureSlotsByMaterial[i];
                for (int j = 0; j < 6; j++) {
                    int num2 = SetMountingFace(SetMaterial(0, i), j);
                    Matrix matrix = j >= 4 ? j != 4 ? Matrix.CreateRotationX((float)Math.PI) * Matrix.CreateTranslation(0.5f, 1f, 0.5f) : Matrix.CreateTranslation(0.5f, 0f, 0.5f) : Matrix.CreateRotationX((float)Math.PI / 2f) * Matrix.CreateTranslation(0f, 0f, -0.5f) * Matrix.CreateRotationY(j * (float)Math.PI / 2f) * Matrix.CreateTranslation(0.5f, 0.5f, 0.5f);
                    m_blockMeshesByData[num2] = new BlockMesh();
                    m_blockMeshesByData[num2]
                    .AppendModelMeshPart(
                        model.FindMesh("PressurePlate").MeshParts[0],
                        boneAbsoluteTransform * matrix,
                        false,
                        false,
                        false,
                        false,
                        Color.White
                    );
                    m_blockMeshesByData[num2].TransformTextureCoordinates(Matrix.CreateTranslation(num % 16 / 16f, num / 16 / 16f, 0f));
                    m_blockMeshesByData[num2].GenerateSidesData();
                    Vector3 vector = Vector3.Transform(new Vector3(-0.5f, 0f, -0.5f), matrix);
                    Vector3 vector2 = Vector3.Transform(new Vector3(0.5f, 0.0625f, 0.5f), matrix);
                    vector.X = MathUtils.Round(vector.X * 100f) / 100f;
                    vector.Y = MathUtils.Round(vector.Y * 100f) / 100f;
                    vector.Z = MathUtils.Round(vector.Z * 100f) / 100f;
                    vector2.X = MathUtils.Round(vector2.X * 100f) / 100f;
                    vector2.Y = MathUtils.Round(vector2.Y * 100f) / 100f;
                    vector2.Z = MathUtils.Round(vector2.Z * 100f) / 100f;
                    m_collisionBoxesByData[num2] = new BoundingBox[1] { new BoundingBox(new Vector3(MathUtils.Min(vector.X, vector2.X), MathUtils.Min(vector.Y, vector2.Y), MathUtils.Min(vector.Z, vector2.Z)), new Vector3(MathUtils.Max(vector.X, vector2.X), MathUtils.Max(vector.Y, vector2.Y), MathUtils.Max(vector.Z, vector2.Z))) };
                }
                Matrix identity = Matrix.Identity;
                m_standaloneBlockMeshesByMaterial[i] = new BlockMesh();
                m_standaloneBlockMeshesByMaterial[i]
                .AppendModelMeshPart(
                    model.FindMesh("PressurePlate").MeshParts[0],
                    boneAbsoluteTransform * identity,
                    false,
                    false,
                    false,
                    false,
                    Color.White
                );
                m_standaloneBlockMeshesByMaterial[i].TransformTextureCoordinates(Matrix.CreateTranslation(num % 16 / 16f, num / 16 / 16f, 0f));
            }
        }

        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value) {
            int material = GetMaterial(Terrain.ExtractData(value));
            return m_displayNamesByMaterial[material];
        }

        public override IEnumerable<int> GetCreativeValues() => m_creativeValuesByMaterial;

        public override BlockDebrisParticleSystem CreateDebrisParticleSystem(SubsystemTerrain subsystemTerrain, Vector3 position, int value, float strength) {
            int material = GetMaterial(Terrain.ExtractData(value));
            return new BlockDebrisParticleSystem(
                subsystemTerrain,
                position,
                strength,
                DestructionDebrisScale,
                Color.White,
                m_textureSlotsByMaterial[material]
            );
        }

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult) {
            int data = SetMountingFace(Terrain.ExtractData(value), raycastResult.CellFace.Face);
            int value2 = Terrain.ReplaceData(value, data);
            BlockPlacementData result = default;
            result.Value = value2;
            result.CellFace = raycastResult.CellFace;
            return result;
        }

        public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris) {
            int material = GetMaterial(Terrain.ExtractData(oldValue));
            dropValues.Add(new BlockDropValue { Value = Terrain.MakeBlockValue(Index, 0, SetMaterial(0, material)), Count = 1 });
            showDebris = true;
        }

        public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value) {
            int num = Terrain.ExtractData(value);
            if (num >= m_collisionBoxesByData.Length) {
                return null;
            }
            return m_collisionBoxesByData[num];
        }

        public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value) => face != CellFace.OppositeFace(GetFace(value));

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) {
            int num = Terrain.ExtractData(value);
            if (num < m_blockMeshesByData.Length
                && m_blockMeshesByData[num] != null) {
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
                    0.8125f,
                    Vector2.Zero,
                    geometry.SubsetOpaque
                );
            }
        }

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData) {
            int material = GetMaterial(Terrain.ExtractData(value));
            BlocksManager.DrawMeshBlock(
                primitivesRenderer,
                m_standaloneBlockMeshesByMaterial[material],
                color,
                2f * size,
                ref matrix,
                environmentData
            );
        }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z) => new PressurePlateGVElectricElement(subsystemGVElectricity, new CellFace(x, y, z, GetFace(value)));

        public override GVElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z) {
            int face2 = GetFace(value);
            if (face == face2
                && SubsystemGVElectricity.GetConnectorDirection(face2, 0, connectorFace).HasValue) {
                return GVElectricConnectorType.Output;
            }
            return null;
        }

        public static int GetMaterial(int data) => data & 1;

        public static int SetMaterial(int data, int material) => (data & -2) | (material & 1);

        public static int GetMountingFace(int data) => (data >> 1) & 7;

        public static int SetMountingFace(int data, int face) => (data & -15) | ((face & 7) << 1);

        public override int GetFace(int value) => GetMountingFace(Terrain.ExtractData(value));
    }
}