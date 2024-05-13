using Engine;

namespace Game {
    public class SubsystemGVSignalGeneratorBlockBehavior : SubsystemBlockBehavior {
        public override int[] HandledBlocks => new[] { GVSignalGeneratorBlock.Index };

        public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner) {
            CellFace cellFace = raycastResult.CellFace;
            int cellValue = SubsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z);
            int data = Terrain.ExtractData(cellValue);
            return true;
        }

        public override void OnBlockAdded(int value, int oldValue, int x, int y, int z) {
            int data = Terrain.ExtractData(SubsystemTerrain.Terrain.GetCellValue(x, y, z));
            if (!GVSignalGeneratorBlock.GetIsTopPart(data)) {
                int face = RotateableMountedGVElectricElementBlock.GetFaceFromDataStatic(data);
                Point3 up = GVSignalGeneratorBlock.m_upPoint3[face * 4 + RotateableMountedGVElectricElementBlock.GetRotation(data)] + new Point3(x, y, z);
                if (Terrain.ExtractContents(SubsystemTerrain.Terrain.GetCellValue(up.X, up.Y, up.Z)) == 0) {
                    Point3 faceDirection = -CellFace.FaceToPoint3(face);
                    int faceValue = SubsystemTerrain.Terrain.GetCellValue(up.X + faceDirection.X, up.Y + faceDirection.Y, up.Z + faceDirection.Z);
                    Block block = BlocksManager.Blocks[Terrain.ExtractContents(faceValue)];
                    if ((block.IsCollidable_(faceValue) && !block.IsFaceTransparent(SubsystemTerrain, face, faceValue))
                        || (face == 4 && block is FenceBlock)) {
                        SubsystemTerrain.ChangeCell(up.X, up.Y, up.Z, Terrain.MakeBlockValue(GVSignalGeneratorBlock.Index, 0, GVSignalGeneratorBlock.SetIsTopPart(data, true)));
                        return;
                    }
                }
                SubsystemTerrain.DestroyCell(
                    int.MaxValue,
                    x,
                    y,
                    z,
                    0,
                    false,
                    false
                );
            }
        }

        public override void OnBlockRemoved(int value, int newValue, int x, int y, int z) {
            int data = Terrain.ExtractData(value);
            int face = RotateableMountedGVElectricElementBlock.GetFaceFromDataStatic(data);
            int rotation = RotateableMountedGVElectricElementBlock.GetRotation(data);
            Point3 upDirection = GVSignalGeneratorBlock.m_upPoint3[face * 4 + rotation];
            bool isUp = GVSignalGeneratorBlock.GetIsTopPart(data);
            Point3 another = new Point3(x, y, z) + upDirection * (isUp ? -1 : 1);
            int anotherData = Terrain.ExtractData(SubsystemTerrain.Terrain.GetCellValue(another.X, another.Y, another.Z));
            if (GVSignalGeneratorBlock.GetIsTopPart(anotherData) != isUp
                && RotateableMountedGVElectricElementBlock.GetFaceFromDataStatic(anotherData) == face
                && RotateableMountedGVElectricElementBlock.GetRotation(anotherData) == rotation) {
                SubsystemTerrain.ChangeCell(another.X, another.Y, another.Z, 0);
            }
        }
    }
}