using System.Collections.Generic;

namespace Game {
    public class GVRealTimeClockBlock : RotateableMountedGVElectricElementBlock {
        public const int Index = 849;

        public GVRealTimeClockBlock() : base("Models/Gates", "RealTimeClock", 0.5f) { }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z, uint subterrainId) => new RealTimeClockGVElectricElement(subsystemGVElectricity, new GVCellFace(x, y, z, GetFace(value)), subterrainId, GetClassic(Terrain.ExtractData(value)));

        public override GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem, int value, int face, int connectorFace, int x, int y, int z, uint subterrainId) {
            int data = Terrain.ExtractData(value);
            if (GetFace(value) == face) {
                GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(GetFace(value), GetRotation(data), connectorFace);
                if (connectorDirection == GVElectricConnectorDirection.Top
                    || connectorDirection == GVElectricConnectorDirection.Right
                    || connectorDirection == GVElectricConnectorDirection.Left
                    || connectorDirection == GVElectricConnectorDirection.Bottom) {
                    return GVElectricConnectorType.Output;
                }
                if (connectorDirection == GVElectricConnectorDirection.In) {
                    return GetClassic(data) ? GVElectricConnectorType.Output : GVElectricConnectorType.Input;
                }
            }
            return null;
        }

        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value) => LanguageControl.Get(GetType().Name, GetClassic(Terrain.ExtractData(value)) ? "ClassicDisplayName" : "DisplayName");
        public override string GetDescription(int value) => LanguageControl.Get(GetType().Name, GetClassic(Terrain.ExtractData(value)) ? "ClassicDescription" : "Description");
        public override string GetCategory(int value) => GetClassic(Terrain.ExtractData(value)) ? "GV Electrics Regular" : "GV Electrics Shift";
        public override int GetDisplayOrder(int value) => GetClassic(Terrain.ExtractData(value)) ? 14 : 12;
        public override IEnumerable<int> GetCreativeValues() => [Index, Terrain.MakeBlockValue(Index, 0, SetClassic(0, true))];
        public static bool GetClassic(int data) => (data & 32) != 0;
        public static int SetClassic(int data, bool classic) => (data & -33) | (classic ? 32 : 0);
    }
}