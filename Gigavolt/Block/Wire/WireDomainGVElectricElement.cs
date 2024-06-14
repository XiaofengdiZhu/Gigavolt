using System.Collections.Generic;
using Engine;

namespace Game {
    public class WireDomainGVElectricElement : GVElectricElement {
        public uint m_voltage;

        public WireDomainGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, IEnumerable<GVCellFace> cellFaces, uint subterrainId) : base(subsystemGVElectricity, cellFaces, subterrainId) { }

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override bool Simulate() {
            uint voltage = m_voltage;
            m_voltage = 0;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    if (connection.NeighborGVElectricElement is WireDomainGVElectricElement) {
                        continue;
                    }
                    if (connection.NeighborGVElectricElement.CellFaces[0].Mask != int.MaxValue
                        && (connection.NeighborGVElectricElement.CellFaces[0].Mask & connection.CellFace.Mask) == 0) {
                        continue;
                    }
                    m_voltage |= connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                }
            }
            return m_voltage != voltage;
        }

        public override void OnNeighborBlockChanged(CellFace cellFace, int neighborX, int neighborY, int neighborZ) {
            Terrain terrain = SubsystemGVElectricity.SubsystemGVSubterrain.GetTerrain(SubterrainId);
            int cellValue = terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z);
            int num = Terrain.ExtractContents(cellValue);
            if (BlocksManager.Blocks[num] is not (GVWireBlock or GVWireHarnessBlock)) {
                return;
            }
            int wireFacesBitmask = GVWireBlock.GetWireFacesBitmask(cellValue);
            int num2 = wireFacesBitmask;
            if (GVWireBlock.WireExistsOnFace(cellValue, cellFace.Face)) {
                Point3 point = CellFace.FaceToPoint3(cellFace.Face);
                int cellValue2 = terrain.GetCellValue(cellFace.X - point.X, cellFace.Y - point.Y, cellFace.Z - point.Z);
                Block block = BlocksManager.Blocks[Terrain.ExtractContents(cellValue2)];
                if (!block.IsCollidable_(cellValue2)
                    || block.IsTransparent_(cellValue2)) {
                    num2 &= ~(1 << cellFace.Face);
                }
            }
            if (num2 == 0) {
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
            else if (num2 != wireFacesBitmask) {
                int newValue = GVWireBlock.SetWireFacesBitmask(cellValue, num2);
                SubsystemGVElectricity.SubsystemGVSubterrain.DestroyCell(
                    0,
                    cellFace.X,
                    cellFace.Y,
                    cellFace.Z,
                    SubterrainId,
                    newValue,
                    false,
                    false
                );
            }
        }
    }
}