using System.Collections.Generic;
using Engine;

namespace Game {
    public abstract class FurnitureGVElectricElement : GVElectricElement {
        public FurnitureGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, Point3 point, uint subterrainId) : base(subsystemGVElectricity, GetMountingCellFaces(subsystemGVElectricity, point, subterrainId), subterrainId) { }

        public static IEnumerable<GVCellFace> GetMountingCellFaces(SubsystemGVElectricity subsystemGVElectricity, Point3 point, uint subterrainId) {
            int data = Terrain.ExtractData(subsystemGVElectricity.SubsystemGVSubterrain.GetTerrain(subterrainId).GetCellValue(point.X, point.Y, point.Z));
            int rotation = FurnitureBlock.GetRotation(data);
            int designIndex = FurnitureBlock.GetDesignIndex(data);
            FurnitureDesign design = subsystemGVElectricity.SubsystemTerrain.SubsystemFurnitureBlockBehavior.GetDesign(designIndex);
            if (design == null) {
                yield break;
            }
            for (int face = 0; face < 6; face++) {
                int num = face < 4 ? (face - rotation + 4) % 4 : face;
                if ((design.MountingFacesMask & (1 << num)) != 0) {
                    yield return new GVCellFace(point.X, point.Y, point.Z, GVCellFace.OppositeFace(face));
                }
            }
        }
    }
}