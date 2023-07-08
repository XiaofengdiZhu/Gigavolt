using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVAnalogToDigitalConverterBlock : RotateableMountedGVElectricElementBlock {
        public const int Index = 851;
        readonly Texture2D[] textures = new Texture2D[4];

        public override void Initialize() {
            base.Initialize();
            for (int i = 0; i < 4; i++) {
                textures[i] = ContentManager.Get<Texture2D>($"Textures/GVAnalogToDigitalConverterBlock{4 << i}-{1 << i}");
            }
        }

        public GVAnalogToDigitalConverterBlock() : base("Models/GigavoltGates", "AnalogToDigitalConverter", 0.375f) { }

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

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z) => new AnalogToDigitalConverterGVElectricElement(subsystemGVElectricity, new CellFace(x, y, z, GetFace(value)), value);

        public override GVElectricConnectorType? GetGVConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z) {
            int data = Terrain.ExtractData(value);
            if (GetFace(value) == face) {
                GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(GetFace(value), GetRotation(data), connectorFace);
                if (connectorDirection == GVElectricConnectorDirection.In) {
                    return GVElectricConnectorType.Input;
                }
                if (connectorDirection == GVElectricConnectorDirection.Bottom
                    || connectorDirection == GVElectricConnectorDirection.Top
                    || connectorDirection == GVElectricConnectorDirection.Right
                    || connectorDirection == GVElectricConnectorDirection.Left) {
                    return GVElectricConnectorType.Output;
                }
            }
            return null;
        }

        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value) {
            int type = GetType(Terrain.ExtractData(value));
            switch (type) {
                case 1: return "GV 8位拆分2位器";
                case 2: return "GV 16位拆分4位器";
                case 3: return "GV 32位拆分8位器";
                default: return "GV 4位拆分1位器";
            }
        }

        public override IEnumerable<int> GetCreativeValues() {
            for (int i = 0; i < 4; i++) {
                yield return Terrain.MakeBlockValue(Index, 0, SetType(0, i));
            }
        }

        public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris) {
            int data = Terrain.ExtractData(oldValue);
            dropValues.Add(new BlockDropValue { Value = Terrain.MakeBlockValue(Index, 0, SetType(data, GetType(data))), Count = 1 });
            showDebris = true;
        }

        public static int GetType(int data) => (data >> 5) & 3;

        public static int SetType(int data, int color) => (data & -97) | ((color & 3) << 5);
    }
}