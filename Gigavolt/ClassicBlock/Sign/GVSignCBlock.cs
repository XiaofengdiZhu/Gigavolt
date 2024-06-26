using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVSignCBlock : GVBaseSignBlock, IGVElectricElementBlock, IPaintableBlock {
        public const int Index = 837;

        public readonly string[] m_modelName = ["Models/WoodenSign", "Models/IronSign"];
        public readonly int[] m_defaultTextureSlot = [4, 63];
        public readonly int[] m_coloredTextureSlot = [23, 63];
        public readonly BlockMesh[] m_standaloneBlockMesh = new BlockMesh[4];
        public readonly BlockMesh[] m_standaloneColoredBlockMesh = new BlockMesh[4];
        public readonly BlockMesh[][] m_blockMeshes = new BlockMesh[4][];
        public readonly BlockMesh[][] m_coloredBlockMeshes = new BlockMesh[4][];
        public readonly BlockMesh[][] m_surfaceMeshes = new BlockMesh[4][];
        public readonly Vector3[][] m_surfaceNormals = new Vector3[4][];
        public readonly BoundingBox[][][] m_collisionBoxes = new BoundingBox[4][][];
        public readonly Vector3[][] m_directions = new Vector3[4][];

        public override void Initialize() {
            int poseIndex = 0;
            for (int materialIndex = 0; materialIndex < 2; materialIndex++) {
                int baseIndex = (poseIndex << 1) + materialIndex;
                BlockMesh[] blockMeshes = new BlockMesh[4];
                m_blockMeshes[baseIndex] = blockMeshes;
                BlockMesh[] coloredBlockMeshes = new BlockMesh[4];
                m_coloredBlockMeshes[baseIndex] = coloredBlockMeshes;
                BoundingBox[][] collisionBoxes = new BoundingBox[4][];
                m_collisionBoxes[baseIndex] = collisionBoxes;
                BlockMesh[] surfaceMeshes = new BlockMesh[4];
                m_surfaceMeshes[baseIndex] = surfaceMeshes;
                Vector3[] surfaceNormals = new Vector3[4];
                m_surfaceNormals[baseIndex] = surfaceNormals;
                int defaultTextureSlot = m_defaultTextureSlot[materialIndex];
                int coloredTextureSlot = m_coloredTextureSlot[materialIndex];
                Model model = ContentManager.Get<Model>(m_modelName[materialIndex]);
                Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Sign").ParentBone);
                Matrix boneAbsoluteTransform2 = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Surface").ParentBone);
                for (int i = 0; i < 4; i++) {
                    float radians = (float)Math.PI / 2f * i;
                    Matrix m = Matrix.CreateTranslation(0f, 0f, -15f / 32f) * Matrix.CreateRotationY(radians) * Matrix.CreateTranslation(0.5f, -0.3125f, 0.5f);
                    BlockMesh blockMesh1 = new();
                    blockMesh1.AppendModelMeshPart(
                        model.FindMesh("Sign").MeshParts[0],
                        boneAbsoluteTransform * m,
                        false,
                        false,
                        false,
                        false,
                        Color.White
                    );
                    BlockMesh blockMesh = new();
                    blockMeshes[i] = blockMesh;
                    blockMesh.AppendBlockMesh(blockMesh1);
                    BlockMesh coloredBlockMesh = new();
                    coloredBlockMeshes[i] = coloredBlockMesh;
                    coloredBlockMesh.AppendBlockMesh(blockMesh);
                    blockMesh.TransformTextureCoordinates(Matrix.CreateTranslation(defaultTextureSlot % 16 / 16f, defaultTextureSlot / 16 / 16f, 0f));
                    coloredBlockMesh.TransformTextureCoordinates(Matrix.CreateTranslation(coloredTextureSlot % 16 / 16f, coloredTextureSlot / 16 / 16f, 0f));
                    collisionBoxes[i] = [blockMesh1.CalculateBoundingBox()];
                    BlockMesh surfaceMesh = new();
                    surfaceMeshes[i] = surfaceMesh;
                    surfaceMesh.AppendModelMeshPart(
                        model.FindMesh("Surface").MeshParts[0],
                        boneAbsoluteTransform2 * m,
                        false,
                        false,
                        false,
                        false,
                        Color.White
                    );
                    surfaceNormals[i] = -m.Forward;
                }
                BlockMesh standaloneBlockMesh = new();
                m_standaloneBlockMesh[baseIndex] = standaloneBlockMesh;
                standaloneBlockMesh.AppendModelMeshPart(
                    model.FindMesh("Sign").MeshParts[0],
                    boneAbsoluteTransform * Matrix.CreateTranslation(0f, -0.6f, 0f),
                    false,
                    false,
                    false,
                    false,
                    Color.White
                );
                BlockMesh standaloneColoredBlockMesh = new();
                m_standaloneColoredBlockMesh[baseIndex] = standaloneColoredBlockMesh;
                standaloneColoredBlockMesh.AppendBlockMesh(standaloneBlockMesh);
                standaloneBlockMesh.TransformTextureCoordinates(Matrix.CreateTranslation(defaultTextureSlot % 16 / 16f, defaultTextureSlot / 16 / 16f, 0f));
                standaloneColoredBlockMesh.TransformTextureCoordinates(Matrix.CreateTranslation(coloredTextureSlot % 16 / 16f, coloredTextureSlot / 16 / 16f, 0f));
            }
            poseIndex = 1;
            for (int materialIndex = 0; materialIndex < 2; materialIndex++) {
                int baseIndex = (poseIndex << 1) + materialIndex;
                Vector3[] directions = new Vector3[16];
                m_directions[baseIndex] = directions;
                BlockMesh[] blockMeshes = new BlockMesh[16];
                m_blockMeshes[baseIndex] = blockMeshes;
                BlockMesh[] coloredBlockMeshes = new BlockMesh[16];
                m_coloredBlockMeshes[baseIndex] = coloredBlockMeshes;
                BoundingBox[][] collisionBoxes = new BoundingBox[16][];
                m_collisionBoxes[baseIndex] = collisionBoxes;
                BlockMesh[] surfaceMeshes = new BlockMesh[16];
                m_surfaceMeshes[baseIndex] = surfaceMeshes;
                Vector3[] surfaceNormals = new Vector3[16];
                m_surfaceNormals[baseIndex] = surfaceNormals;
                int defaultTextureSlot = m_defaultTextureSlot[materialIndex];
                int coloredTextureSlot = m_coloredTextureSlot[materialIndex];
                Model model = ContentManager.Get<Model>(m_modelName[materialIndex]);
                Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Sign").ParentBone);
                Matrix boneAbsoluteTransform2 = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Post").ParentBone);
                Matrix boneAbsoluteTransform3 = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Surface").ParentBone);
                for (int i = 0; i < 16; i++) {
                    bool hanging = GetHanging(i);
                    Matrix m = Matrix.CreateRotationY(GetDirection(i) * (float)Math.PI / 4f) * Matrix.CreateTranslation(0.5f, 0f, 0.5f);
                    if (hanging) {
                        m *= Matrix.CreateScale(1f, -1f, 1f) * Matrix.CreateTranslation(0f, 1f, 0f);
                    }
                    directions[i] = m.Forward;
                    BlockMesh blockMesh1 = new();
                    blockMesh1.AppendModelMeshPart(
                        model.FindMesh("Sign").MeshParts[0],
                        boneAbsoluteTransform * m,
                        false,
                        hanging,
                        false,
                        false,
                        Color.White
                    );
                    BlockMesh blockMesh2 = new();
                    blockMesh2.AppendModelMeshPart(
                        model.FindMesh("Post").MeshParts[0],
                        boneAbsoluteTransform2 * m,
                        false,
                        hanging,
                        false,
                        false,
                        Color.White
                    );
                    BlockMesh blockMesh3 = new();
                    blockMeshes[i] = blockMesh3;
                    blockMesh3.AppendBlockMesh(blockMesh1);
                    blockMesh3.AppendBlockMesh(blockMesh2);
                    BlockMesh blockMesh4 = new();
                    coloredBlockMeshes[i] = blockMesh4;
                    blockMesh4.AppendBlockMesh(blockMesh3);
                    blockMesh3.TransformTextureCoordinates(Matrix.CreateTranslation(defaultTextureSlot % 16 / 16f, defaultTextureSlot / 16 / 16f, 0f));
                    blockMesh4.TransformTextureCoordinates(Matrix.CreateTranslation(coloredTextureSlot % 16 / 16f, coloredTextureSlot / 16 / 16f, 0f));
                    BoundingBox[] collisionBox = new BoundingBox[2];
                    collisionBoxes[i] = collisionBox;
                    collisionBox[0] = blockMesh1.CalculateBoundingBox();
                    collisionBox[1] = blockMesh2.CalculateBoundingBox();
                    BlockMesh surfaceMesh = new();
                    surfaceMeshes[i] = surfaceMesh;
                    surfaceMesh.AppendModelMeshPart(
                        model.FindMesh("Surface").MeshParts[0],
                        boneAbsoluteTransform3 * m,
                        false,
                        hanging,
                        false,
                        false,
                        Color.White
                    );
                    surfaceNormals[i] = -m.Forward;
                    if (hanging) {
                        for (int j = 0; j < surfaceMesh.Vertices.Count; j++) {
                            Vector2 textureCoordinates = surfaceMesh.Vertices.Array[j].TextureCoordinates;
                            textureCoordinates.Y = 1f - textureCoordinates.Y;
                            surfaceMesh.Vertices.Array[j].TextureCoordinates = textureCoordinates;
                        }
                    }
                }
                BlockMesh standaloneBlockMesh = new();
                m_standaloneBlockMesh[baseIndex] = standaloneBlockMesh;
                standaloneBlockMesh.AppendModelMeshPart(
                    model.FindMesh("Sign").MeshParts[0],
                    boneAbsoluteTransform * Matrix.CreateTranslation(0f, -0.6f, 0f),
                    false,
                    false,
                    false,
                    false,
                    Color.White
                );
                standaloneBlockMesh.AppendModelMeshPart(
                    model.FindMesh("Post").MeshParts[0],
                    boneAbsoluteTransform2 * Matrix.CreateTranslation(0f, -0.6f, 0f),
                    false,
                    false,
                    false,
                    false,
                    Color.White
                );
                BlockMesh standaloneColoredBlockMesh = new();
                m_standaloneColoredBlockMesh[baseIndex] = standaloneColoredBlockMesh;
                standaloneColoredBlockMesh.AppendBlockMesh(standaloneBlockMesh);
                standaloneBlockMesh.TransformTextureCoordinates(Matrix.CreateTranslation(defaultTextureSlot % 16 / 16f, defaultTextureSlot / 16 / 16f, 0f));
                standaloneColoredBlockMesh.TransformTextureCoordinates(Matrix.CreateTranslation(coloredTextureSlot % 16 / 16f, coloredTextureSlot / 16 / 16f, 0f));
            }
            base.Initialize();
        }

        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value) {
            int data = Terrain.ExtractData(value);
            return SubsystemPalette.GetName(subsystemTerrain, GetColor(data), LanguageControl.Get(GetType().Name, GetMaterial(data) == 1 ? "IronDisplayName" : "WoodDisplayName"));
        }

        public override string GetCategory(int value) => GetColor(Terrain.ExtractData(value)).HasValue ? "Painted" : base.GetCategory(value);

        public override IEnumerable<int> GetCreativeValues() {
            int data = SetPose(0, 1);
            for (int material = 0; material < 2; material++) {
                data = SetMaterial(data, material);
                yield return Terrain.MakeBlockValue(Index, 0, SetColor(data, null));
                int i = 0;
                while (i < 16) {
                    yield return Terrain.MakeBlockValue(Index, 0, SetColor(data, i));
                    int num = i + 1;
                    i = num;
                }
            }
        }

        public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris) {
            showDebris = true;
            int data = Terrain.ExtractData(oldValue);
            dropValues.Add(new BlockDropValue { Value = Terrain.MakeBlockValue(Index, 0, SetMaterial(SetColor(SetPose(0, 1), GetColor(data)), GetMaterial(data))), Count = 1 });
        }

        public override BlockDebrisParticleSystem CreateDebrisParticleSystem(SubsystemTerrain subsystemTerrain, Vector3 position, int value, float strength) {
            int data = Terrain.ExtractData(value);
            int? color = GetColor(data);
            if (color.HasValue) {
                return new BlockDebrisParticleSystem(
                    subsystemTerrain,
                    position,
                    strength,
                    DestructionDebrisScale,
                    SubsystemPalette.GetColor(subsystemTerrain, color),
                    m_coloredTextureSlot[GetBaseIndex(data)]
                );
            }
            return new BlockDebrisParticleSystem(
                subsystemTerrain,
                position,
                strength,
                DestructionDebrisScale,
                Color.White,
                m_defaultTextureSlot[GetMaterial(data)]
            );
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) {
            int data = Terrain.ExtractData(value);
            int baseIndex = GetBaseIndex(data);
            bool isPosted = GetPose(data) == 1;
            int secondIndex = isPosted ? GetVariant(data) : GetFace(data);
            int? color = GetColor(data);
            if (color.HasValue) {
                generator.GenerateMeshVertices(
                    this,
                    x,
                    y,
                    z,
                    m_coloredBlockMeshes[baseIndex][secondIndex],
                    SubsystemPalette.GetColor(generator, color),
                    null,
                    geometry.SubsetOpaque
                );
            }
            else {
                generator.GenerateMeshVertices(
                    this,
                    x,
                    y,
                    z,
                    m_blockMeshes[baseIndex][secondIndex],
                    Color.White,
                    null,
                    geometry.SubsetOpaque
                );
            }
            GVBlockGeometryGenerator.GenerateGVWireVertices(
                generator,
                value,
                x,
                y,
                z,
                isPosted ? GetHanging(data) ? 5 : 4 : GetFace(data),
                0.01f,
                Vector2.Zero,
                geometry.SubsetOpaque
            );
        }

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData) {
            int data = Terrain.ExtractData(value);
            int? color2 = GetColor(data);
            if (color2.HasValue) {
                BlocksManager.DrawMeshBlock(
                    primitivesRenderer,
                    m_standaloneColoredBlockMesh[GetBaseIndex(data)],
                    color * SubsystemPalette.GetColor(environmentData, color2),
                    1.25f * size,
                    ref matrix,
                    environmentData
                );
            }
            else {
                BlocksManager.DrawMeshBlock(
                    primitivesRenderer,
                    m_standaloneBlockMesh[GetBaseIndex(data)],
                    color,
                    1.25f * size,
                    ref matrix,
                    environmentData
                );
            }
        }

        public int? GetPaintColor(int value) => GetColor(Terrain.ExtractData(value));

        public int Paint(SubsystemTerrain terrain, int value, int? color) => Terrain.ReplaceData(value, SetColor(Terrain.ExtractData(value), color));

        public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value) {
            int data = Terrain.ExtractData(value);
            return m_collisionBoxes[GetBaseIndex(data)][GetPose(data) == 1 ? GetVariant(data) : GetFace(data)];
        }

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult) {
            int data = Terrain.ExtractData(value);
            int? color = GetColor(data);
            int baseIndex = GetBaseIndex(data);
            BlockPlacementData result;
            if (raycastResult.CellFace.Face < 4) {
                result = default;
                result.Value = Terrain.MakeBlockValue(Index, 0, SetFace(SetMaterial(SetColor(0, color), GetMaterial(data)), raycastResult.CellFace.Face));
                result.CellFace = raycastResult.CellFace;
                return result;
            }
            Vector3 forward = Matrix.CreateFromQuaternion(componentMiner.ComponentCreature.ComponentCreatureModel.EyeRotation).Forward;
            float num = float.MinValue;
            int direction = 0;
            for (int i = 0; i < 8; i++) {
                float num2 = Vector3.Dot(forward, m_directions[baseIndex][i]);
                if (num2 > num) {
                    num = num2;
                    direction = i;
                }
            }
            result = default;
            result.Value = Terrain.MakeBlockValue(Index, 0, SetHanging(SetDirection(SetMaterial(SetColor(SetPose(data, 1), color), GetMaterial(data)), direction), raycastResult.CellFace.Face == 5));
            result.CellFace = raycastResult.CellFace;
            return result;
        }

        public override BlockMesh GetSignSurfaceBlockMesh(int data) => m_surfaceMeshes[GetBaseIndex(data)][GetPose(data) == 1 ? GetVariant(data) : GetFace(data)];

        public override Vector3 GetSignSurfaceNormal(int data) => m_surfaceNormals[GetBaseIndex(data)][GetPose(data) == 1 ? GetVariant(data) : GetFace(data)];

        public GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z, uint subterrainId) {
            int data = Terrain.ExtractData(value);
            return new SignGVCElectricElement(subsystemGVElectricity, new GVCellFace(x, y, z, GetPose(data) == 1 ? GetHanging(data) ? 5 : 4 : GetFace(data)), subterrainId);
        }

        public GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem, int value, int face, int connectorFace, int x, int y, int z, uint subterrainId) {
            int data = Terrain.ExtractData(value);
            if (GetPose(data) == 1) {
                if (GetHanging(data)) {
                    if (face != 5
                        || !SubsystemElectricity.GetConnectorDirection(face, 0, connectorFace).HasValue) {
                        return null;
                    }
                    return GVElectricConnectorType.Input;
                }
                if (face != 4
                    || !SubsystemElectricity.GetConnectorDirection(face, 0, connectorFace).HasValue) {
                    return null;
                }
            }
            else {
                if (face != GetFace(data)
                    || !SubsystemElectricity.GetConnectorDirection(face, 0, connectorFace).HasValue) {
                    return null;
                }
            }
            return GVElectricConnectorType.Input;
        }

        public int GetConnectionMask(int value) => int.MaxValue;

        public static int GetFace(int data) => data & 3;

        public static int SetFace(int data, int face) => (data & -4) | (face & 3);
        public static int GetDirection(int data) => data & 7;

        public static int SetDirection(int data, int direction) => (data & -8) | (direction & 7);

        public static bool GetHanging(int data) => (data & 8) != 0;

        public static int SetHanging(int data, bool hanging) {
            if (!hanging) {
                return data & -9;
            }
            return data | 8;
        }

        public static int GetVariant(int data) => data & 0xF;

        public static int SetVariant(int data, int variant) => (data & -16) | (variant & 0xF);

        public static int? GetColor(int data) {
            if ((data & 0x10) != 0) {
                return (data >> 5) & 0xF;
            }
            return null;
        }

        public static int SetColor(int data, int? color) {
            if (color.HasValue) {
                return (data & -497) | 0x10 | ((color.Value & 0xF) << 5);
            }
            return data & -497;
        }

        public static int GetMaterial(int data) => (data >> 9) & 1;
        public static int SetMaterial(int data, int material) => (data & -513) | ((material & 1) << 9);
        public static int GetPose(int data) => (data >> 10) & 1;
        public static int SetPose(int data, int pose) => (data & -1025) | ((pose & 1) << 10);
        public static int GetBaseIndex(int data) => (data >> 9) & 3;
        public override int GetShadowStrength(int value) => GetPose(Terrain.ExtractData(value)) == 1 ? 3 : 6;
        public override float GetDensity(int value) => GetMaterial(Terrain.ExtractData(value)) == 1 ? 3f : 0.5f;
        public override string GetSoundMaterialName(SubsystemTerrain subsystemTerrain, int value) => GetMaterial(Terrain.ExtractData(value)) == 1 ? "Metal" : "Wood";
        public override float GetFireDuration(int value) => GetMaterial(Terrain.ExtractData(value)) == 1 ? 0f : 15f;
        public override float GetExplosionResilience(int value) => GetMaterial(Terrain.ExtractData(value)) == 1 ? 20f : 3f;
        public override BlockDigMethod GetBlockDigMethod(int value) => GetMaterial(Terrain.ExtractData(value)) == 1 ? BlockDigMethod.Quarry : BlockDigMethod.Hack;
        public override int GetFaceTextureSlot(int face, int value) => GetMaterial(Terrain.ExtractData(value)) == 1 ? 63 : 4;
    }
}