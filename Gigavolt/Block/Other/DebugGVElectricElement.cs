using System;

namespace Game {
    public class DebugGVElectricElement : GVElectricElement {
        public uint m_voltage;

        public DebugGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace) : base(subsystemGVElectricity, cellFace) {
            subsystemGVElectricity.Project.FindSubsystem<SubsystemGVDebugBlockBehavior>(true).m_elementHashSet.Add(this);
            m_voltage = Double2Uint(subsystemGVElectricity.SpeedFactor);
        }

        public override void OnRemoved() {
            base.OnRemoved();
            SubsystemGVElectricity.Project.FindSubsystem<SubsystemGVDebugBlockBehavior>(true).m_elementHashSet.Remove(this);
        }

        public override uint GetOutputVoltage(int face) => Double2Uint(SubsystemGVElectricity.SpeedFactor);

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
            uint voltage = m_voltage;
            m_voltage = Double2Uint(SubsystemGVElectricity.SpeedFactor);
            return m_voltage != voltage;
        }

        public static uint Double2Uint(double num) => num > 0 ? (((uint)Math.Truncate(num) & 0xffff) << 16) | (uint)Math.Round(num % 1 * 0xffff) : 0u;
    }
}