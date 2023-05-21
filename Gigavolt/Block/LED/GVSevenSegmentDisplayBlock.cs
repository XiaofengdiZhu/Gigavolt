using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVSevenSegmentDisplayBlock : MountedGVElectricElementBlock {
        public const int Index = 885;

        public BlockMesh m_standaloneBlockMesh;

        public BlockMesh[] m_blockMeshesByFace = new BlockMesh[4];

        public BoundingBox[][] m_collisionBoxesByFace = new BoundingBox[4][];

        public override void Initialize() {
            ModelMesh modelMesh = ContentManager.Get<Model>("Models/Gates").FindMesh("SevenSegmentDisplay");
            Matrix boneAbsoluteTransform = BlockMesh.GetBoneAbsoluteTransform(modelMesh.ParentBone);
            for (int i = 0; i < 4; i++) {
                Matrix m = Matrix.CreateRotationX((float)Math.PI / 2f) * Matrix.CreateTranslation(0f, 0f, -0.5f) * Matrix.CreateRotationY(i * (float)Math.PI / 2f) * Matrix.CreateTranslation(0.5f, 0.5f, 0.5f);
                m_blockMeshesByFace[i] = new BlockMesh();
                m_blockMeshesByFace[i]
                .AppendModelMeshPart(
                    modelMesh.MeshParts[0],
                    boneAbsoluteTransform * m,
                    false,
                    false,
                    false,
                    false,
                    Color.White
                );
                m_collisionBoxesByFace[i] = new BoundingBox[1] { m_blockMeshesByFace[i].CalculateBoundingBox() };
            }
            Matrix m2 = Matrix.CreateRotationY(-(float)Math.PI / 2f) * Matrix.CreateRotationZ((float)Math.PI / 2f);
            m_standaloneBlockMesh = new BlockMesh();
            m_standaloneBlockMesh.AppendModelMeshPart(
                modelMesh.MeshParts[0],
                boneAbsoluteTransform * m2,
                false,
                false,
                false,
                false,
                Color.White
            );
        }

        /*public override IEnumerable<CraftingRecipe> GetProceduralCraftingRecipes()
        {
            int color = 0;
            while (color < 8)
            {
                var craftingRecipe = new CraftingRecipe
                {
                    ResultCount = 4,
                    ResultValue = Terrain.MakeBlockValue(Index, 0, SetColor(0, color)),
                    RemainsCount = 1,
                    RemainsValue = Terrain.MakeBlockValue(90),
                    RequiredHeatLevel = 0f,
                    Description = "用铜玻璃和颜料来制作7段显示器"
                };
                craftingRecipe.Ingredients[0] = "glass";
                craftingRecipe.Ingredients[2] = "glass";
                craftingRecipe.Ingredients[4] = "paintbucket:" + color.ToString(CultureInfo.InvariantCulture);
                craftingRecipe.Ingredients[6] = "copperingot";
                craftingRecipe.Ingredients[7] = "copperingot";
                craftingRecipe.Ingredients[8] = "copperingot";
                yield return craftingRecipe;
                int num = color + 1;
                color = num;
            }
        }*/

        public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value) {
            int mountingFace = GetMountingFace(Terrain.ExtractData(value));
            return face != CellFace.OppositeFace(mountingFace);
        }

        public override int GetFace(int value) => GetMountingFace(Terrain.ExtractData(value));

        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value) {
            int data = Terrain.ExtractData(value);
            int color = GetColor(data);
            return LanguageControl.GetWorldPalette(color) + LanguageControl.GetBlock(string.Format("{0}:{1}", GetType().Name, data.ToString()), "DisplayName");
        }

        public override IEnumerable<int> GetCreativeValues() {
            int i = 0;
            while (i < 8) {
                yield return Terrain.MakeBlockValue(Index, 0, SetColor(0, i));
                int num = i + 1;
                i = num;
            }
        }

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult) {
            BlockPlacementData result;
            if (raycastResult.CellFace.Face < 4) {
                int data = SetMountingFace(Terrain.ExtractData(value), raycastResult.CellFace.Face);
                int value2 = Terrain.ReplaceData(value, data);
                result = default;
                result.Value = value2;
                result.CellFace = raycastResult.CellFace;
                return result;
            }
            result = default;
            return result;
        }

        public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris) {
            int color = GetColor(Terrain.ExtractData(oldValue));
            dropValues.Add(new BlockDropValue { Value = Terrain.MakeBlockValue(Index, 0, SetColor(0, color)), Count = 1 });
            showDebris = true;
        }

        public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value) {
            int mountingFace = GetMountingFace(Terrain.ExtractData(value));
            if (mountingFace >= m_collisionBoxesByFace.Length) {
                return null;
            }
            return m_collisionBoxesByFace[mountingFace];
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) {
            int mountingFace = GetMountingFace(Terrain.ExtractData(value));
            if (mountingFace < m_blockMeshesByFace.Length) {
                generator.GenerateMeshVertices(
                    this,
                    x,
                    y,
                    z,
                    m_blockMeshesByFace[mountingFace],
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
                    mountingFace,
                    1f,
                    Vector2.Zero,
                    geometry.SubsetOpaque
                );
            }
        }

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

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z) => new SevenSegmentDisplayGVElectricElement(subsystemGVElectricity, new CellFace(x, y, z, GetFace(value)));

        public override GVElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z) {
            int face2 = GetFace(value);
            if (face == face2
                && SubsystemGVElectricity.GetConnectorDirection(face2, 0, connectorFace).HasValue) {
                return GVElectricConnectorType.Input;
            }
            return null;
        }

        public static int GetMountingFace(int data) => data & 3;

        public static int SetMountingFace(int data, int face) => (data & -4) | (face & 3);

        public static int GetColor(int data) => (data >> 3) & 7;

        public static int SetColor(int data, int color) => (data & -57) | ((color & 7) << 3);
    }
}