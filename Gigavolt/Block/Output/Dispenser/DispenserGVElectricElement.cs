using System.Collections.Generic;
using Engine;

namespace Game {
    public class DispenserGVElectricElement : GVElectricElement {
        public SubsystemBlockEntities m_subsystemBlockEntities;
        public uint m_voltage;

        public DispenserGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, Point3 point, uint subterrainId) : base(
            subsystemGVElectricity,
            new List<GVCellFace> {
                new(point.X, point.Y, point.Z, 0),
                new(point.X, point.Y, point.Z, 1),
                new(point.X, point.Y, point.Z, 2),
                new(point.X, point.Y, point.Z, 3),
                new(point.X, point.Y, point.Z, 4),
                new(point.X, point.Y, point.Z, 5)
            },
            subterrainId
        ) => m_subsystemBlockEntities = SubsystemGVElectricity.Project.FindSubsystem<SubsystemBlockEntities>(true);

        public override bool Simulate() {
            uint voltage = m_voltage;
            m_voltage = 0;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    m_voltage |= connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                }
            }
            if (m_voltage != voltage
                && m_voltage > 0) {
                m_subsystemBlockEntities.GetBlockEntity(CellFaces[0].Point.X, CellFaces[0].Point.Y, CellFaces[0].Point.Z)?.Entity.FindComponent<ComponentGVDispenser>()?.Dispense(m_voltage);
            }
            return false;
        }
    }
}