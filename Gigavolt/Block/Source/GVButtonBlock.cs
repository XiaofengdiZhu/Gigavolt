using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Color = Engine.Color;
using Image = Engine.Media.Image;

namespace Game {
    public class GVButtonBlock : MountedGVElectricElementBlock, IPaintableBlock {
        public const int Index = 821;

        public readonly BlockMesh m_standaloneBlockMesh_Body = new();
        public readonly BlockMesh m_standaloneBlockMesh_Top = new();
        public readonly BlockMesh[] m_blockMeshesByFace_Body = new BlockMesh[6];
        public readonly BlockMesh[] m_blockMeshesByFace_Top = new BlockMesh[6];
        public readonly BoundingBox[][] m_collisionBoxesByFace = new BoundingBox[6][];

        public static Texture2D WhiteTexture;

        public override void Initialize() {
            if (WhiteTexture == null
                || WhiteTexture.m_isDisposed) {
                WhiteTexture = Texture2D.Load(new Image(new Image<Rgba32>(Image.DefaultImageSharpConfiguration, 1, 1, SixLabors.ImageSharp.Color.White)));
            }
            Model model = ContentManager.Get<Model>("Models/GVButton");
            Matrix boneAbsoluteTransform_Body = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Button").ParentBone);
            Matrix boneAbsoluteTransform_Top = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Top").ParentBone);
            for (int i = 0; i < 6; i++) {
                Matrix matrix = i >= 4 ? i != 4 ? Matrix.CreateRotationX((float)Math.PI) * Matrix.CreateTranslation(0.5f, 1f, 0.5f) : Matrix.CreateTranslation(0.5f, 0f, 0.5f) : Matrix.CreateRotationX((float)Math.PI / 2f) * Matrix.CreateTranslation(0f, 0f, -0.5f) * Matrix.CreateRotationY(i * (float)Math.PI / 2f) * Matrix.CreateTranslation(0.5f, 0.5f, 0.5f);
                m_blockMeshesByFace_Body[i] = new BlockMesh();
                m_blockMeshesByFace_Body[i]
                .AppendModelMeshPart(
                    model.FindMesh("Button").MeshParts[0],
                    boneAbsoluteTransform_Body * matrix,
                    false,
                    false,
                    false,
                    false,
                    Color.White
                );
                m_blockMeshesByFace_Top[i] = new BlockMesh();
                m_blockMeshesByFace_Top[i]
                .AppendModelMeshPart(
                    model.FindMesh("Top").MeshParts[0],
                    boneAbsoluteTransform_Top * matrix,
                    false,
                    false,
                    false,
                    false,
                    Color.White
                );
                m_collisionBoxesByFace[i] = new[] { m_blockMeshesByFace_Body[i].CalculateBoundingBox() };
            }
            Matrix matrix2 = Matrix.CreateRotationY(-(float)Math.PI / 2f) * Matrix.CreateRotationZ((float)Math.PI / 2f);
            m_standaloneBlockMesh_Body.AppendModelMeshPart(
                model.FindMesh("Button").MeshParts[0],
                boneAbsoluteTransform_Body * matrix2,
                false,
                false,
                false,
                false,
                Color.White
            );
            m_standaloneBlockMesh_Top.AppendModelMeshPart(
                model.FindMesh("Top").MeshParts[0],
                boneAbsoluteTransform_Body * matrix2,
                false,
                false,
                false,
                false,
                Color.White
            );
        }

        public override int GetFace(int value) => Terrain.ExtractData(value) & 7;
        public static int SetFace(int value, int face) => Terrain.ReplaceData(value, (Terrain.ExtractData(value) & -8) | (face & 7));

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult) {
            BlockPlacementData result = default;
            result.Value = SetFace(value, raycastResult.CellFace.Face);
            result.CellFace = raycastResult.CellFace;
            return result;
        }

        public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris) {
            dropValues.Add(new BlockDropValue { Value = Terrain.MakeBlockValue(Index, 0, Terrain.ExtractData(SetFace(oldValue, 0))), Count = 1 });
            showDebris = true;
        }

        public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value) {
            int face = GetFace(value);
            if (face >= m_collisionBoxesByFace.Length) {
                return null;
            }
            return m_collisionBoxesByFace[face];
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) {
            int face = GetFace(value);
            if (face < m_blockMeshesByFace_Body.Length) {
                int? blockColor = GetColor(Terrain.ExtractData(value));
                generator.GenerateMeshVertices(
                    this,
                    x,
                    y,
                    z,
                    m_blockMeshesByFace_Body[face],
                    Color.White,
                    null,
                    geometry.SubsetOpaque
                );
                generator.GenerateMeshVertices(
                    this,
                    x,
                    y,
                    z,
                    m_blockMeshesByFace_Top[face],
                    blockColor.HasValue ? SubsystemPalette.GetColor(generator, blockColor) : Color.White,
                    null,
                    blockColor.HasValue ? geometry.GetGeometry(WhiteTexture).SubsetOpaque : geometry.SubsetOpaque
                );
                GVBlockGeometryGenerator.GenerateGVWireVertices(
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
            int? blockColor = GetColor(Terrain.ExtractData(value));
            environmentData = environmentData ?? BlocksManager.m_defaultEnvironmentData;
            BlocksManager.DrawMeshBlock(
                primitivesRenderer,
                m_standaloneBlockMesh_Body,
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

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z, uint subterrainId) => new ButtonGVElectricElement(
            subsystemGVElectricity,
            new GVCellFace(
                x,
                y,
                z,
                GetFace(value),
                GetConnectionMask(value)
            ),
            subterrainId
        );

        public override GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem, int value, int face, int connectorFace, int x, int y, int z, uint subterrainId) {
            int face2 = GetFace(value);
            if (face == face2
                && SubsystemGVElectricity.GetConnectorDirection(face2, 0, connectorFace).HasValue) {
                return GVElectricConnectorType.Output;
            }
            return null;
        }

        public override bool IsNonDuplicable_(int value) => ((Terrain.ExtractData(value) >> 3) & 2047) > 0;

        public override IEnumerable<int> GetCreativeValues() {
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
        public override int GetDisplayOrder(int value) => GetColor(Terrain.ExtractData(value)).HasValue ? 11 : base.GetDisplayOrder(value);

        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value) {
            int? paintColor = GetColor(Terrain.ExtractData(value));
            return SubsystemPalette.GetName(subsystemTerrain, paintColor, base.GetDisplayName(subsystemTerrain, value));
        }

        public override int GetConnectionMask(int value) {
            int? color = GetColor(Terrain.ExtractData(value));
            return color.HasValue ? 1 << color.Value : int.MaxValue;
        }

        public static int? GetColor(int data) {
            int? result = (data >> 14) & 0xF;
            switch (result.Value) {
                case 0: return null;
                case <= 7:
                    result--;
                    break;
            }
            return result;
        }

        public static int SetColor(int data, int? color) {
            if (color.HasValue) {
                if (color.Value < 7) {
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