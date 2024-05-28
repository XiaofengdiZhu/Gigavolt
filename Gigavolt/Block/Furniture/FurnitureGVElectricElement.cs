using System.Collections.Generic;
using Engine;

namespace Game {
    public abstract class FurnitureGVElectricElement : GVElectricElement {
        public FurnitureGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, Point3 point) : base(subsystemGVElectricity, GetMountingCellFaces(subsystemGVElectricity, point)) { }

        public static IEnumerable<CellFace> GetMountingCellFaces(SubsystemGVElectricity subsystemGVElectricity, Point3 point) {
            int data = Terrain.ExtractData(subsystemGVElectricity.SubsystemTerrain.Terrain.GetCellValue(point.X, point.Y, point.Z));
            int rotation = FurnitureBlock.GetRotation(data);
            int designIndex = FurnitureBlock.GetDesignIndex(data);
            FurnitureDesign design = subsystemGVElectricity.SubsystemTerrain.SubsystemFurnitureBlockBehavior.GetDesign(designIndex);
            if (design == null) {
                yield break;
            }
            int face = 0;
            while (face < 6) {
                int num = face < 4 ? (face - rotation + 4) % 4 : face;
                if ((design.MountingFacesMask & (1 << num)) != 0) {
                    yield return new CellFace(point.X, point.Y, point.Z, CellFace.OppositeFace(face));
                }
                int num2 = face + 1;
                face = num2;
            }
        }
    }
}