using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Engine.Media;

namespace Game
{
    public class GVAdjustableDelayGateBlock : RotateableMountedGVElectricElementBlock, IPaintableBlock
    {
        public const int Index = 808;
        public readonly Texture2D WhiteTexture = Texture2D.Load(new Image(1, 1) { Pixels = { [0] = Color.White } });

        public GVAdjustableDelayGateBlock() : base("Models/GVAdjustableDelayGate", "Body", 0.375f) { }

        //顶部有颜色的部分
        public BlockMesh[] m_blockMeshes_Top = new BlockMesh[24];

        public BlockMesh m_standaloneBlockMesh_Top = new BlockMesh();

        public override void Initialize()
        {
            //原先获取主体模型部分保留
            base.Initialize();
            //获取顶部模型
            Model model = ContentManager.Get<Model>("Models/GVAdjustableDelayGate");
            Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Top").ParentBone);
            for (int i = 0; i < 6; i++)
            {
                float radians;
                bool flag;
                if (i < 4)
                {
                    radians = i * (float)Math.PI / 2f;
                    flag = false;
                }
                else if (i == 4)
                {
                    radians = -(float)Math.PI / 2f;
                    flag = true;
                }
                else
                {
                    radians = (float)Math.PI / 2f;
                    flag = true;
                }
                for (int j = 0; j < 4; j++)
                {
                    float radians2 = -j * (float)Math.PI / 2f;
                    int num = (i << 2) + j;
                    Matrix m = Matrix.CreateRotationX((float)Math.PI / 2f) * Matrix.CreateRotationZ(radians2) * Matrix.CreateTranslation(0f, 0f, -0.5f) * (flag ? Matrix.CreateRotationX(radians) : Matrix.CreateRotationY(radians)) * Matrix.CreateTranslation(0.5f, 0.5f, 0.5f);
                    m_blockMeshes_Top[num] = new BlockMesh();
                    m_blockMeshes_Top[num]
                    .AppendModelMeshPart(
                        model.FindMesh("Top").MeshParts[0],
                        boneAbsoluteTransform * m,
                        false,
                        false,
                        false,
                        false,
                        Color.White
                    );
                }
            }
            Matrix m2 = Matrix.CreateRotationY(-(float)Math.PI / 2f) * Matrix.CreateRotationZ((float)Math.PI / 2f);
            m_standaloneBlockMesh_Top.AppendModelMeshPart(
                model.FindMesh("Top").MeshParts[0],
                boneAbsoluteTransform * m2,
                false,
                false,
                false,
                false,
                Color.White
            );
        }

