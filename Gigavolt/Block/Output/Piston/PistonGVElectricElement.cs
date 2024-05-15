using System.Collections.Generic;
using Engine;

namespace Game {
    public class PistonGVElectricElement : GVElectricElement {
        public SubsystemGVPistonBlockBehavior m_subsystemGVPistonBlockBehavior;
        public int m_lastLength = -1;

        public PistonGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, Point3 point) : base(
            subsystemGVElectricity,
            new List<CellFace> {
                new(point.X, point.Y, point.Z, 0),
                new(point.X, point.Y, point.Z, 1),
                new(point.X, point.Y, point.Z, 2),
                new(point.X, point.Y, point.Z, 3),
                new(point.X, point.Y, point.Z, 4),
                new(point.X, point.Y, point.Z, 5)
            }
        ) => m_subsystemGVPistonBlockBehavior = SubsystemGVElectricity.Project.FindSubsystem<SubsystemGVPistonBlockBehavior>(true);

        public override bool Simulate() {
            uint num = 0u;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    num = MathUint.Max(num, connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace));
                }
            }
            int num2 = MathUint.ToIntWithClamp(num);
            if (num2 != m_lastLength) {
                m_lastLength = num2;
                m_subsystemGVPistonBlockBehavior.AdjustPiston(CellFaces[0].Point, num2);
            }
            return false;
        }
    }
}