using System.Collections.Generic;
using Engine;

namespace Game {
    public class GVEWireThroughBlock : CubeBlock, IGVElectricWireElementBlock, IPaintableBlock {
        public const int Index = 1021;
        public int[] m_wiredTextureSlot = { 168, 184, 152, 136, 216 };
        public int[] m_unwiredTextureSlot = { 4, 1, 70, 16, 78 };
        public int[] m_coloredTextureSlot = { 23, 24, 39, 69, 78 };

        public GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z) => null;

        public GVElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z) {
            if (WireExistsOnFace(value, face)
                && connectorFace == CellFace.OppositeFace(face)) {
                return GVElectricConnectorType.InputOutput;
            }
            return null;
        }

        public int GetConnectionMask(int value) => int.MaxValue;

        public int GetConnectedWireFacesMask(int value, int face) {
            int num = 0;
            int data = Terrain.ExtractData(value);
            int type = GetType(data);
            if (type < 4) {
                if (WireExistsOnFace(value, face)) {
                    int num2 = CellFace.OppositeFace(face);
                    bool flag = false;
                    for (int i = 0; i < 6; i++) {
                        if (i == face) {
                            num |= 1 << i;
                        }
                        else if (i != num2
                            && WireExistsOnFace(value, i)) {
                            num |= 1 << i;
                            flag = true;
                        }
                    }
                    if (flag && WireExistsOnFace(value, num2)) {
                        num |= 1 << num2;
                    }
                }
            }
            else if (type == 4
                && GetWireFacesBitmask(data) == 63) {
                return (1 << face) | (1 << CellFace.OppositeFace(face));
            }
            return num;
        }

        public override int GetFaceTextureSlot(int face, int value) {
            int type = GetType(Terrain.ExtractData(value));
            if (WireExistsOnFace(value, CellFace.OppositeFace(face))) {
                return m_wiredTextureSlot[type];
            }
            if (GetPaintColor(value).HasValue) {
                return m_coloredTextureSlot[type];
            }
            return m_unwiredTextureSlot[type];
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
                geometry.OpaqueSubsetsByFace
            );
        }

        public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris) {
            int? paintColor = GetPaintColor(oldValue);
            for (int i = 0; i < 6; i++) {
                if (WireExistsOnFace(oldValue, i)
                    && !WireExistsOnFace(newValue, i)) {
                    dropValues.Add(new BlockDropValue { Value = Terrain.MakeBlockValue(GVWireBlock.Index, 0, SetColor(0, paintColor)), Count = 1 });
                }
            }
            showDebris = dropValues.Count > 0;
        }

        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value) {
            int data = Terrain.ExtractData(value);
            int type = GetType(data);
            if (type < 4) {
                if (GetWireFacesBitmask(data) == 63) {
                    return "GV六面穿线块";
                }
                return "GV多面穿线块";
            }
            if (type == 4) {
                if (GetWireFacesBitmask(data) == 63) {
                    return "GV六面跨线块";
                }
                return "GV多面跨线块";
            }
            return null;
        }

        public override IEnumerable<int> GetCreativeValues() {
            yield return Terrain.MakeBlockValue(Index, 0, 63);
            yield return Terrain.MakeBlockValue(Index, 0, 63 | (4 << 11));
        }

        public int? GetPaintColor(int value) => GetColor(Terrain.ExtractData(value));

        public int Paint(SubsystemTerrain subsystemTerrain, int value, int? color) {
            int data = Terrain.ExtractData(value);
            return Terrain.ReplaceData(value, SetColor(data, color));
        }

        public static bool WireExistsOnFace(int value, int face) => (GetWireFacesBitmask(Terrain.ExtractData(value)) & (1 << face)) != 0;

        public static int GetWireFacesBitmask(int data) => data & 0x3F;

        public static int SetWireFacesBitmask(int value, int bitmask) {
            int num = Terrain.ExtractData(value);
            num &= -64;
            num |= bitmask & 0x3F;
            return Terrain.ReplaceData(Terrain.ReplaceContents(value, Index), num);
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

        public static int GetType(int data) => (data >> 11) & 7;

        public static int SetType(int data, int type) => (data & -14337) | ((type & 7) << 11);
    }
}