namespace Game {
    public class BatteryGVElectricElement : GVElectricElement {
        public SubsystemGVBatteryBlockBehavior m_subsystemGVBatteryBlockBehavior;
        public uint m_voltage;
        public bool m_edited;

        public BatteryGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace) : base(subsystemGVElectricity, cellFace) {
            m_subsystemGVBatteryBlockBehavior = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVBatteryBlockBehavior>(true);
            GigaVoltageLevelData blockdata = m_subsystemGVBatteryBlockBehavior.GetBlockData(cellFace.Point);
            m_voltage = blockdata?.Data ?? uint.MaxValue;
        }

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override void OnNeighborBlockChanged(CellFace cellFace, int neighborX, int neighborY, int neighborZ) {
            int cellValue = SubsystemGVElectricity.SubsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y - 1, cellFace.Z);
            Block block = BlocksManager.Blocks[Terrain.ExtractContents(cellValue)];
            if (!block.IsCollidable_(cellValue)
                || block.IsTransparent_(cellValue)) {
                SubsystemGVElectricity.SubsystemTerrain.DestroyCell(
                    0,
                    cellFace.X,
                    cellFace.Y,
                    cellFace.Z,
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
            GigaVoltageLevelData blockdata = m_subsystemGVBatteryBlockBehavior.GetBlockData(CellFaces[0].Point);
            m_voltage = blockdata?.Data ?? uint.MaxValue;
            if (voltage != m_voltage) {
                SubsystemGVElectricity.QueueGVElectricElementForSimulation(this, SubsystemGVElectricity.CircuitStep + 10);
                return true;
            }
            return false;
        }
    }
}