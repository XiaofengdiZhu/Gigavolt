using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVLightbulbBlock : MountedGVElectricElementBlock, IPaintableBlock {
        public const int Index = 819;

        public readonly BlockMesh m_standaloneBulbBlockMesh = new();

        public readonly BlockMesh m_standaloneSidesBlockMesh = new();

        public readonly BlockMesh[] m_bulbBlockMeshes = new BlockMesh[6];

        public readonly BlockMesh[] m_bulbBlockMeshesLit = new BlockMesh[6];

        public readonly BlockMesh[] m_sidesBlockMeshes = new BlockMesh[6];

        public readonly BoundingBox[][] m_collisionBoxes = new BoundingBox[6][];
        public new static readonly string fName = "GVLightbulbBlock";
        public Color m_copperColor = new(118, 56, 32);

        public override void Initialize() {
            Model model = ContentManager.Get<Model>("Models/Lightbulbs");
            Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Top").ParentBone);
            Matrix boneAbsoluteTransform2 = BlockMesh.GetBoneAbsoluteTransform(model.FindMesh("Sides").ParentBone);
            for (int i = 0; i < 6; i++) {
                Matrix m = i >= 4
                    ? i != 4
                        ? Matrix.CreateRotationX((float)Math.PI) * Matrix.CreateTranslation(0.5f, 1f, 0.5f)
                        : Matrix.CreateTranslation(0.5f, 0f, 0.5f)
                    : Matrix.CreateRotationX((float)Math.PI / 2f)
                    * Matrix.CreateTranslation(0f, 0f, -0.5f)
                    * Matrix.CreateRotationY(i * (float)Math.PI / 2f)
                    * Matrix.CreateTranslation(0.5f, 0.5f, 0.5f);
                m_bulbBlockMeshes[i] = new BlockMesh();
                m_bulbBlockMeshes[i]
                .AppendModelMeshPart(
                    model.FindMesh("Top").MeshParts[0],
                    boneAbsoluteTransform * m,
                    false,
                    false,
                    false,
                    false,
                    Color.White
                );
                m_bulbBlockMeshes[i].TransformTextureCoordinates(Matrix.CreateTranslation(0.1875f, 0.25f, 0f));
                m_bulbBlockMeshesLit[i] = new BlockMesh();
                m_bulbBlockMeshesLit[i]
                .AppendModelMeshPart(
                    model.FindMesh("Top").MeshParts[0],
                    boneAbsoluteTransform * m,
                    true,
                    false,
                    false,
                    false,
                    new Color(255, 255, 230)
                );
                m_bulbBlockMeshesLit[i].TransformTextureCoordinates(Matrix.CreateTranslation(0.9375f, 0f, 0f));
                m_sidesBlockMeshes[i] = new BlockMesh();
                m_sidesBlockMeshes[i]
                .AppendModelMeshPart(
                    model.FindMesh("Sides").MeshParts[0],
                    boneAbsoluteTransform2 * m,
                    false,
                    false,
                    true,
                    false,
                    Color.White
                );
                m_sidesBlockMeshes[i].TransformTextureCoordinates(Matrix.CreateTranslation(0.9375f, 0.1875f, 0f));
                m_collisionBoxes[i] = [m_sidesBlockMeshes[i].CalculateBoundingBox()];
            }
            Matrix m2 = Matrix.CreateRotationY(-(float)Math.PI / 2f) * Matrix.CreateRotationZ((float)Math.PI / 2f);
            m_standaloneBulbBlockMesh.AppendModelMeshPart(
                model.FindMesh("Top").MeshParts[0],
                boneAbsoluteTransform * m2,
                false,
                false,
                true,
                false,
                Color.White
            );
            m_standaloneBulbBlockMesh.TransformTextureCoordinates(Matrix.CreateTranslation(0.1875f, 0.25f, 0f));
            m_standaloneSidesBlockMesh.AppendModelMeshPart(
                model.FindMesh("Sides").MeshParts[0],
                boneAbsoluteTransform2 * m2,
                false,
                false,
                true,
                false,
                Color.White
            );
            m_standaloneSidesBlockMesh.TransformTextureCoordinates(Matrix.CreateTranslation(0.9375f, 0.1875f, 0f));
        }

        public override IEnumerable<int> GetCreativeValues() {
            yield return Terrain.MakeBlockValue(BlockIndex);
            int i = 0;
            while (i < 16) {
                yield return Terrain.MakeBlockValue(BlockIndex, 0, SetColor(0, i));
                int num = i + 1;
                i = num;
            }
        }

        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value) {
            int? color = GetColor(Terrain.ExtractData(value));
            return SubsystemPalette.GetName(subsystemTerrain, color, LanguageControl.Get(fName, 1));
        }

        public override string GetCategory(int value) {
            if (!GetColor(Terrain.ExtractData(value)).HasValue) {
                return base.GetCategory(value);
            }
            return "Painted";
        }

        public override int GetFace(int value) => GetMountingFace(Terrain.ExtractData(value));

        public override int GetEmittedLightAmount(int value) => GetLightIntensity(Terrain.ExtractData(value));

        public override int GetShadowStrength(int value) {
            int lightIntensity = GetLightIntensity(Terrain.ExtractData(value));
            return DefaultShadowStrength - 10 * lightIntensity;
        }

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain,
            ComponentMiner componentMiner,
            int value,
            TerrainRaycastResult raycastResult) {
            BlockPlacementData result = default;
            result.Value = Terrain.MakeBlockValue(BlockIndex, 0, SetMountingFace(Terrain.ExtractData(value), raycastResult.CellFace.Face));
            result.CellFace = raycastResult.CellFace;
            return result;
        }

        public override void GetDropValues(SubsystemTerrain subsystemTerrain,
            int oldValue,
            int newValue,
            int toolLevel,
            List<BlockDropValue> dropValues,
            out bool showDebris) {
            int? color = GetColor(Terrain.ExtractData(oldValue));
            dropValues.Add(new BlockDropValue { Value = Terrain.MakeBlockValue(BlockIndex, 0, SetColor(0, color)), Count = 1 });
            showDebris = true;
        }

        public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value) {
            int mountingFace = GetMountingFace(Terrain.ExtractData(value));
            if (mountingFace >= m_collisionBoxes.Length) {
                return null;
            }
            return m_collisionBoxes[mountingFace];
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) {
            int data = Terrain.ExtractData(value);
            int mountingFace = GetMountingFace(data);
            int lightIntensity = GetLightIntensity(data);
            int? color = GetColor(data);
            Color color2 = color.HasValue ? SubsystemPalette.GetColor(generator, color) : m_copperColor;
            if (mountingFace < m_bulbBlockMeshes.Length) {
                if (lightIntensity <= 0) {
                    generator.GenerateMeshVertices(
                        this,
                        x,
                        y,
                        z,
                        m_bulbBlockMeshes[mountingFace],
                        Color.White,
                        null,
                        geometry.SubsetAlphaTest
                    );
                }
                else {
                    byte r = (byte)(195 + lightIntensity * 4);
                    byte g = (byte)(180 + lightIntensity * 5);
                    byte b = (byte)(165 + lightIntensity * 6);
                    generator.GenerateMeshVertices(
                        this,
                        x,
                        y,
                        z,
                        m_bulbBlockMeshesLit[mountingFace],
                        new Color(r, g, b),
                        null,
                        geometry.SubsetOpaque
                    );
                }
                generator.GenerateMeshVertices(
                    this,
                    x,
                    y,
                    z,
                    m_sidesBlockMeshes[mountingFace],
                    color2,
                    null,
                    geometry.SubsetOpaque
                );
                GVBlockGeometryGenerator.GenerateGVWireVertices(
                    generator,
                    value,
                    x,
                    y,
                    z,
                    mountingFace,
                    0.875f,
                    Vector2.Zero,
                    geometry.SubsetOpaque
                );
            }
        }

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer,
            int value,
            Color color,
            float size,
            ref Matrix matrix,
            DrawBlockEnvironmentData environmentData) {
            int? color2 = GetColor(Terrain.ExtractData(value));
            Color c = color2.HasValue ? SubsystemPalette.GetColor(environmentData, color2) : m_copperColor;
            BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneSidesBlockMesh, color * c, 2f * size, ref matrix, environmentData);
            BlocksManager.DrawMeshBlock(primitivesRenderer, m_standaloneBulbBlockMesh, color, 2f * size, ref matrix, environmentData);
        }

        public int? GetPaintColor(int value) => GetColor(Terrain.ExtractData(value));

        public int Paint(SubsystemTerrain subsystemTerrain, int value, int? color) {
            int data = Terrain.ExtractData(value);
            return Terrain.ReplaceData(value, SetColor(data, color));
        }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity,
            int value,
            int x,
            int y,
            int z,
            uint subterrainId) =>
            new LightBulbGVElectricElement(subsystemGVElectricity, new GVCellFace(x, y, z, GetFace(value)), value, subterrainId);

        public override GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem,
            int value,
            int face,
            int connectorFace,
            int x,
            int y,
            int z,
            Terrain terrain) {
            int face2 = GetFace(value);
            if (face == face2
                && SubsystemGVElectricity.GetConnectorDirection(face2, 0, connectorFace).HasValue) {
                return GVElectricConnectorType.Input;
            }
            return null;
        }

        public static int GetMountingFace(int data) => data & 7;

        public static int SetMountingFace(int data, int face) => (data & -8) | (face & 7);

        public static int GetLightIntensity(int data) => (data >> 3) & 0xF;

        public static int SetLightIntensity(int data, int intensity) => (data & -121) | ((intensity & 0xF) << 3);

        public static int? GetColor(int data) {
            if ((data & 0x80) != 0) {
                return (data >> 8) & 0xF;
            }
            return null;
        }

        public static int SetColor(int data, int? color) {
            if (color.HasValue) {
                return (data & -3969) | 0x80 | ((color.Value & 0xF) << 8);
            }
            return data & -3969;
        }
    }
}