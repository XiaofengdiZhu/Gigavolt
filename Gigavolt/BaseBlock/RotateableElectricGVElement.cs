using Engine;

namespace Game
{
    public abstract class RotateableGVElectricElement : MountedGVElectricElement
    {
        public int Rotation
        {
            get
            {
                CellFace cellFace = CellFaces[0];
                return RotateableMountedGVElectricElementBlock.GetRotation(Terrain.ExtractData(SubsystemGVElectricity.SubsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z)));
            }
            set
            {
                CellFace cellFace = CellFaces[0];
                int cellValue = SubsystemGVElectricity.SubsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z);
                int value2 = Terrain.ReplaceData(cellValue, RotateableMountedGVElectricElementBlock.SetRotation(Terrain.ExtractData(cellValue), value % 4));
                SubsystemGVElectricity.SubsystemTerrain.ChangeCell(cellFace.X, cellFace.Y, cellFace.Z, value2);
                SubsystemGVElectricity.SubsystemAudio.PlaySound("Audio/Click", 1f, 0f, new Vector3(cellFace.X, cellFace.Y, cellFace.Z), 2f, autoDelay: true);
            }
        }

        public RotateableGVElectricElement(SubsystemGVElectricity subsystemGVElectric, CellFace cellFace)
            : base(subsystemGVElectric, cellFace)
        {
        }

        public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner)
        {
            int num = ++Rotation;
            return true;
        }
    }
}