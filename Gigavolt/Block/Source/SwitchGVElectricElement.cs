using Engine;

namespace Game {
    public class SwitchGVElectricElement : MountedGVElectricElement {
        public readonly uint m_voltage;
        public bool m_edited;

        public SwitchGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, int value, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) {
            SubsystemGVSwitchBlockBehavior subsystemGVSwitchBlockBehavior = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVSwitchBlockBehavior>(true);
            m_voltage = GVSwitchBlock.GetLeverState(value) ? subsystemGVSwitchBlockBehavior.GetItemData(subsystemGVSwitchBlockBehavior.GetIdFromValue(value))?.Data ?? uint.MaxValue : 0;
        }

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner) {
            Switch();
            return true;
        }

        public void Switch() {
            GVCellFace cellFace = CellFaces[0];
            Vector3 position = new(cellFace.X + 0.5f, cellFace.Y + 0.5f, cellFace.Z + 0.5f);
            if (SubterrainId == 0) {
                int cellValue = SubsystemGVElectricity.SubsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z);
                SubsystemGVElectricity.SubsystemTerrain.ChangeCell(cellFace.X, cellFace.Y, cellFace.Z, GVSwitchBlock.SetLeverState(cellValue, !GVSwitchBlock.GetLeverState(cellValue)));
                SubsystemGVElectricity.SubsystemAudio.PlaySound(
                    "Audio/Click",
                    1f,
                    0f,
                    position,
                    2f,
                    true
                );
            }
            else {
                GVSubterrainSystem subterrainSystem = GVStaticStorage.GVSubterrainSystemDictionary[SubterrainId];
                int cellValue = subterrainSystem.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z);
                subterrainSystem.ChangeCell(cellFace.X, cellFace.Y, cellFace.Z, GVSwitchBlock.SetLeverState(cellValue, !GVSwitchBlock.GetLeverState(cellValue)));
                SubsystemGVElectricity.SubsystemAudio.PlaySound(
                    "Audio/Click",
                    1f,
                    0f,
                    Vector3.Transform(position, subterrainSystem.GlobalTransform),
                    2f,
                    true
                );
            }
        }

        public override bool Simulate() {
            if (m_edited) {
                m_edited = false;
                return true;
            }
            return false;
        }
    }
}