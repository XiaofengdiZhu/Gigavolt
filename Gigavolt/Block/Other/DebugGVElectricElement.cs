using System;

namespace Game {
    public class DebugGVElectricElement : GVElectricElement {
        public uint m_voltage;

        public DebugGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) {
            subsystemGVElectricity.Project.FindSubsystem<SubsystemGVDebugBlockBehavior>(true).m_elementHashSet.Add(this);
            m_voltage = Double2Uint(subsystemGVElectricity.SpeedFactor);
        }

        public override void OnRemoved() {
            base.OnRemoved();
            SubsystemGVElectricity.Project.FindSubsystem<SubsystemGVDebugBlockBehavior>(true).m_elementHashSet.Remove(this);
        }

        public override uint GetOutputVoltage(int face) => Double2Uint(SubsystemGVElectricity.SpeedFactor);

        public override void OnNeighborBlockChanged(CellFace cellFace, int neighborX, int neighborY, int neighborZ) {
            int cellValue = SubsystemGVElectricity.SubsystemGVSubterrain.GetTerrain(SubterrainId).GetCellValue(cellFace.X, cellFace.Y - 1, cellFace.Z);
            Block block = BlocksManager.Blocks[Terrain.ExtractContents(cellValue)];
            if (!block.IsFaceSuitableForElectricElements(SubsystemGVElectricity.SubsystemTerrain, cellFace, cellValue)
                && (cellFace.Face != 4 || block is not FenceBlock)) {
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
            uint voltage = m_voltage;
            m_voltage = Double2Uint(SubsystemGVElectricity.SpeedFactor);
            return m_voltage != voltage;
        }

        public static uint Double2Uint(double num) => num > 0 ? (((uint)Math.Truncate(num) & 0xffff) << 16) | (uint)Math.Round(num % 1 * 0xffff) : 0u;
    }
}