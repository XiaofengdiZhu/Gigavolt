using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVInventoryFetcherBlock : Block, IGVElectricElementBlock {
        public const int Index = 885;

        public readonly BlockMesh[] m_blockMeshesByData = new BlockMesh[32];
        public readonly BlockMesh[] m_standaloneBlockMeshes = new BlockMesh[4];
        public readonly BoundingBox[][] m_collisionBoxesByData = new BoundingBox[32][];
        public readonly Texture2D[] m_textures = new Texture2D[3];

        public override void Initialize() {
            Model model = ContentManager.Get<Model>("Models/GVFetcher");
            for (int i = 0; i < 3; i++) {
                string name = GetIsShaft(i) ? "FetcherShaft" : "FetcherHead";
                Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh(name).ParentBone);
                for (int j = 0; j < 6; j++) {
                    int num = SetFace(SetType(0, i), j);
                    Matrix m = j < 4 ? Matrix.CreateTranslation(0f, -0.5f, 0f) * Matrix.CreateRotationY(j * (float)Math.PI / 2f) * Matrix.CreateTranslation(0.5f, 0.5f, 0.5f) :
                        j != 4 ? Matrix.CreateTranslation(0f, -0.5f, 0f) * Matrix.CreateRotationX((float)Math.PI / 2f) * Matrix.CreateTranslation(0.5f, 0.5f, 0.5f) : Matrix.CreateTranslation(0f, -0.5f, 0f) * Matrix.CreateRotationX(-(float)Math.PI / 2f) * Matrix.CreateTranslation(0.5f, 0.5f, 0.5f);
                    m_blockMeshesByData[num] = new BlockMesh();
                    m_blockMeshesByData[num]
                    .AppendModelMeshPart(
                        model.FindMesh(name).MeshParts[0],
                        boneAbsoluteTransform * m,
                        false,
                        false,
                        false,
                        false,
                        Color.White
                    );
                    m_collisionBoxesByData[num] = [m_blockMeshesByData[num].CalculateBoundingBox()];
                }
            }
            Matrix boneAbsoluteTransform2 = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("FetcherHead").ParentBone);
            m_standaloneBlockMeshes[0] = new BlockMesh();
            m_standaloneBlockMeshes[0]
            .AppendModelMeshPart(
                model.FindMesh("FetcherHead").MeshParts[0],
                boneAbsoluteTransform2 * Matrix.CreateTranslation(0f, -0.5f, 0f) * Matrix.CreateRotationY((float)Math.PI),
                false,
                false,
                false,
                false,
                Color.White
            );
            Matrix boneAbsoluteTransform3 = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("FetcherShaft").ParentBone);
            m_standaloneBlockMeshes[1] = new BlockMesh();
            m_standaloneBlockMeshes[1]
            .AppendModelMeshPart(
                model.FindMesh("FetcherShaft").MeshParts[0],
                boneAbsoluteTransform3 * Matrix.CreateTranslation(0f, -0.5f, 0f),
                false,
                false,
                false,
                false,
                Color.White
            );
            m_standaloneBlockMeshes[2] = m_standaloneBlockMeshes[0];
            m_textures[0] = ContentManager.Get<Texture2D>("Textures/GVFetcherBlock0");
            m_textures[1] = m_textures[0];
            m_textures[2] = ContentManager.Get<Texture2D>("Textures/GVFetcherBlock2");
        }

        public override IEnumerable<int> GetCreativeValues() => new[] { Terrain.MakeBlockValue(BlockIndex, 0, 0), Terrain.MakeBlockValue(BlockIndex, 0, 2), Terrain.MakeBlockValue(BlockIndex, 0, 1) };

        public override int GetShadowStrength(int value) => !GetIsShaft(Terrain.ExtractData(value)) ? base.GetShadowStrength(value) : 0;

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) {
            int num = Terrain.ExtractData(value);
            if (num < m_blockMeshesByData.Length
                && m_blockMeshesByData[num] != null) {
                generator.GenerateShadedMeshVertices(
                    this,
                    x,
                    y,
                    z,
                    m_blockMeshesByData[num],
                    Color.White,
                    null,
                    null,
                    geometry.GetGeometry(m_textures[GetType(num)]).SubsetOpaque
                );
            }
        }

        public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value) {
            int num = Terrain.ExtractData(value);
            return num < m_collisionBoxesByData.Length ? m_collisionBoxesByData[num] : base.GetCustomCollisionBoxes(terrain, value);
        }

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData) {
            int type = GetType(Terrain.ExtractData(value));
            if (type < m_standaloneBlockMeshes.Length
                && m_standaloneBlockMeshes[type] != null) {
                BlocksManager.DrawMeshBlock(
                    primitivesRenderer,
                    m_standaloneBlockMeshes[type],
                    m_textures[type],
                    color,
                    1f * size,
                    ref matrix,
                    environmentData
                );
            }
        }

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult) {
            Vector3 forward = Matrix.CreateFromQuaternion(componentMiner.ComponentCreature.ComponentCreatureModel.EyeRotation).Forward;
            float num = float.PositiveInfinity;
            int face = 0;
            for (int i = 0; i < 6; i++) {
                float num2 = Vector3.Dot(CellFace.FaceToVector3(i), forward);
                if (num2 < num) {
                    num = num2;
                    face = i;
                }
            }
            int data = Terrain.ExtractData(value);
            BlockPlacementData result = default;
            result.Value = Terrain.MakeBlockValue(BlockIndex, 0, SetFace(data, face));
            result.CellFace = raycastResult.CellFace;
            return result;
        }

        public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris) {
            int data = Terrain.ExtractData(oldValue);
            dropValues.Add(new BlockDropValue { Value = Terrain.MakeBlockValue(BlockIndex, 0, SetType(SetFace(0, 0), GetType(data))), Count = 1 });
            showDebris = true;
        }

        public override Vector3 GetIconBlockOffset(int value, DrawBlockEnvironmentData environmentData) => GetIsShaft(Terrain.ExtractData(value)) ? Vector3.Zero : new Vector3(0.2f, -0.2f, 0f);

        public static int GetType(int data) => data & 3;

        public static int SetType(int data, int type) => (data & -4) | (type & 3);

        public static bool GetIsShaft(int data) => GetType(data) == 1;

        public static int GetFace(int data) => (data >> 2) & 7;

        public static int SetFace(int data, int face) => (data & -57) | ((face & 7) << 2);
        public GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z, uint subterrainId) => GetIsShaft(Terrain.ExtractData(value)) ? null : new InventoryFetcherGVElectricElement(subsystemGVElectricity, value, new Point3(x, y, z), subterrainId);

        public GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem, int value, int face, int connectorFace, int x, int y, int z, Terrain terrain) {
            int data = Terrain.ExtractData(value);
            int type = GetType(data);
            if (type == 1) {
                return null;
            }
            int originFace = GetFace(data);
            if (originFace == face
                || CellFace.OppositeFace(originFace) == connectorFace) {
                return GVElectricConnectorType.Input;
            }
            return null;
        }

        public int GetConnectionMask(int value) => GetIsShaft(Terrain.ExtractData(value)) ? 0 : int.MaxValue;
    }
}