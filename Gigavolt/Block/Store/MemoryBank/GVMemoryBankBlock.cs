using System.Collections.Generic;
using GameEntitySystem;

namespace Game {
    public class GVMemoryBankBlock : RotateableMountedGVElectricElementBlock, IGVCustomWheelPanelBlock {
        public const int Index = 847;

        public GVMemoryBankBlock() : base("Models/Gates", "MemoryBank", 0.875f) { }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z, uint subterrainId) => new MemoryBankGVElectricElement(subsystemGVElectricity, new GVCellFace(x, y, z, GetFace(value)), value, subterrainId);

        public override GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem, int value, int face, int connectorFace, int x, int y, int z, uint subterrainId) {
            int data = Terrain.ExtractData(value);
            if (GetFace(value) == face) {
                GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(GetFace(value), GetRotation(data), connectorFace);
                switch (connectorDirection) {
                    case GVElectricConnectorDirection.Right:
                    case GVElectricConnectorDirection.Left:
                    case GVElectricConnectorDirection.Bottom:
                    case GVElectricConnectorDirection.In: return GVElectricConnectorType.Input;
                    case GVElectricConnectorDirection.Top: return GVElectricConnectorType.Output;
                }
            }
            return null;
        }

        public List<int> GetCustomWheelPanelValues(int centerValue) => IGVCustomWheelPanelBlock.MemoryBankValues;

        public virtual int GetCustomCopyBlock(Project project, int centerValue) {
            SubsystemGVMemoryBankBlockBehavior subsystem = project.FindSubsystem<SubsystemGVMemoryBankBlockBehavior>(true);
            int id = subsystem.GetIdFromValue(centerValue);
            return id == 0 ? centerValue : subsystem.SetIdToValue(centerValue, subsystem.StoreItemDataAtUniqueId((GVMemoryBankData)subsystem.GetItemData(id).Copy()));
        }
    }
}