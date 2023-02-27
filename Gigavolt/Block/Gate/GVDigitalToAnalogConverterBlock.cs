using Engine;
using Engine.Graphics;
using System.Collections.Generic;

namespace Game
{
    public class GVDigitalToAnalogConverterBlock : RotateableMountedGVElectricElementBlock
    {
        public const int Index = 880;

        public override void Initialize()
        {
            base.Initialize();
        }
        public GVDigitalToAnalogConverterBlock()
            : base("Models/Gates", "DigitalToAnalogConverter", 0.375f)
        {
        }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z)
        {
            return new DigitalToAnalogConverterGVElectricElement(subsystemGVElectricity, new CellFace(x, y, z, GetFace(value)),value);
        }

        public override GVElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
        {
            int data = Terrain.ExtractData(value);
            if (GetFace(value) == face)
            {
                GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(GetFace(value), GetRotation(data), connectorFace);
                if (connectorDirection == GVElectricConnectorDirection.In)
                {
                    return GVElectricConnectorType.Output;
                }
                if (connectorDirection == GVElectricConnectorDirection.Bottom || connectorDirection == GVElectricConnectorDirection.Top || connectorDirection == GVElectricConnectorDirection.Right || connectorDirection == GVElectricConnectorDirection.Left)
                {
                    return GVElectricConnectorType.Input;
                }
            }
            return null;
        }
        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value)
        {
            int type = GetType(Terrain.ExtractData(value));
            switch(type)
            {
                case 1:
                    return "GV 2位合并8位器";
                case 2:
                    return "GV 4位合并16位器";
                case 3:
                    return "GV 8位合并32位器";
                default:
                    return "GV 1位合并4位器";
            }
        }
        public override IEnumerable<int> GetCreativeValues()
        {
            for (int i = 0; i < 4; i++)
            {
                yield return Terrain.MakeBlockValue(Index, 0, SetType(0, i));
            }
        }
        public static int GetType(int data)
        {
            return (data >> 5) & 3;
        }

        public static int SetType(int data, int color)
        {
            return (data & -97) | ((color & 3) << 5);
        }
    }
}
