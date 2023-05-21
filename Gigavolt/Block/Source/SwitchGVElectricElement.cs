using Engine;

namespace Game {
    public class SwitchGVElectricElement : MountedGVElectricElement {
        public SubsystemGVSwitchBlockBehavior m_subsystemGVSwitchBlockBehavior;
        public uint m_voltage;

        public SwitchGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace, int value) : base(subsystemGVElectricity, cellFace) {
            m_subsystemGVSwitchBlockBehavior = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVSwitchBlockBehavior>(true);
            GigaVoltageLevelData blockData = m_subsystemGVSwitchBlockBehavior.GetBlockData(cellFace.Point);
            m_voltage = GVSwitchBlock.GetLeverState(value) ? blockData == null ? uint.MaxValue : blockData.Data : 0;
        }

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner) {
            CellFace cellFace = CellFaces[0];
            int cellValue = SubsystemGVElectricity.SubsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z);
            int value = GVSwitchBlock.SetLeverState(cellValue, !GVSwitchBlock.GetLeverState(cellValue));
            SubsystemGVElectricity.SubsystemTerrain.ChangeCell(cellFace.X, cellFace.Y, cellFace.Z, value);
            SubsystemGVElectricity.SubsystemAudio.PlaySound(
                "Audio/Click",
                1f,
                0f,
                new Vector3(cellFace.X, cellFace.Y, cellFace.Z),
                2f,
                true
            );
            return true;
        }
    }
}