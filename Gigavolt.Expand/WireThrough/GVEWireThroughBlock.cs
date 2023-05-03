using Engine;
using Engine.Graphics;
using System.Collections.Generic;

namespace Game
{
    public class GVEWireThroughBlock : CubeBlock, IGVElectricWireElementBlock, IGVElectricElementBlock, IPaintableBlock
    {
        public const int Index = 1021;
        public int[] m_wiredTextureSlot=new int[] { 168,184,152,216,136 };
        public int[] m_unwiredTextureSlot=new int[] { 4,1,70,78,16 };
        public int[] m_coloredTextureSlot=new int[] { 23,24,39,78,69 };

        public GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z)
        {
            return null;
        }

        public GVElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
        {
            if (!WireExistsOnFace(value, face))
            {
                return null;
            }
            return GVElectricConnectorType.InputOutput;
        }

        public int GetConnectionMask(int value)
        {
            return int.MaxValue;
        }

        public int GetConnectedWireFacesMask(int value, int face)
        {
            int num = 0;
            if (WireExistsOnFace(value, face))
            {
                int num2 = CellFace.OppositeFace(face);
                bool flag = false;
                for (int i = 0; i < 6; i++)
                {
                    if (i == face)
                    {
                        num |= 1 << i;
                    }
                    else if (i != num2 && WireExistsOnFace(value, i))
                    {
                        num |= 1 << i;
                        flag = true;
                    }
                }
                if (flag && WireExistsOnFace(value, num2))
                {
                    num |= 1 << num2;
                }
            }
            return num;
        }
        public override int GetFaceTextureSlot(int face, int value)
        {
            int type = GetType(Terrain.ExtractData(value));
            if (WireExistsOnFace(value, CellFace.OppositeFace(face)))
            {
                return m_wiredTextureSlot[type];
            }
            if (GetPaintColor(value).HasValue)
            {
                return m_coloredTextureSlot[type];
            }
            return m_unwiredTextureSlot[type];
        }
        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z)
        {
            int data = Terrain.ExtractData(value);
            Color color = SubsystemPalette.GetColor(generator, GetColor(data));
            generator.GenerateCubeVertices(this, value, x, y, z, color, geometry.OpaqueSubsetsByFace);
        }

        public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
        {
            int? paintColor = GetPaintColor(oldValue);
            for (int i = 0; i < 6; i++)
            {
                if (WireExistsOnFace(oldValue, i) && !WireExistsOnFace(newValue, i))
                {
                    dropValues.Add(new BlockDropValue
                    {
                        Value = Terrain.MakeBlockValue(GVWireBlock.Index, 0, SetColor(0, paintColor)),
                        Count = 1
                    });
                }
            }
            showDebris = (dropValues.Count > 0);
        }

        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
        {
            int? paintColor = GetPaintColor(value);
            return SubsystemPalette.GetName(subsystemTerrain, paintColor, base.GetDisplayName(subsystemTerrain, value));
        }

        public int? GetPaintColor(int value)
        {
            return GetColor(Terrain.ExtractData(value));
        }

        public int Paint(SubsystemTerrain subsystemTerrain, int value, int? color)
        {
            int data = Terrain.ExtractData(value);
            return Terrain.ReplaceData(value, SetColor(data, color));
        }

        public static bool WireExistsOnFace(int value, int face)
        {
            return (GetWireFacesBitmask(value) & (1 << face)) != 0;
        }

        public static int GetWireFacesBitmask(int value)
        {
            if (Terrain.ExtractContents(value) == Index)
            {
                return Terrain.ExtractData(value) & 0x3F;
            }
            return 0;
        }

        public static int SetWireFacesBitmask(int value, int bitmask)
        {
            int num = Terrain.ExtractData(value);
            num &= -64;
            num |= (bitmask & 0x3F);
            return Terrain.ReplaceData(Terrain.ReplaceContents(value, Index), num);
        }

        public static int? GetColor(int data)
        {
            if ((data & 0x40) != 0)
            {
                return (data >> 7) & 0xF;
            }
            return null;
        }

        public static int SetColor(int data, int? color)
        {
            if (color.HasValue)
            {
                return (data & -1985) | 0x40 | ((color.Value & 0xF) << 7);
            }
            return data & -1985;
        }
        public static int GetType(int data)
        {
            return (data >> 11) & 7;
        }
        public static int SetType(int data, int type)
        {
            return (data & -14337) | ((type & 7) << 11);
        }
    }
}