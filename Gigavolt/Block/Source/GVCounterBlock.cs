using System.Collections.Generic;
using GameEntitySystem;

namespace Game {
    public class GVCounterBlock : RotateableMountedGVElectricElementBlock, IGVCustomWheelPanelBlock {
        public const int Index = 846;

        public GVCounterBlock() : base("Models/Gates", "Counter", 0.5f) { }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z, uint subterrainId) => new CounterGVElectricElement(subsystemGVElectricity, value, new GVCellFace(x, y, z, GetFace(value)), subterrainId);

        public override GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem, int value, int face, int connectorFace, int x, int y, int z, uint subterrainId) {
            int data = Terrain.ExtractData(value);
            if (GetFace(value) == face) {
                GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(GetFace(value), GetRotation(data), connectorFace);
                switch (connectorDirection) {
                    case GVElectricConnectorDirection.Right:
                    case GVElectricConnectorDirection.Left:
                    case GVElectricConnectorDirection.In: return GVElectricConnectorType.Input;
                    case GVElectricConnectorDirection.Top:
                    case GVElectricConnectorDirection.Bottom: return GVElectricConnectorType.Output;
                }
            }
            return null;
        }

        public override void GetDropValues(SubsystemTerrain subsystemTerrain, int oldValue, int newValue, int toolLevel, List<BlockDropValue> dropValues, out bool showDebris) {
            dropValues.Add(new BlockDropValue { Value = Terrain.MakeBlockValue(Index, 0, Terrain.ExtractData(oldValue) & 0x1FFE0), Count = 1 });
            showDebris = true;
        }

        public override bool IsNonDuplicable_(int value) => ((Terrain.ExtractData(value) >> 5) & 4095) > 0;

        public int GetCustomCopyBlock(Project project, int centerValue) {
            SubsystemGVCounterBlockBehavior subsystem = project.FindSubsystem<SubsystemGVCounterBlockBehavior>(true);
            int id = subsystem.GetIdFromValue(centerValue);
            return id == 0 ? centerValue : subsystem.SetIdToValue(centerValue, subsystem.StoreItemDataAtUniqueId((GVCounterData)subsystem.GetItemData(id).Copy()));
        }
    }
}