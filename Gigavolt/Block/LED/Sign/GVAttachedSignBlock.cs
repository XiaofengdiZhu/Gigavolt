using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;

// ReSharper disable PossibleLossOfFraction

namespace Game {
    public abstract class GVAttachedSignBlock<T> : GVBaseSignBlock, IGVElectricElementBlock, IPaintableBlock {
        public readonly string m_modelName;

        public readonly int m_coloredTextureSlot;

        public readonly BlockMesh m_standaloneBlockMesh = new();

        public readonly BlockMesh m_standaloneColoredBlockMesh = new();

        public readonly BlockMesh[] m_blockMeshes = new BlockMesh[4];

        public readonly BlockMesh[] m_coloredBlockMeshes = new BlockMesh[4];

        public readonly BlockMesh[] m_surfaceMeshes = new BlockMesh[4];

        public readonly Vector3[] m_surfaceNormals = new Vector3[4];

        public readonly BoundingBox[][] m_collisionBoxes = new BoundingBox[4][];

        public GVAttachedSignBlock(string modelName, int coloredTextureSlot) {
            m_modelName = modelName;
            m_coloredTextureSlot = coloredTextureSlot;
        }

        public override void Initialize() {
            Model model = ContentManager.Get<Model>(m_modelName);
            Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Sign").ParentBone);
            Matrix boneAbsoluteTransform2 = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Surface").ParentBone);
            for (int i = 0; i < 4; i++) {
                float radians = (float)Math.PI / 2f * i;
                Matrix m = Matrix.CreateTranslation(0f, 0f, -15f / 32f) * Matrix.CreateRotationY(radians) * Matrix.CreateTranslation(0.5f, -0.3125f, 0.5f);
                BlockMesh blockMesh = new();
                blockMesh.AppendModelMeshPart(
                    model.FindMesh("Sign").MeshParts[0],
                    boneAbsoluteTransform * m,
                    false,
                    false,
                    false,
                    false,
                    Color.White
                );
                m_blockMeshes[i] = new BlockMesh();
                m_blockMeshes[i].AppendBlockMesh(blockMesh);
                m_coloredBlockMeshes[i] = new BlockMesh();
                m_coloredBlockMeshes[i].AppendBlockMesh(m_blockMeshes[i]);
                m_blockMeshes[i].TransformTextureCoordinates(Matrix.CreateTranslation(DefaultTextureSlot % 16 / 16f, DefaultTextureSlot / 16 / 16f, 0f));
                m_coloredBlockMeshes[i].TransformTextureCoordinates(Matrix.CreateTranslation(m_coloredTextureSlot % 16 / 16f, m_coloredTextureSlot / 16 / 16f, 0f));
                m_collisionBoxes[i] = new BoundingBox[1];
                m_collisionBoxes[i][0] = blockMesh.CalculateBoundingBox();
                m_surfaceMeshes[i] = new BlockMesh();
                m_surfaceMeshes[i]
                .AppendModelMeshPart(
                    model.FindMesh("Surface").MeshParts[0],
                    boneAbsoluteTransform2 * m,
                    false,
                    false,
                    false,
                    false,
                    Color.White
                );
                m_surfaceNormals[i] = -m.Forward;
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
            m_standaloneColoredBlockMesh.AppendBlockMesh(m_standaloneBlockMesh);
            m_standaloneBlockMesh.TransformTextureCoordinates(Matrix.CreateTranslation(DefaultTextureSlot % 16 / 16f, DefaultTextureSlot / 16 / 16f, 0f));
            m_standaloneColoredBlockMesh.TransformTextureCoordinates(Matrix.CreateTranslation(m_coloredTextureSlot % 16 / 16f, m_coloredTextureSlot / 16 / 16f, 0f));
            base.Initialize();
        }

        public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris) {
            showDebris = true;
            int? color = GetColor(Terrain.ExtractData(oldValue));
            int data = SetColor(0, color);
            dropValues.Add(new BlockDropValue { Value = Terrain.MakeBlockValue(GVBlocksManager.GetBlockIndex<T>(), 0, data), Count = 1 });
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
            int face = GetFace(data);
            int? color = GetColor(data);
            if (color.HasValue) {
                generator.GenerateMeshVertices(
                    this,
                    x,
                    y,
                    z,
                    m_coloredBlockMeshes[face],
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
                    m_blockMeshes[face],
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
                GetFace(data),
                0.375f,
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
            int face = GetFace(Terrain.ExtractData(value));
            return m_collisionBoxes[face];
        }

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult) => default;

        public override BlockMesh GetSignSurfaceBlockMesh(int data) => m_surfaceMeshes[GetFace(data)];

        public override Vector3 GetSignSurfaceNormal(int data) => m_surfaceNormals[GetFace(data)];

        public virtual GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z, uint subterrainId) {
            int data = Terrain.ExtractData(value);
            return new SignGVCElectricElement(subsystemGVElectricity, new GVCellFace(x, y, z, GetFace(data)), subterrainId);
        }

        public GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem, int value, int face, int connectorFace, int x, int y, int z, Terrain terrain) {
            int data = Terrain.ExtractData(value);
            if (face != GetFace(data)
                || !SubsystemElectricity.GetConnectorDirection(face, 0, connectorFace).HasValue) {
                return null;
            }
            return GVElectricConnectorType.Input;
        }

        public int GetConnectionMask(int value) => int.MaxValue;

        public static int GetFace(int data) => data & 3;

        public static int SetFace(int data, int face) => (data & -4) | (face & 3);

        public static int? GetColor(int data) {
            if ((data & 4) != 0) {
                return (data >> 3) & 0xF;
            }
            return null;
        }

        public static int SetColor(int data, int? color) {
            if (color.HasValue) {
                return (data & -125) | 4 | ((color.Value & 0xF) << 3);
            }
            return data & -125;
        }
    }
}