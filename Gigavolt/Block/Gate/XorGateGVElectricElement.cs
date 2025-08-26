using System.Collections.Generic;
using Engine;

namespace Game {
    public class XorGateGVElectricElement : RotateableGVElectricElement {
        public uint m_voltage;

        public XorGateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, uint subterrainId) : base(
            subsystemGVElectricity,
            cellFace,
            subterrainId
        ) { }

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override bool Simulate() {
            uint voltage = m_voltage;
            uint? num = null;
            HashSet<Point3> points = [];
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0
                    && points.Add(connection.NeighborCellFace.Point)) {
                    uint num2 = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                    num = num.HasValue ? num ^ num2 : num2;
                }
            }
            m_voltage = num ?? 0u;
            return m_voltage != voltage;
        }
    }
}