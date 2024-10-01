using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVDisplayLedBlock : RotateableMountedGVElectricElementBlock, IGVCustomWheelPanelBlock {
        public const int Index = 874;
        readonly Texture2D[] textures = new Texture2D[6];
        readonly string[] names = ["方块展示板", "图片显示器", "地层显示器"];
        public Color topColor = new(0xFF38E14F);

        public override void Initialize() {
            base.Initialize();
            for (int i = 0; i < 6; i++) {
                textures[i] = ContentManager.Get<Texture2D>("Textures/GVDisplayLedBlock" + i);
            }
            IGVCustomWheelPanelBlock.LedValues.AddRange(GetCreativeValues());
            IGVCustomWheelPanelBlock.LedValues.Add(GVBlocksManager.GetBlockIndex<GVOscilloscopeBlock>());
        }

        public GVDisplayLedBlock() : base("Models/GigavoltGates", "OneLed", 0.5f) { }

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
            int type = GetType(data);
            if (type == 0) {
                Matrix matrix2 = Matrix.CreateTranslation(new Vector3(-0.65f, 0.33f, 0.2f));
                BlocksManager.DrawCubeBlock(
                    primitivesRenderer,
                    GrassBlock.Index,
                    new Vector3(0.4f),
                    ref matrix2,
                    Color.White,
                    topColor,
                    environmentData
                );
            }
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
            GVBlockGeometryGenerator.GenerateGVWireVertices(
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

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z, uint subterrainId) => new DisplayLedGVElectricElement(subsystemGVElectricity, new GVCellFace(x, y, z, GetFace(value)), subterrainId);

        public override GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem, int value, int face, int connectorFace, int x, int y, int z, uint subterrainId) {
            int face2 = GetFace(value);
            if (face == face2
                && SubsystemGVElectricity.GetConnectorDirection(face2, 0, connectorFace).HasValue) {
                return GVElectricConnectorType.Input;
            }
            return null;
        }

        public override IEnumerable<int> GetCreativeValues() {
            for (int i = 0; i < 6; i++) {
                yield return Terrain.MakeBlockValue(BlockIndex, 0, i << 5);
            }
        }

        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value) {
            string typeName = GetType().Name;
            int data = Terrain.ExtractData(value);
            return LanguageControl.Get(typeName, "DisplayNamePrefix", GetComplex(data) ? "2" : "1") + LanguageControl.Get(typeName, "DisplayName", (GetType(data) + 1).ToString());
        }

        public override string GetDescription(int value) {
            string typeName = GetType().Name;
            int data = Terrain.ExtractData(value);
            return GetComplex(data) ? string.Format(LanguageControl.Get(typeName, "ComplexDescription", "0"), LanguageControl.Get(typeName, "ComplexDescription", (GetType(data) + 1).ToString())) : LanguageControl.Get(typeName, "SimpleDescription", (GetType(data) + 1).ToString());
        }

        public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris) {
            int data = Terrain.ExtractData(oldValue);
            dropValues.Add(new BlockDropValue { Value = Terrain.MakeBlockValue(BlockIndex, 0, SetComplex(SetType(0, GetType(data)), GetComplex(data))), Count = 1 });
            showDebris = true;
        }

        public static bool GetComplex(int data) => ((data >> 5) & 1) == 1;
        public static int SetComplex(int data, bool complex) => (data & -33) | ((complex ? 1 : 0) << 5);

        public static int GetType(int data) => (data >> 6) & 3;

        public static int SetType(int data, int type) => (data & -193) | ((type & 3) << 6);
        public List<int> GetCustomWheelPanelValues(int centerValue) => IGVCustomWheelPanelBlock.LedValues;
    }
}