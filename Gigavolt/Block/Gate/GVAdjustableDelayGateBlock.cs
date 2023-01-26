using System.Collections.Generic;

namespace Game
{
    public class GVAdjustableDelayGateBlock : RotateableMountedGVElectricElementBlock
    {
        public const int Index = 924;

        public GVAdjustableDelayGateBlock()
            : base("Models/Gates", "AdjustableDelayGate", 0.375f)
        {
        }

        public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris)
        {
            showDebris = true;
            if (toolLevel >= RequiredToolLevel)
            {
                int delay = GetDelay(Terrain.ExtractData(oldValue));
                int data = SetDelay(0, delay);
                dropValues.Add(new BlockDropValue
                {
                    Value = Terrain.MakeBlockValue(924, 0, data),
                    Count = 1
                });
            }
        }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z)
        {
            return new AdjustableDelayGateGVElectricElement(subsystemGVElectricity, new CellFace(x, y, z, GetFace(value)));
        }

        public override GVElectricConnectorType? GetConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z)
        {
            int data = Terrain.ExtractData(value);
            if (GetFace(value) == face)
            {
                GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(GetFace(value), GetRotation(data), connectorFace);
                if (connectorDirection == GVElectricConnectorDirection.Bottom)
                {
                    return GVElectricConnectorType.Input;
                }
                if (connectorDirection == GVElectricConnectorDirection.Top || connectorDirection == GVElectricConnectorDirection.In)
                {
                    return GVElectricConnectorType.Output;
                }
            }
            return null;
        }

        public static int GetDelay(int data)
        {
            return (data >> 5) & 0xFF;
        }

        public static int SetDelay(int data, int delay)
        {
            return (data & -8161) | ((delay & 0xFF) << 5);
        }
    }
}
