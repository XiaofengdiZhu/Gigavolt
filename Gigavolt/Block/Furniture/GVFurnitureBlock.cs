using Engine;

namespace Game {
    public class GVFurnitureBlock : IGVElectricElementBlock {
        public GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z) {
            int designIndex = FurnitureBlock.GetDesignIndex(Terrain.ExtractData(value));
            FurnitureDesign design = subsystemGVElectricity.SubsystemTerrain.SubsystemFurnitureBlockBehavior.GetDesign(designIndex);
            if (design != null
                && design.MountingFacesMask != 0) {
                switch (design.InteractionMode) {
                    case FurnitureInteractionMode.Multistate:
                    case FurnitureInteractionMode.ConnectedMultistate: return new MultistateFurnitureGVElectricElement(subsystemGVElectricity, new Point3(x, y, z));
                    case FurnitureInteractionMode.ElectricButton: return new ButtonFurnitureGVElectricElement(subsystemGVElectricity, new Point3(x, y, z));
                    case FurnitureInteractionMode.ElectricSwitch: return new SwitchFurnitureGVElectricElement(subsystemGVElectricity, new Point3(x, y, z), value);
                }
            }
            return null;
        }

        public GVElectricConnectorType? GetGVConnectorType(SubsystemTerrain terrain, int value, int face, int connectorFace, int x, int y, int z) {
            int data = Terrain.ExtractData(value);
            int rotation = FurnitureBlock.GetRotation(data);
            int designIndex = FurnitureBlock.GetDesignIndex(data);
            FurnitureDesign design = terrain.SubsystemFurnitureBlockBehavior.GetDesign(designIndex);
            if (design != null) {
                int num = CellFace.OppositeFace(face < 4 ? (face - rotation + 4) % 4 : face);
                if ((design.MountingFacesMask & (1 << num)) != 0
                    && SubsystemGVElectricity.GetConnectorDirection(face, 0, connectorFace).HasValue) {
                    Point3 point = CellFace.FaceToPoint3(face);
                    int cellValue = terrain.Terrain.GetCellValue(x - point.X, y - point.Y, z - point.Z);
                    if (!BlocksManager.Blocks[Terrain.ExtractContents(cellValue)].IsFaceTransparent(terrain, CellFace.OppositeFace(num), cellValue)) {
                        switch (design.InteractionMode) {
                            case FurnitureInteractionMode.Multistate:
                            case FurnitureInteractionMode.ConnectedMultistate: return GVElectricConnectorType.Input;
                            case FurnitureInteractionMode.ElectricButton:
                            case FurnitureInteractionMode.ElectricSwitch: return GVElectricConnectorType.Output;
                        }
                    }
                }
            }
            return null;
        }

        public int GetConnectionMask(int value) => int.MaxValue;
    }
}