using Engine;

namespace Game {
    public abstract class RotateableGVElectricElement : MountedGVElectricElement {
        public int Rotation {
            get {
                GVCellFace cellFace = CellFaces[0];
                return RotateableMountedGVElectricElementBlock.GetRotation(Terrain.ExtractData(SubsystemGVElectricity.SubsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z)));
            }
            set {
                GVCellFace cellFace = CellFaces[0];
                int cellValue = SubsystemGVElectricity.SubsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z);
                int value2 = Terrain.ReplaceData(cellValue, RotateableMountedGVElectricElementBlock.SetRotation(Terrain.ExtractData(cellValue), value % 4));
                SubsystemGVElectricity.SubsystemTerrain.ChangeCell(cellFace.X, cellFace.Y, cellFace.Z, value2);
                SubsystemGVElectricity.SubsystemAudio.PlaySound(
                    "Audio/Click",
                    1f,
                    0f,
                    new Vector3(cellFace.X, cellFace.Y, cellFace.Z),
                    2f,
                    true
                );
            }
        }

        public RotateableGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace) : base(subsystemGVElectricity, cellFace) { }
        public RotateableGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace) : base(subsystemGVElectricity, cellFace) { }
        public RotateableGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace[] cellFaces) : base(subsystemGVElectricity, cellFaces) { }

        public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner) {
            ++Rotation;
            return true;
        }
    }
}