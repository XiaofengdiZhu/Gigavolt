using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVDigitalToAnalogConverterBlock : RotateableMountedGVElectricElementBlock, IGVCustomWheelPanelBlock {
        public const int Index = 850;
        readonly Texture2D[] textures = new Texture2D[4];

        public override void Initialize() {
            base.Initialize();
            for (int i = 0; i < 4; i++) {
                textures[i] = ContentManager.Get<Texture2D>($"Textures/GVDigitalToAnalogConverterBlock{1 << i}-{4 << i}");
            }
        }

        public GVDigitalToAnalogConverterBlock() : base("Models/GigavoltGates", "AnalogToDigitalConverter", 0.375f) { }

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData) {
            BlocksManager.DrawMeshBlock(
                primitivesRenderer,
                m_standaloneBlockMesh,
                textures[GetType(Terrain.ExtractData(value))],
                color,
                2f * size,
                ref matrix,
                environmentData
            );
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) {
            int num = Terrain.ExtractData(value) & 0x1F;
            generator.GenerateMeshVertices(
                this,
                x,
                y,
                z,
                m_blockMeshes[num],
                Color.White,
                null,
                geometry.GetGeometry(textures[GetType(Terrain.ExtractData(value))]).SubsetOpaque
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

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z, uint subterrainId) => new DigitalToAnalogConverterGVElectricElement(subsystemGVElectricity, new GVCellFace(x, y, z, GetFace(value)), value, subterrainId);

        public override GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem, int value, int face, int connectorFace, int x, int y, int z, uint subterrainId) {
            int data = Terrain.ExtractData(value);
            if (GetFace(value) == face) {
                GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(GetFace(value), GetRotation(data), connectorFace);
                if (connectorDirection == GVElectricConnectorDirection.In) {
                    return GVElectricConnectorType.Output;
                }
                if (connectorDirection == GVElectricConnectorDirection.Bottom
                    || connectorDirection == GVElectricConnectorDirection.Top
                    || connectorDirection == GVElectricConnectorDirection.Right
                    || connectorDirection == GVElectricConnectorDirection.Left) {
                    return GVElectricConnectorType.Input;
                }
            }
            return null;
        }

        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value) {
            int data = Terrain.ExtractData(value);
            if (GetClassic(data)) {
                return LanguageControl.Get(GetType().Name, "ClassicDisplayName");
            }
            int type = GetType(Terrain.ExtractData(value));
            string format = LanguageControl.Get(GetType().Name, "DisplayName");
            return type switch {
                1 => string.Format(format, 2, 8),
                2 => string.Format(format, 4, 16),
                3 => string.Format(format, 8, 32),
                _ => string.Format(format, 1, 4)
            };
        }

        public override string GetDescription(int value) => LanguageControl.Get(GetType().Name, GetClassic(Terrain.ExtractData(value)) ? "ClassicDescription" : "Description");
        public override string GetCategory(int value) => GetClassic(Terrain.ExtractData(value)) ? "GV Electrics Regular" : "GV Electrics Shift";
        public override int GetDisplayOrder(int value) => GetClassic(Terrain.ExtractData(value)) ? 16 : 10;

        public override IEnumerable<int> GetCreativeValues() {
            yield return Terrain.MakeBlockValue(Index, 0, SetClassic(0, true));
            for (int i = 0; i < 4; i++) {
                yield return Terrain.MakeBlockValue(Index, 0, SetType(0, i));
            }
        }

        public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris) {
            int data = Terrain.ExtractData(oldValue);
            dropValues.Add(new BlockDropValue { Value = Terrain.MakeBlockValue(Index, 0, GetClassic(data) ? SetClassic(0, true) : SetType(0, GetType(data))), Count = 1 });
            showDebris = true;
        }

        public static int GetType(int data) => (data >> 5) & 3;

        public static int SetType(int data, int color) => (data & -97) | ((color & 3) << 5);
        public static bool GetClassic(int data) => (data & 128) != 0;
        public static int SetClassic(int data, bool classic) => (data & -129) | (classic ? 128 : 0);

        public List<int> GetCustomWheelPanelValues(int centerValue) {
            List<int> result = [];
            for (int i = 0; i < 4; i++) {
                result.Add(Terrain.MakeBlockValue(Index, 0, SetType(0, i)));
            }
            for (int i = 0; i < 4; i++) {
                result.Add(Terrain.MakeBlockValue(GVAnalogToDigitalConverterBlock.Index, 0, GVAnalogToDigitalConverterBlock.SetType(0, i)));
            }
            return result;
        }
    }
}