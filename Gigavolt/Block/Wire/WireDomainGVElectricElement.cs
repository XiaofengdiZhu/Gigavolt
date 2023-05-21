using System.Collections.Generic;
using Engine;

namespace Game {
    public class WireDomainGVElectricElement : GVElectricElement {
        public uint m_voltage;

        public WireDomainGVElectricElement(SubsystemGVElectricity subsystemGVElectric, IEnumerable<CellFace> cellFaces) : base(subsystemGVElectric, cellFaces) { }

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override bool Simulate() {
            uint voltage = m_voltage;
            uint num = 0;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    num |= connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                }
            }
            m_voltage = num;
            return m_voltage != voltage;
        }

        public override void OnNeighborBlockChanged(CellFace cellFace, int neighborX, int neighborY, int neighborZ) {
            int cellValue = SubsystemGVElectricity.SubsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z);
            int num = Terrain.ExtractContents(cellValue);
            if (!(BlocksManager.Blocks[num] is GVWireBlock)) {
                return;
            }
            int wireFacesBitmask = GVWireBlock.GetWireFacesBitmask(cellValue);
            int num2 = wireFacesBitmask;
            if (GVWireBlock.WireExistsOnFace(cellValue, cellFace.Face)) {
                Point3 point = CellFace.FaceToPoint3(cellFace.Face);
                int cellValue2 = SubsystemGVElectricity.SubsystemTerrain.Terrain.GetCellValue(cellFace.X - point.X, cellFace.Y - point.Y, cellFace.Z - point.Z);
                Block block = BlocksManager.Blocks[Terrain.ExtractContents(cellValue2)];
                if (!block.IsCollidable_(cellValue2)
                    || block.IsTransparent_(cellValue2)) {
                    num2 &= ~(1 << cellFace.Face);
                }
            }
            if (num2 == 0) {
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
            else if (num2 != wireFacesBitmask) {
                int newValue = GVWireBlock.SetWireFacesBitmask(cellValue, num2);
                SubsystemGVElectricity.SubsystemTerrain.DestroyCell(
                    0,
                    cellFace.X,
                    cellFace.Y,
                    cellFace.Z,
                    newValue,
                    false,
                    false
                );
            }
        }
    }
}