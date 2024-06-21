using System.Collections.Generic;

namespace Game {
    public class GVSRLatchBlock : RotateableMountedGVElectricElementBlock {
        public const int Index = 845;

        public GVSRLatchBlock() : base("Models/Gates", "SRLatch", 0.375f) { }
        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value) => LanguageControl.Get(GetType().Name, "DisplayName") + (GetClassic(Terrain.ExtractData(value)) ? LanguageControl.Get("Gigavolt", "ClassicName") : "");
        public override string GetDescription(int value) => GetClassic(Terrain.ExtractData(value)) ? LanguageControl.Get(GetType().Name, "ClassicDescription") : LanguageControl.Get(GetType().Name, "Description");
        public override string GetCategory(int value) => GetClassic(Terrain.ExtractData(value)) ? "GV Electrics Regular" : "GV Electrics Shift";
        public override int GetDisplayOrder(int value) => GetClassic(Terrain.ExtractData(value)) ? 10 : 5;

        public override IEnumerable<int> GetCreativeValues() => [Terrain.MakeBlockValue(Index, 0, SetClassic(0, false)), Terrain.MakeBlockValue(Index, 0, SetClassic(0, true))];

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z, uint subterrainId) => new SRLatchGVElectricElement(subsystemGVElectricity, new GVCellFace(x, y, z, GetFace(value)), subterrainId, GetClassic(Terrain.ExtractData(value)));

        public override GVElectricConnectorType? GetGVConnectorType(SubsystemGVSubterrain subsystem, int value, int face, int connectorFace, int x, int y, int z, uint subterrainId) {
            int data = Terrain.ExtractData(value);
            if (GetFace(value) == face) {
                GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(GetFace(value), GetRotation(data), connectorFace);
                if (connectorDirection == GVElectricConnectorDirection.Right
                    || connectorDirection == GVElectricConnectorDirection.Left
                    || connectorDirection == GVElectricConnectorDirection.Bottom) {
                    return GVElectricConnectorType.Input;
                }
                if (connectorDirection == GVElectricConnectorDirection.Top
                    || connectorDirection == GVElectricConnectorDirection.In) {
                    return GVElectricConnectorType.Output;
                }
            }
            return null;
        }

        public static bool GetClassic(int data) => (data & 32) != 0;
        public static int SetClassic(int data, bool classic) => (data & -33) | (classic ? 32 : 0);
    }
}