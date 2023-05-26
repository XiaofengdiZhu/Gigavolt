using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVDisplayLedBlock : RotateableMountedGVElectricElementBlock {
        public const int Index = 1014;
        readonly Texture2D[] textures = new Texture2D[6];
        readonly string[] names = { "方块展示板", "图片显示器", "地层显示器" };

        public override void Initialize() {
            base.Initialize();
            for (int i = 0; i < 6; i++) {
                textures[i] = ContentManager.Get<Texture2D>("Textures/GVDisplayLedBlock" + i);
            }
        }

        public GVDisplayLedBlock() : base("Models/GVOneSurfaceBlock", "OneLed", 0.5f) { }

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData) {
            int data = Terrain.ExtractData(value);
            BlocksManager.DrawMeshBlock(
                primitivesRenderer,
                m_standaloneBlockMesh,
                textures[(GetType(data) << 1) | (GetComplex(data) ? 1 : 0)],
                color,
                2f * size,
                ref matrix,
                environmentData
            );
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) {
            int data = Terrain.ExtractData(value);
            generator.GenerateMeshVertices(
                this,
                x,
                y,
                z,
                m_blockMeshes[data & 0x1F],
                Color.White,
                null,
                geometry.GetGeometry(textures[(GetType(data) << 1) | (GetComplex(data) ? 1 : 0)]).SubsetOpaque
            );
            GenerateGVWireVertices(
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

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z) => new DisplayLedGVElectricElement(subsystemGVElectricity, new CellFace(x, y, z, GetFace(value)));

        public override GVElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z) {
            int face2 = GetFace(value);
            if (face == face2
                && SubsystemGVElectricity.GetConnectorDirection(face2, 0, connectorFace).HasValue) {
                return GVElectricConnectorType.Input;
            }
            return null;
        }

        public override IEnumerable<int> GetCreativeValues() {
            for (int i = 0; i < 6; i++) {
                yield return Terrain.MakeBlockValue(Index, 0, i << 5);
            }
        }

        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value) {
            int data = Terrain.ExtractData(value);
            string prefix = GetComplex(data) ? "GV复杂" : "GV简单";
            return $"{prefix}{names[GetType(data)]}";
        }

        public override string GetDescription(int value) {
            int data = Terrain.ExtractData(value);
            bool complex = GetComplex(data);
            int type = GetType(data);
            if (complex) {
                string prefix = "和简单";
                string suffix = "类似，但可以根据电压控制大小、位置、旋转、亮度、颜色，详见本Mod Github页面的介绍";
                return $"{prefix}{names[type]}{suffix}";
            }
            switch (type) {
                case 1: return "输入等于存储了图形数据的存储板ID的电压，就会在其面前显示图片";
                case 2: return "输入等于存储了地层数据的存储板ID的电压，就会在其面前显示地层";
                default: return "输入等于存储了方块数据的存储板ID的电压，就会在其面前显示方块";
            }
        }

        public static bool GetComplex(int data) => ((data >> 5) & 1) == 1;
        public static int SetComplex(int data, bool complex) => (data & -33) | ((complex ? 1 : 0) << 5);

        public static int GetType(int data) => (data >> 6) & 3;

        public static int SetType(int data, int type) => (data & -193) | ((type & 3) << 6);
    }
}