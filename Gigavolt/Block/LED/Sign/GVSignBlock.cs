using Engine;

namespace Game {
    public class GVSignBlock : GVAttachedSignCBlock {
        public const int Index = 862;

        public GVSignBlock() : base("Models/IronSign", 78, Index) { }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z, uint subterrainId) => new SignGVElectricElement(subsystemGVElectricity, new GVCellFace(x, y, z, GetFace(Terrain.ExtractData(value))), subterrainId);

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

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) {
            int data = Terrain.ExtractData(value);
            int face = GetFace(data);
            int? color = GetColor(data);
            if (color.HasValue) {
                generator.GenerateMeshVertices(
                    this,
                    x,
                    y,
                    z,
                    m_coloredBlockMeshes[face],
                    SubsystemPalette.GetColor(generator, color),
                    null,
                    geometry.SubsetOpaque
                );
            }
            else {
                generator.GenerateMeshVertices(
                    this,
                    x,
                    y,
                    z,
                    m_blockMeshes[face],
                    new Color(204, 204, 204, 255),
                    null,
                    geometry.SubsetOpaque
                );
            }
            GVBlockGeometryGenerator.GenerateGVWireVertices(
                generator,
                value,
                x,
                y,
                z,
                GetFace(data),
                0.375f,
                Vector2.Zero,
                geometry.SubsetOpaque
            );
        }
    }
}