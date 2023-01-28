using Engine;

namespace Game
{
    public class BatteryGVElectricElement : GVElectricElement
    {
        public SubsystemGVBatteryBlockBehavior m_subsystemGVBatteryBlockBehavior;
        uint m_voltage;
        public BatteryGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace)
            : base(subsystemGVElectricity, cellFace)
        {
            m_subsystemGVBatteryBlockBehavior = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVBatteryBlockBehavior>(throwOnError: true);
            GigaVoltageLevelData blockdata = m_subsystemGVBatteryBlockBehavior.GetBlockData(cellFace.Point);
            m_voltage = blockdata == null ? uint.MaxValue : blockdata.Data;
        }

        public override uint GetOutputVoltage(int face)
        {
            return m_voltage;
        }

        public override void OnNeighborBlockChanged(CellFace cellFace, int neighborX, int neighborY, int neighborZ)
        {
            int cellValue = SubsystemGVElectricity.SubsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y - 1, cellFace.Z);
            Block block = BlocksManager.Blocks[Terrain.ExtractContents(cellValue)];
            if (!block.IsCollidable_(cellValue) || block.IsTransparent_(cellValue))
            {
                SubsystemGVElectricity.SubsystemTerrain.DestroyCell(0, cellFace.X, cellFace.Y, cellFace.Z, 0, noDrop: false, noParticleSystem: false);
            }
        }
        public override bool Simulate()
        {
            uint voltage = m_voltage;
            GigaVoltageLevelData blockdata = m_subsystemGVBatteryBlockBehavior.GetBlockData(CellFaces[0].Point);
            m_voltage = blockdata == null ? uint.MaxValue : blockdata.Data;
            if (voltage != m_voltage)
            {
                base.SubsystemGVElectricity.QueueGVElectricElementForSimulation(this, base.SubsystemGVElectricity.CircuitStep + 10);
                return true;
            }
            return false;
        }
    }
}
