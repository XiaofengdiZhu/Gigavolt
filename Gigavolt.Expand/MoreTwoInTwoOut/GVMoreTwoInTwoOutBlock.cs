using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVMoreTwoInTwoOutBlock : RotateableMountedGVElectricElementBlock {
        public const int Index = 879;
        public readonly Texture2D[] textures = new Texture2D[16];

        public readonly string[] textureNames = [
            "Adder",
            "Subtracter",
            "Multiplier",
            "Divider",
            "Remainder",
            "Equaler",
            "Greater",
            "NoLesser",
            "Lesser",
            "NoGreater",
            "Maximumer",
            "Minimumer",
            "LeftShifter",
            "RightShifter",
            "Power",
            "Logarithmer"
        ];

        public override void Initialize() {
            base.Initialize();
            for (int i = 0; i < 16; i++) {
                textures[i] = ContentManager.Get<Texture2D>($"Textures/GVMoreTwoInTwoOutBlock/{textureNames[i]}");
            }
        }

        public GVMoreTwoInTwoOutBlock() : base("Models/GigavoltGates", "AndGate", 0.5f) { }

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

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z, uint subterrainId) => new MoreTwoInTwoOutGVElectricElement(subsystemGVElectricity, new GVCellFace(x, y, z, GetFace(value)), value, subterrainId);

        public override GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem, int value, int face, int connectorFace, int x, int y, int z, uint subterrainId) {
            int data = Terrain.ExtractData(value);
            if (GetFace(value) == face) {
                GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(GetFace(value), GetRotation(data), connectorFace);
                if (connectorDirection == GVElectricConnectorDirection.Right
                    || connectorDirection == GVElectricConnectorDirection.Left) {
                    return GVElectricConnectorType.Input;
                }
                if (connectorDirection == GVElectricConnectorDirection.Top
                    || connectorDirection == GVElectricConnectorDirection.In
                    || connectorDirection == GVElectricConnectorDirection.Bottom) {
                    return GVElectricConnectorType.Output;
                }
            }
            return null;
        }

        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value) {
            int type = GetType(Terrain.ExtractData(value));
            return type switch {
                1 => "GV减法器",
                2 => "GV乘法器",
                3 => "GV除法器",
                4 => "GV取余器",
                5 => "GV等于门",
                6 => "GV大于门",
                7 => "GV大于等于门",
                8 => "GV小于门",
                9 => "GV小于等于门",
                10 => "GV取大器",
                11 => "GV取小器",
                12 => "GV左移器",
                13 => "GV右移器",
                14 => "GV乘方器",
                15 => "GV对数器",
                _ => "GV加法器"
            };
        }

        public override string GetDescription(int value) {
            int type = GetType(Terrain.ExtractData(value));
            string start = "左和右是输入端，上和后是本位输出端，下是溢出/借位等输出端";
            string name = GetDisplayName(null, value);
            string end1 = type switch {
                1 => "左-右，结果小于0时加上2^32",
                2 => "左*右，结果超过2^32时输出最低的32位",
                3 => "左/右，整数计算只保留整数部分",
                4 => "左%右，整数计算",
                5 => "左==右时输出0xFFFFFFFF V，否则0V",
                6 or 7 or 8 or 9 => "略",
                10 => "取两边较大的输入作为输出",
                11 => "取两边较小的输入作为输出",
                12 => "左<<右，结果超过2^32时输出最低的32位",
                13 => "左>>右",
                14 => "左^右，结果超过2^32时输出最低的32位",
                15 => "lg(左)/lg(右)，结果只保留整数部分",
                _ => "左+右，结果超过2^32时输出最低的32位"
            };
            string end2 = type switch {
                1 => "溢出时输出1V",
                2 => "需要借位时输出1V",
                3 => "溢出时输出结果的第33到64位",
                4 or 5 or 6 or 7 or 8 or 9 or 10 or 11 or 15 => "0V",
                12 => "溢出时输出结果的第33到64位",
                13 => "左<<32>>右的最低的32位",
                14 => "溢出时输出结果的第33到64位",
                _ => "溢出时输出1V"
            };
            return $"{start}，对于{name}：\n本位：{end1}\n溢出/借位：{end2}";
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