using Engine;

namespace Game {
    public abstract class MountedGVElectricElement : GVElectricElement {
        public MountedGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, uint subterrainId) : base(
            subsystemGVElectricity,
            cellFace,
            subterrainId
        ) { }

        public MountedGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace[] cellFaces, uint subterrainId) : base(
            subsystemGVElectricity,
            cellFaces,
            subterrainId
        ) { }

        public override void OnNeighborBlockChanged(CellFace cellFace, int neighborX, int neighborY, int neighborZ) {
            Point3 point = CellFace.FaceToPoint3(cellFace.Face);
            int x = cellFace.X - point.X;
            int y = cellFace.Y - point.Y;
            int z = cellFace.Z - point.Z;
            Terrain terrain = SubsystemGVElectricity.SubsystemGVSubterrain.GetTerrain(SubterrainId);
            if (terrain.IsCellValid(x, y, z)) {
                int cellValue = terrain.GetCellValue(x, y, z);
                int elementCellValue = terrain.GetCellValue(point.X, point.Y, point.Z);
                Block block = BlocksManager.Blocks[Terrain.ExtractContents(cellValue)];
                if (block.IsFaceNonAttachable(SubsystemGVElectricity.SubsystemTerrain, cellFace.Face, cellValue, elementCellValue)
                    && (cellFace.Face != 4 || block is not FenceBlock)) {
                    SubsystemGVElectricity.SubsystemGVSubterrain.DestroyCell(
                        0,
                        cellFace.X,
                        cellFace.Y,
                        cellFace.Z,
                        SubterrainId,
                        0,
                        false,
                        false
                    );
                }
            }
        }
    }
}