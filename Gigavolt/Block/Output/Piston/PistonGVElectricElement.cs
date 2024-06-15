using System.Collections.Generic;
using Engine;

namespace Game {
    public class PistonGVElectricElement : GVElectricElement {
        public SubsystemGVPistonBlockBehavior m_subsystemGVPistonBlockBehavior;
        public int m_lastLength = -1;
        public uint m_lastInput;
        public readonly bool m_complex;
        public GVPistonData m_pistonData;

        public PistonGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, Point3 point, uint subterrainId, bool complex) : base(
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
        ) {
            m_subsystemGVPistonBlockBehavior = SubsystemGVElectricity.Project.FindSubsystem<SubsystemGVPistonBlockBehavior>(true);
            m_complex = complex;
            if (complex) {
                m_pistonData = new GVPistonData { MaxExtension = 0xFF };
                m_subsystemGVPistonBlockBehavior.m_complexPistonData[point] = m_pistonData;
            }
        }

        public override bool Simulate() {
            if (SubterrainId != 0) {
                return false;
            }
            uint input = 0u;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    input |= connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                }
            }
            if (m_complex) {
                if (m_lastInput != input) {
                    m_lastInput = input;
                    m_pistonData.Speed = (int)((input >> 8) & 0xFFu);
                    m_pistonData.PullCount = (int)((input >> 16) & 0xFFu) - 1;
                    m_pistonData.Pulling = ((input >> 24) & 1u) == 1u;
                    m_pistonData.Strict = ((input >> 25) & 1u) == 1u;
                    m_pistonData.Transparent = ((input >> 26) & 1u) == 1u;
                    m_subsystemGVPistonBlockBehavior.AdjustPiston(CellFaces[0].Point, (int)(input & 0xFFu), m_pistonData);
                }
            }
            else {
                int length = MathUint.ToIntWithClamp(input);
                if (length != m_lastLength) {
                    m_lastLength = length;
                    m_subsystemGVPistonBlockBehavior.AdjustPiston(CellFaces[0].Point, length, null);
                }
            }
            return false;
        }
    }
}