using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public class GVEWireThroughBlock : Block, IGVElectricWireElementBlock, IPaintableBlock {
        public const int Index = 868;

        public readonly int[] m_wiredTextureSlot = [
            168,
            184,
            152,
            136,
            216
        ];

        public readonly int[] m_unwiredTextureSlot = [
            4,
            1,
            70,
            16,
            78
        ];

        public readonly int[] m_coloredTextureSlot = [
            23,
            24,
            39,
            69,
            78
        ];

        public static Texture2D m_harnessTexture;

        public Texture2D HarnessTexture => m_harnessTexture ?? BlocksTexturesManager.DefaultBlocksTexture;

        public GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z) => null;

        public GVElectricConnectorType? GetGVConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z) {
            if (WireExistsOnFace(value, face)
                && connectorFace == CellFace.OppositeFace(face)) {
                return GVElectricConnectorType.InputOutput;
            }
            return null;
        }

        public int GetConnectionMask(int value) => int.MaxValue;

        public int GetConnectedWireFacesMask(int value, int face) {
            int data = Terrain.ExtractData(value);
            if (GetIsCross(data)) {
                return GetWireFacesBitmask(data) == 63 ? (1 << face) | (1 << CellFace.OppositeFace(face)) : 0;
            }
            return WireExistsOnFace(value, face) ? GetWireFacesBitmask(data) : 0;
        }

        public override int GetFaceTextureSlot(int face, int value) {
            int data = Terrain.ExtractData(value);
            int texture = GetIsCross(data) ? 4 : GetTexture(Terrain.ExtractData(value));
            if (WireExistsOnFace(value, CellFace.OppositeFace(face))) {
                return m_wiredTextureSlot[texture];
            }
            return GetPaintColor(value).HasValue ? m_coloredTextureSlot[texture] : m_unwiredTextureSlot[texture];
        }

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) {
            int data = Terrain.ExtractData(value);
            Color color = SubsystemPalette.GetColor(generator, GetColor(data));
            generator.GenerateCubeVertices(
                this,
                value,
                x,
                y,
                z,
                color,
                GetIsWireHarness(data) ? geometry.GetGeometry(m_harnessTexture).OpaqueSubsetsByFace : geometry.OpaqueSubsetsByFace
            );
        }

        public override void DrawBlock(PrimitivesRenderer3D primitivesRenderer, int value, Color color, float size, ref Matrix matrix, DrawBlockEnvironmentData environmentData) {
            environmentData = environmentData ?? BlocksManager.m_defaultEnvironmentData;
            BlocksManager.DrawCubeBlock(
                primitivesRenderer,
                value,
                new Vector3(size),
                ref matrix,
                color,
                color,
                environmentData,
                GetIsWireHarness(Terrain.ExtractData(value)) ? HarnessTexture :
                environmentData.SubsystemTerrain == null ? BlocksTexturesManager.DefaultBlocksTexture : environmentData.SubsystemTerrain.SubsystemAnimatedTextures.AnimatedBlocksTexture
            );
        }

        public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris) {
            int? paintColor = GetPaintColor(oldValue);
            int data = Terrain.ExtractData(oldValue);
            int bitmask = GetWireFacesBitmask(data);
            if (bitmask == 63
                && GetIsCross(data)) {
                dropValues.Add(new BlockDropValue { Value = Terrain.MakeBlockValue(Index, 0, SetColor(data, null)), Count = 1 });
            }
            else {
                for (int i = 0; i < 6; i++) {
                    if (WireExistsOnFace(oldValue, i)
                        && !WireExistsOnFace(newValue, i)) {
                        dropValues.Add(new BlockDropValue { Value = Terrain.MakeBlockValue(GetIsWireHarness(data) ? GVWireHarnessBlock.Index : GVWireBlock.Index, 0, SetColor(0, paintColor)), Count = 1 });
                    }
                }
            }
            showDebris = dropValues.Count > 0;
        }

        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value) {
            int data = Terrain.ExtractData(value);
            return SubsystemPalette.GetName(subsystemTerrain, GetColor(data), $"GV{(GetWireFacesBitmask(data) == 63 ? "六" : "多")}面{(GetIsCross(data) ? "跨" : "穿")}{(GetIsWireHarness(data) ? "总" : "")}线块");
        }

        public override string GetDescription(int value) {
            int data = Terrain.ExtractData(value);
            if (GetIsCross(data)) {
                return "仅相对的两面互相导通；可染色，仅影响外观，不影响接线";
            }
            string name = GetIsWireHarness(data) ? "总" : "导";
            return $"使用铜锤将一格范围内已摆放好的{name}线转换成的穿线块，已摆放{name}线的面将互相连通，未摆放{name}线的面不互通；对其多次使用铜锤将改变外观，第4次恢复为普通的{name}线；染色不影响接线，仅影响外观";
        }

        public override IEnumerable<int> GetCreativeValues() {
            yield return Terrain.MakeBlockValue(Index, 0, 63);
            yield return Terrain.MakeBlockValue(Index, 0, SetIsCross(63, true));
            yield return Terrain.MakeBlockValue(Index, 0, SetIsWireHarness(63, true));
            yield return Terrain.MakeBlockValue(Index, 0, SetIsWireHarness(SetIsCross(63, true), true));
        }

        public int? GetPaintColor(int value) => GetColor(Terrain.ExtractData(value));

        public int Paint(SubsystemTerrain subsystemTerrain, int value, int? color) {
            int data = Terrain.ExtractData(value);
            return Terrain.ReplaceData(value, SetColor(data, color));
        }

        public static bool WireExistsOnFace(int value, int face) => (GetWireFacesBitmask(Terrain.ExtractData(value)) & (1 << face)) != 0;

        public static int GetWireFacesBitmask(int data) => data & 0x3F;

        public static int SetWireFacesBitmask(int data, int bitmask) {
            data &= -64;
            data |= bitmask & 0x3F;
            return data;
        }

        public static int? GetColor(int data) {
            if ((data & 0x40) != 0) {
                return (data >> 7) & 0xF;
            }
            return null;
        }

        public static int SetColor(int data, int? color) {
            if (color.HasValue) {
                return (data & -1985) | 0x40 | ((color.Value & 0xF) << 7);
            }
            return data & -1985;
        }

        public static int GetTexture(int data) => (data >> 11) & 3;
        public static int SetTexture(int data, int texture) => (data & -6145) | ((texture & 3) << 11);
        public static bool GetIsCross(int data) => ((data >> 13) & 1) == 1;
        public static int SetIsCross(int data, bool isCross) => (data & -8193) | ((isCross ? 1 : 0) << 13);
        public static bool GetIsWireHarness(int data) => ((data >> 14) & 1) == 1;
        public static int SetIsWireHarness(int data, bool isWireHarness) => (data & -16385) | ((isWireHarness ? 1 : 0) << 14);
        public bool IsWireHarness(int value) => GetIsWireHarness(Terrain.ExtractData(value));
        public bool IsWireThrough() => true;
    }
}