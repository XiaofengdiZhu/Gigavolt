using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVCameraBlock : RotateableMountedGVElectricElementBlock {
        public const int Index = 895;
        readonly Texture2D[] textures = new Texture2D[2];

        public override void Initialize() {
            base.Initialize();
            textures[0] = ContentManager.Get<Texture2D>("Textures/GVCameraBlock0");
            textures[1] = ContentManager.Get<Texture2D>("Textures/GVCameraBlock1");
        }

        public GVCameraBlock() : base("Models/GigavoltGates", "AnalogToDigitalConverter", 0.375f) { }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z) => new CameraGVElectricElement(subsystemGVElectricity, new CellFace(x, y, z, GetFace(value)));

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData) {
            BlocksManager.DrawMeshBlock(
                primitivesRenderer,
                m_standaloneBlockMesh,
                GetComplex(Terrain.ExtractData(value)) ? textures[1] : textures[0],
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
                geometry.GetGeometry(GetComplex(Terrain.ExtractData(value)) ? textures[1] : textures[0]).SubsetOpaque
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

        public override GVElectricConnectorType? GetGVConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z) {
            if (GetFace(value) == face
                && SubsystemGVElectricity.GetConnectorDirection(GetFace(value), 0, connectorFace).HasValue) {
                return GVElectricConnectorType.Input;
            }
            return null;
        }

        public override IEnumerable<int> GetCreativeValues() {
            yield return Terrain.MakeBlockValue(Index, 0, SetComplex(0, false));
            yield return Terrain.MakeBlockValue(Index, 0, SetComplex(0, true));
        }

        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value) => $"{(GetComplex(Terrain.ExtractData(value)) ? "GV复杂" : "GV简单")}照相机";

        public override string GetDescription(int value) => GetComplex(Terrain.ExtractData(value)) ? "用于拍照，可进行非常详尽的参数调整，详见本Mod Github页面的介绍" : "输入指定的存储板ID，每次输入发生变化时，就会对着该方块面对的方向拍照，并把图像传输到指定的存储板里，分辨率为512*512，视角为90度";

        public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris) {
            int data = Terrain.ExtractData(oldValue);
            dropValues.Add(new BlockDropValue { Value = Terrain.MakeBlockValue(Index, 0, SetComplex(0, GetComplex(data))), Count = 1 });
            showDebris = true;
        }

        public static bool GetComplex(int data) => ((data >> 5) & 1) == 1;
        public static int SetComplex(int data, bool complex) => (data & -33) | ((complex ? 1 : 0) << 5);
    }
}