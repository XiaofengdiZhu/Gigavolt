using System.Collections.Generic;

namespace Game {
    public class GVCounterBlock : RotateableMountedGVElectricElementBlock {
        public const int Index = 846;

        public GVCounterBlock() : base("Models/Gates", "Counter", 0.5f) { }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z) => new CounterGVElectricElement(subsystemGVElectricity, new CellFace(x, y, z, GetFace(value)));

        public override GVElectricConnectorType? GetGVConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z) {
            int data = Terrain.ExtractData(value);
            if (GetFace(value) == face) {
                GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(GetFace(value), GetRotation(data), connectorFace);
                if (connectorDirection == GVElectricConnectorDirection.Right
                    || connectorDirection == GVElectricConnectorDirection.Left
                    || connectorDirection == GVElectricConnectorDirection.In) {
                    return GVElectricConnectorType.Input;
                }
                if (connectorDirection == GVElectricConnectorDirection.Top
                    || connectorDirection == GVElectricConnectorDirection.Bottom) {
                    return GVElectricConnectorType.Output;
                }
            }
            return null;
        }

        public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris) {
            dropValues.Add(new BlockDropValue { Value = Terrain.MakeBlockValue(Index, 0, Terrain.ExtractData(oldValue) & 0x1FFE0), Count = 1 });
            showDebris = true;
        }

        public override bool IsNonDuplicable_(int value) => ((Terrain.ExtractData(value) >> 5) & 4095) > 0;
    }
}