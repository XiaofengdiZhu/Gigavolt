using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public abstract class GVPostedSignCBlock : GVSignCBlock, IGVElectricElementBlock, IPaintableBlock {
        public string m_modelName;

        public int m_coloredTextureSlot;

        public int m_attachedSignBlockIndex;

        public BlockMesh m_standaloneBlockMesh = new BlockMesh();

        public BlockMesh m_standaloneColoredBlockMesh = new BlockMesh();

        public BlockMesh[] m_blockMeshes = new BlockMesh[16];

        public BlockMesh[] m_coloredBlockMeshes = new BlockMesh[16];

        public BlockMesh[] m_surfaceMeshes = new BlockMesh[16];

        public Vector3[] m_surfaceNormals = new Vector3[16];

        public BoundingBox[][] m_collisionBoxes = new BoundingBox[16][];

        public Vector3[] m_directions = new Vector3[16];

        public GVPostedSignCBlock(string modelName, int coloredTextureSlot, int attachedSignBlockIndex) {
            m_modelName = modelName;
            m_coloredTextureSlot = coloredTextureSlot;
            m_attachedSignBlockIndex = attachedSignBlockIndex;
        }

        public override void Initialize() {
            Model model = ContentManager.Get<Model>(m_modelName);
            Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Sign").ParentBone);
            Matrix boneAbsoluteTransform2 = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Post").ParentBone);
            Matrix boneAbsoluteTransform3 = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Surface").ParentBone);
            for (int i = 0; i < 16; i++) {
                bool hanging = GetHanging(i);
                Matrix m = Matrix.CreateRotationY(GetDirection(i) * (float)Math.PI / 4f) * Matrix.CreateTranslation(0.5f, 0f, 0.5f);
                if (hanging) {
                    m *= Matrix.CreateScale(1f, -1f, 1f) * Matrix.CreateTranslation(0f, 1f, 0f);
                }
                m_directions[i] = m.Forward;
                BlockMesh blockMesh = new BlockMesh();
                blockMesh.AppendModelMeshPart(
                    model.FindMesh("Sign").MeshParts[0],
                    boneAbsoluteTransform * m,
                    false,
                    hanging,
                    false,
                    false,
                    Color.White
                );
                BlockMesh blockMesh2 = new BlockMesh();
                blockMesh2.AppendModelMeshPart(
                    model.FindMesh("Post").MeshParts[0],
                    boneAbsoluteTransform2 * m,
                    false,
                    hanging,
                    false,
                    false,
                    Color.White
                );
                m_blockMeshes[i] = new BlockMesh();
                m_blockMeshes[i].AppendBlockMesh(blockMesh);
                m_blockMeshes[i].AppendBlockMesh(blockMesh2);
                m_coloredBlockMeshes[i] = new BlockMesh();
                m_coloredBlockMeshes[i].AppendBlockMesh(m_blockMeshes[i]);
                m_blockMeshes[i].TransformTextureCoordinates(Matrix.CreateTranslation(DefaultTextureSlot % 16 / 16f, DefaultTextureSlot / 16 / 16f, 0f));
                m_coloredBlockMeshes[i].TransformTextureCoordinates(Matrix.CreateTranslation(m_coloredTextureSlot % 16 / 16f, m_coloredTextureSlot / 16 / 16f, 0f));
                m_collisionBoxes[i] = new BoundingBox[2];
                m_collisionBoxes[i][0] = blockMesh.CalculateBoundingBox();
                m_collisionBoxes[i][1] = blockMesh2.CalculateBoundingBox();
                m_surfaceMeshes[i] = new BlockMesh();
                m_surfaceMeshes[i]
                .AppendModelMeshPart(
                    model.FindMesh("Surface").MeshParts[0],
                    boneAbsoluteTransform3 * m,
                    false,
                    hanging,
                    false,
                    false,
                    Color.White
                );
                m_surfaceNormals[i] = -m.Forward;
                if (hanging) {
                    for (int j = 0; j < m_surfaceMeshes[i].Vertices.Count; j++) {
                        Vector2 textureCoordinates = m_surfaceMeshes[i].Vertices.Array[j].TextureCoordinates;
                        textureCoordinates.Y = 1f - textureCoordinates.Y;
                        m_surfaceMeshes[i].Vertices.Array[j].TextureCoordinates = textureCoordinates;
                    }
                }
            }
            m_standaloneBlockMesh.AppendModelMeshPart(
                model.FindMesh("Sign").MeshParts[0],
                boneAbsoluteTransform * Matrix.CreateTranslation(0f, -0.6f, 0f),
                false,
                false,
                false,
                false,
                Color.White
            );
            m_standaloneBlockMesh.AppendModelMeshPart(
                model.FindMesh("Post").MeshParts[0],
                boneAbsoluteTransform2 * Matrix.CreateTranslation(0f, -0.6f, 0f),
                false,
                false,
                false,
                false,
                Color.White
            );
            m_standaloneColoredBlockMesh.AppendBlockMesh(m_standaloneBlockMesh);
            m_standaloneBlockMesh.TransformTextureCoordinates(Matrix.CreateTranslation(DefaultTextureSlot % 16 / 16f, DefaultTextureSlot / 16 / 16f, 0f));
            m_standaloneColoredBlockMesh.TransformTextureCoordinates(Matrix.CreateTranslation(m_coloredTextureSlot % 16 / 16f, m_coloredTextureSlot / 16 / 16f, 0f));
            base.Initialize();
        }

        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value) {
            int? color = GetColor(Terrain.ExtractData(value));
            return SubsystemPalette.GetName(subsystemTerrain, color, base.GetDisplayName(subsystemTerrain, value));
        }

        public override string GetCategory(int value) {
            if (!GetColor(Terrain.ExtractData(value)).HasValue) {
                return base.GetCategory(value);
            }
            return "Painted";
        }

        public override IEnumerable<int> GetCreativeValues() {
            yield return Terrain.MakeBlockValue(BlockIndex, 0, SetColor(0, null));
            int i = 0;
            while (i < 16) {
                yield return Terrain.MakeBlockValue(BlockIndex, 0, SetColor(0, i));
                int num = i + 1;
                i = num;
            }
        }

        public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris) {
            showDebris = true;
            int? color = GetColor(Terrain.ExtractData(oldValue));
            int data = SetColor(0, color);
            dropValues.Add(new BlockDropValue { Value = Terrain.MakeBlockValue(BlockIndex, 0, data), Count = 1 });
        }

        public override BlockDebrisParticleSystem CreateDebrisParticleSystem(SubsystemTerrain subsystemTerrain, Vector3 position, int value, float strength) {
            int? color = GetColor(Terrain.ExtractData(value));
            if (color.HasValue) {
                return new BlockDebrisParticleSystem(
                    subsystemTerrain,
                    position,
                    strength,
                    DestructionDebrisScale,
                    SubsystemPalette.GetColor(subsystemTerrain, color),
                    m_coloredTextureSlot
                );
            }
            return new BlockDebrisParticleSystem(
                subsystemTerrain,
                position,
                strength,
                DestructionDebrisScale,
                Color.White,
                DefaultTextureSlot
            );
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) {
            int data = Terrain.ExtractData(value);
            int variant = GetVariant(data);
            int? color = GetColor(data);
            if (color.HasValue) {
                generator.GenerateMeshVertices(
                    this,
                    x,
                    y,
                    z,
                    m_coloredBlockMeshes[variant],
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
                    m_blockMeshes[variant],
                    Color.White,
                    null,
                    geometry.SubsetOpaque
                );
            }
            GenerateGVWireVertices(
                generator,
                value,
                x,
                y,
                z,
                GetHanging(data) ? 5 : 4,
                0.01f,
                Vector2.Zero,
                geometry.SubsetOpaque
            );
        }

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData) {
            int? color2 = GetColor(Terrain.ExtractData(value));
            if (color2.HasValue) {
                BlocksManager.DrawMeshBlock(
                    primitivesRenderer,
                    m_standaloneColoredBlockMesh,
                    color * SubsystemPalette.GetColor(environmentData, color2),
                    1.25f * size,
                    ref matrix,
                    environmentData
                );
            }
            else {
                BlocksManager.DrawMeshBlock(
                    primitivesRenderer,
                    m_standaloneBlockMesh,
                    color,
                    1.25f * size,
                    ref matrix,
                    environmentData
                );
            }
        }

        public int? GetPaintColor(int value) => GetColor(Terrain.ExtractData(value));

        public int Paint(SubsystemTerrain terrain, int value, int? color) {
            int data = Terrain.ExtractData(value);
            return Terrain.ReplaceData(value, SetColor(data, color));
        }

        public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value) {
            int variant = GetVariant(Terrain.ExtractData(value));
            return m_collisionBoxes[variant];
        }

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult) {
            int? color = GetColor(Terrain.ExtractData(value));
            BlockPlacementData result;
            if (raycastResult.CellFace.Face < 4) {
                int data = GVAttachedSignCBlock.SetFace(GVAttachedSignCBlock.SetColor(0, color), raycastResult.CellFace.Face);
                result = default;
                result.Value = Terrain.MakeBlockValue(m_attachedSignBlockIndex, 0, data);
                result.CellFace = raycastResult.CellFace;
                return result;
            }
            Vector3 forward = Matrix.CreateFromQuaternion(componentMiner.ComponentCreature.ComponentCreatureModel.EyeRotation).Forward;
            float num = float.MinValue;
            int direction = 0;
            for (int i = 0; i < 8; i++) {
                float num2 = Vector3.Dot(forward, m_directions[i]);
                if (num2 > num) {
                    num = num2;
                    direction = i;
                }
            }
            int data2 = SetHanging(SetDirection(SetColor(0, color), direction), raycastResult.CellFace.Face == 5);
            result = default;
            result.Value = Terrain.MakeBlockValue(BlockIndex, 0, data2);
            result.CellFace = raycastResult.CellFace;
            return result;
        }

        public override BlockMesh GetSignSurfaceBlockMesh(int data) => m_surfaceMeshes[GetVariant(data)];

        public override Vector3 GetSignSurfaceNormal(int data) => m_surfaceNormals[GetVariant(data)];

        public GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemElectricity, int value, int x, int y, int z) {
            int data = Terrain.ExtractData(value);
            return new SignGVCElectricElement(subsystemElectricity, new CellFace(x, y, z, GetHanging(data) ? 5 : 4));
        }

        public GVElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z) {
            if (GetHanging(Terrain.ExtractData(value))) {
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
            return GVElectricConnectorType.Input;
        }

        public int GetConnectionMask(int value) => int.MaxValue;

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
    }
}