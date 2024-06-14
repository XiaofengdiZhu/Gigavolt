using Engine;

namespace Game {
    public class SwitchGVElectricElement : MountedGVElectricElement {
        public SubsystemGVSwitchBlockBehavior m_subsystemGVSwitchBlockBehavior;
        public uint m_voltage;
        public bool m_edited;

        public SwitchGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, int value, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) {
            m_subsystemGVSwitchBlockBehavior = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVSwitchBlockBehavior>(true);
            m_voltage = GVSwitchBlock.GetLeverState(value) ? m_subsystemGVSwitchBlockBehavior.GetItemData(cellFace.Point)?.Data ?? uint.MaxValue : 0;
        }

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner) {
            Switch();
            return true;
        }

        public void Switch() {
            GVCellFace cellFace = CellFaces[0];
            int cellValue = SubsystemGVElectricity.SubsystemGVSubterrain.GetTerrain(SubterrainId).GetCellValue(cellFace.X, cellFace.Y, cellFace.Z);
            int value = GVSwitchBlock.SetLeverState(cellValue, !GVSwitchBlock.GetLeverState(cellValue));
            SubsystemGVElectricity.SubsystemGVSubterrain.ChangeCell(
                cellFace.X,
                cellFace.Y,
                cellFace.Z,
                SubterrainId,
                value
            );
            SubsystemGVElectricity.SubsystemAudio.PlaySound(
                "Audio/Click",
                1f,
                0f,
                new Vector3(cellFace.X, cellFace.Y, cellFace.Z),
                2f,
                true
            );
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