        public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
        {
            showDebris = true;
            if (toolLevel >= RequiredToolLevel)
            {
                int delay = GetDelay(Terrain.ExtractData(oldValue));
                int data = SetDelay(0, delay);
                dropValues.Add(new BlockDropValue { Value = Terrain.MakeBlockValue(Index, 0, data), Count = 1 });
            }
        }

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData)
        {
            int? blockColor = GetColor(Terrain.ExtractData(value));
            environmentData = environmentData ?? BlocksManager.m_defaultEnvironmentData;
            BlocksManager.DrawMeshBlock(
                primitivesRenderer,
                m_standaloneBlockMesh,
                environmentData.SubsystemTerrain == null ? BlocksTexturesManager.DefaultBlocksTexture : environmentData.SubsystemTerrain.SubsystemAnimatedTextures.AnimatedBlocksTexture,
                color,
                2f * size,
                ref matrix,
                environmentData
            );
            BlocksManager.DrawMeshBlock(
                primitivesRenderer,
                m_standaloneBlockMesh_Top,
                blockColor.HasValue ? WhiteTexture :
                environmentData.SubsystemTerrain == null ? BlocksTexturesManager.DefaultBlocksTexture : environmentData.SubsystemTerrain.SubsystemAnimatedTextures.AnimatedBlocksTexture,
                blockColor.HasValue ? color * SubsystemPalette.GetColor(environmentData, blockColor) : color,
                2f * size,
                ref matrix,
                environmentData
            );
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
        {
            int num = Terrain.ExtractData(value) & 0x1F;
            if (num < m_blockMeshes.Length)
            {
                int? blockColor = GetColor(Terrain.ExtractData(value));
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
                generator.GenerateMeshVertices(
                    this,
                    x,
                    y,
                    z,
                    m_blockMeshes_Top[num],
                    blockColor.HasValue ? SubsystemPalette.GetColor(generator, blockColor) : Color.White,
                    null,
                    blockColor.HasValue ? geometry.GetGeometry(WhiteTexture).SubsetOpaque : geometry.SubsetOpaque
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

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z) => new AdjustableDelayGateGVElectricElement(subsystemGVElectricity, new CellFace(x, y, z, GetFace(value)));

        public override GVElectricConnectorType? GetGVConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
        {
            int data = Terrain.ExtractData(value);
            if (GetFace(value) == face)
            {
                GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(GetFace(value), GetRotation(data), connectorFace);
                if (connectorDirection == GVElectricConnectorDirection.Bottom)
                {
                    return GVElectricConnectorType.Input;
                }
                if (connectorDirection == GVElectricConnectorDirection.Top
                    || connectorDirection == GVElectricConnectorDirection.In)
                {
                    return GVElectricConnectorType.Output;
                }
            }
            return null;
        }

        public static int GetDelay(int data) => (data >> 5) & 0xFF;

        public static int SetDelay(int data, int delay) => (data & -8161) | ((delay & 0xFF) << 5);

        public override IEnumerable<int> GetCreativeValues()
        {
            yield return Terrain.MakeBlockValue(Index);
            yield return Terrain.MakeBlockValue(Index, 0, SetColor(0, 0));
            yield return Terrain.MakeBlockValue(Index, 0, SetColor(0, 8));
            yield return Terrain.MakeBlockValue(Index, 0, SetColor(0, 15));
            yield return Terrain.MakeBlockValue(Index, 0, SetColor(0, 11));
            yield return Terrain.MakeBlockValue(Index, 0, SetColor(0, 12));
            yield return Terrain.MakeBlockValue(Index, 0, SetColor(0, 13));
            yield return Terrain.MakeBlockValue(Index, 0, SetColor(0, 14));
            yield return Terrain.MakeBlockValue(Index, 0, SetColor(0, 1));
            yield return Terrain.MakeBlockValue(Index, 0, SetColor(0, 2));
            yield return Terrain.MakeBlockValue(Index, 0, SetColor(0, 3));
            yield return Terrain.MakeBlockValue(Index, 0, SetColor(0, 4));
            yield return Terrain.MakeBlockValue(Index, 0, SetColor(0, 5));
            yield return Terrain.MakeBlockValue(Index, 0, SetColor(0, 6));
            yield return Terrain.MakeBlockValue(Index, 0, SetColor(0, 10));
        }

        public override string GetCategory(int value) => GetColor(Terrain.ExtractData(value)).HasValue ? "GV Electrics Multiple" : "GV Electrics Regular";
        public override int GetDisplayOrder(int value) => GetColor(Terrain.ExtractData(value)).HasValue ? 12 : base.GetDisplayOrder(value);

        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
        {
            int? paintColor = GetColor(Terrain.ExtractData(value));
            return paintColor.HasValue ? SubsystemPalette.GetName(subsystemTerrain, paintColor, "单向二极管") : base.GetDisplayName(subsystemTerrain, value);
        }

        public override string GetDescription(int value) => GetColor(Terrain.ExtractData(value)).HasValue ? "电压只能单向导通的二极管（有延迟），只提供彩色版本，本质上是延迟门" : base.GetDescription(value);

        public override bool IsEditable_(int value) => !GetColor(Terrain.ExtractData(value)).HasValue;

        public override int GetConnectionMask(int value)
        {
            int? color = GetColor(Terrain.ExtractData(value));
            return color.HasValue ? 1 << color.Value : int.MaxValue;
        }

        public static int? GetColor(int data)
        {
            int? result = (data >> 14) & 0xF;
            switch (result.Value)
            {
                case 0: return null;
                case <= 7:
                    result--;
                    break;
            }
            return result;
        }

        public static int SetColor(int data, int? color)
        {
            if (color.HasValue)
            {
                if (color.Value < 7)
                {
                    color++;
                }
                return (data & -245761) | ((color.Value & 0xF) << 14);
            }
            return data & -245761;
        }

        public int? GetPaintColor(int value) => GetColor(Terrain.ExtractData(value));

        public int Paint(SubsystemTerrain subsystemTerrain, int value, int? color) => Terrain.ReplaceData(value, SetColor(Terrain.ExtractData(value), color));
    }
}