using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVMoreOneInOneOutBlock : RotateableMountedGVElectricElementBlock {
        public const int Index = 881;
        public Texture2D[] textures = new Texture2D[16];
        public string[] textureNames = { "Sin", "Cos", "Tan", "Cot", "Sec", "Csc", "Asin", "Acos", "Atan", "Sinh", "Cosh", "Tanh", "Deg2Rad", "Rad2Deg", "SMR", "TCR" };

        public override void Initialize() {
            base.Initialize();
            for (int i = 0; i < 16; i++) {
                textures[i] = ContentManager.Get<Texture2D>($"Textures/GVMoreOneInOneOutBlock/{textureNames[i]}");
            }
        }

        public GVMoreOneInOneOutBlock() : base("Models/GigavoltGates", "AndGate", 0.5f) { }

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

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z) => new MoreOneInOneOutGVElectricElement(subsystemGVElectricity, new CellFace(x, y, z, GetFace(value)), value);

        public override GVElectricConnectorType? GetGVConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z) {
            int data = Terrain.ExtractData(value);
            if (GetFace(value) == face) {
                GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(GetFace(value), GetRotation(data), connectorFace);
                if (connectorDirection == GVElectricConnectorDirection.Bottom) {
                    return GVElectricConnectorType.Input;
                }
                if (connectorDirection == GVElectricConnectorDirection.Top
                    || connectorDirection == GVElectricConnectorDirection.In) {
                    return GVElectricConnectorType.Output;
                }
            }
            return null;
        }

        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value) {
            int type = GetType(Terrain.ExtractData(value));
            switch (type) {
                case 1: return "GV余弦器";
                case 2: return "GV正切器";
                case 3: return "GV余切器";
                case 4: return "GV正割器";
                case 5: return "GV余割器";
                case 6: return "GV反正弦器";
                case 7: return "GV反余弦器";
                case 8: return "GV反正切器";
                case 9: return "GV双曲正弦器";
                case 10: return "GV双曲余弦器";
                case 11: return "GV双曲正切器";
                case 12: return "GV角度转弧度器";
                case 13: return "GV弧度转角度器";
                case 14: return "GV原码正负转换器";
                case 15: return "GV补码正负转换器";
                default: return "GV正弦器";
            }
        }

        public override IEnumerable<int> GetCreativeValues() {
            for (int i = 0; i < 16; i++) {
                yield return Terrain.MakeBlockValue(Index, 0, SetType(0, i));
            }
        }

        public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris) {
            int data = Terrain.ExtractData(oldValue);
            dropValues.Add(new BlockDropValue { Value = Terrain.MakeBlockValue(Index, 0, SetType(data, GetType(data))), Count = 1 });
            showDebris = true;
        }

        public static int GetType(int data) => (data >> 5) & 15;

        public static int SetType(int data, int type) => (data & -481) | ((type & 15) << 5);
    }
}