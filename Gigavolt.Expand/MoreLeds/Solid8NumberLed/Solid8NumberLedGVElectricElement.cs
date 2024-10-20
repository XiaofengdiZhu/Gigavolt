using Engine;

namespace Game {
    public class Solid8NumberLedGVElectricElement : GVElectricElement {
        public readonly SubsystemGVSolid8NumberLedGlow m_subsystemGV8NumberLedGlow;

        public uint m_voltage;

        public GVSolid8NumberGlowPoint m_glowPoint;

        public Solid8NumberLedGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, Point3 point, uint subterrainId) : base(
            subsystemGVElectricity,
            [
                new GVCellFace(point.X, point.Y, point.Z, 0),
                new GVCellFace(point.X, point.Y, point.Z, 1),
                new GVCellFace(point.X, point.Y, point.Z, 2),
                new GVCellFace(point.X, point.Y, point.Z, 3),
                new GVCellFace(point.X, point.Y, point.Z, 4),
                new GVCellFace(point.X, point.Y, point.Z, 5)
            ],
            subterrainId
        ) => m_subsystemGV8NumberLedGlow = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVSolid8NumberLedGlow>(true);

        public override void OnAdded() {
            m_glowPoint = m_subsystemGV8NumberLedGlow.AddGlowPoint(SubterrainId);
            m_glowPoint.Position = CellFaces[0].Point;
        }

        public override void OnRemoved() {
            m_subsystemGV8NumberLedGlow.RemoveGlowPoint(m_glowPoint, SubterrainId);
        }

        public override bool Simulate() {
            uint voltage = m_voltage;
            m_voltage = 0;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != GVElectricConnectorType.Input) {
                    m_voltage = MathUint.Max(m_voltage, connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace));
                }
            }
            if (m_voltage != voltage) {
                m_glowPoint.Voltage = m_voltage;
            }
            return false;
        }
    }
}