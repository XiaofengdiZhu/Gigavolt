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

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity,
            int value,
            int x,
            int y,
            int z,
            uint subterrainId) => new CameraGVElectricElement(subsystemGVElectricity, new GVCellFace(x, y, z, GetFace(value)), subterrainId);

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer,
            int value,
            Color color,
            float size,
            ref Matrix matrix,
            DrawBlockEnvironmentData environmentData) {
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

        public override GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem,
            int value,
            int face,
            int connectorFace,
            int x,
            int y,
            int z,
            Terrain terrain) {
            if (GetFace(value) == face
                && SubsystemGVElectricity.GetConnectorDirection(GetFace(value), 0, connectorFace).HasValue) {
                return GVElectricConnectorType.Input;
            }
            return null;
        }

        public override IEnumerable<int> GetCreativeValues() {
            yield return Terrain.MakeBlockValue(BlockIndex, 0, SetComplex(0, false));
            yield return Terrain.MakeBlockValue(BlockIndex, 0, SetComplex(0, true));
        }

        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value) => LanguageControl.Get(
            GetType().Name,
            "DisplayName",
            GetComplex(Terrain.ExtractData(value)) ? "2" : "1"
        );

        public override string GetDescription(int value) => LanguageControl.Get(
            GetType().Name,
            "Description",
            GetComplex(Terrain.ExtractData(value)) ? "2" : "1"
        );

        public override void GetDropValues(SubsystemTerrain subsystemTerrain,
            int oldValue,
            int newValue,
            int toolLevel,
            List<BlockDropValue> dropValues,
            out bool showDebris) {
            int data = Terrain.ExtractData(oldValue);
            dropValues.Add(new BlockDropValue { Value = Terrain.MakeBlockValue(BlockIndex, 0, SetComplex(0, GetComplex(data))), Count = 1 });
            showDebris = true;
        }

        public static bool GetComplex(int data) => ((data >> 5) & 1) == 1;
        public static int SetComplex(int data, bool complex) => (data & -33) | ((complex ? 1 : 0) << 5);
    }
}