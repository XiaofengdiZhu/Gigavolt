namespace Game {
    public class BatteryGVElectricElement : GVElectricElement {
        public SubsystemGVBatteryBlockBehavior m_subsystemGVBatteryBlockBehavior;
        readonly GigaVoltageLevelData m_blockdata;
        public uint m_voltage;
        public bool m_edited;

        public BatteryGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, GVCellFace cellFace, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) {
            m_subsystemGVBatteryBlockBehavior = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVBatteryBlockBehavior>(true);
            m_blockdata = m_subsystemGVBatteryBlockBehavior.GetItemData(m_subsystemGVBatteryBlockBehavior.GetIdFromValue(value), true);
            m_voltage = m_blockdata?.Data ?? uint.MaxValue;
        }

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override void OnNeighborBlockChanged(CellFace cellFace, int neighborX, int neighborY, int neighborZ) {
            int cellValue = SubsystemGVElectricity.SubsystemGVSubterrain.GetTerrain(SubterrainId).GetCellValue(cellFace.X, cellFace.Y - 1, cellFace.Z);
            Block block = BlocksManager.Blocks[Terrain.ExtractContents(cellValue)];
            if (!block.IsCollidable_(cellValue)
                || block.IsTransparent_(cellValue)) {
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

        public override bool Simulate() {
            if (m_edited) {
                m_edited = false;
                return true;
            }
            uint voltage = m_voltage;
            m_voltage = m_blockdata?.Data ?? uint.MaxValue;
            if (voltage != m_voltage) {
                SubsystemGVElectricity.QueueGVElectricElementForSimulation(this, SubsystemGVElectricity.CircuitStep + 10);
                return true;
            }
            return false;
        }
    }
}