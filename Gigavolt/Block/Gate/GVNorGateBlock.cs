using System.Collections.Generic;

namespace Game {
    public class GVNorGateBlock : RotateableMountedGVElectricElementBlock, IGVCustomWheelPanelBlock {
        public const int Index = 806;

        public GVNorGateBlock() : base("Models/Gates", "NorGate", 0.375f) { }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity,
            int value,
            int x,
            int y,
            int z,
            uint subterrainId) => new NorGateGVElectricElement(subsystemGVElectricity, new GVCellFace(x, y, z, GetFace(value)), subterrainId);

        public override GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem,
            int value,
            int face,
            int connectorFace,
            int x,
            int y,
            int z,
            Terrain terrain) {
            int data = Terrain.ExtractData(value);
            if (GetFace(value) == face) {
                GVElectricConnectorDirection? connectorDirection =
                    SubsystemGVElectricity.GetConnectorDirection(GetFace(value), GetRotation(data), connectorFace);
                if (connectorDirection == GVElectricConnectorDirection.Right
                    || connectorDirection == GVElectricConnectorDirection.Left) {
                    return GVElectricConnectorType.Input;
                }
                if (connectorDirection == GVElectricConnectorDirection.Top
                    || connectorDirection == GVElectricConnectorDirection.In) {
                    return GVElectricConnectorType.Output;
                }
            }
            return null;
        }

        public List<int> GetCustomWheelPanelValues(int centerValue) => IGVCustomWheelPanelBlock.BasicElementsValues;
    }
}