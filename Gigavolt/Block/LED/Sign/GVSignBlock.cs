namespace Game {
    public class GVSignBlock : GVAttachedSignCBlock {
        public const int Index = 801;

        public GVSignBlock() : base("Models/IronSign", 78, Index) { }

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain, ComponentMiner componentMiner, int value, TerrainRaycastResult raycastResult) {
            int? color = GetColor(Terrain.ExtractData(value));
            if (raycastResult.CellFace.Face < 4) {
                int data = SetFace(SetColor(0, color), raycastResult.CellFace.Face);
                BlockPlacementData result = default;
                result.Value = Terrain.MakeBlockValue(Index, 0, data);
                result.CellFace = raycastResult.CellFace;
                return result;
            }
            return default;
        }
    }
}