using System.Collections.Generic;

namespace Game {
    public class PistonGVElectricElement : GVElectricElement {
        public readonly SubsystemGVPistonBlockBehavior m_subsystemGVPistonBlockBehavior;
        public int m_lastLength = -1;
        public uint m_lastInput;
        public readonly bool m_complex;
        public readonly GVPistonData m_pistonData;
        public readonly GVPoint3 m_point;

        public PistonGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVPoint3 point, bool complex) : base(
            subsystemGVElectricity,
            new List<GVCellFace> {
                new(point.X, point.Y, point.Z, 0),
                new(point.X, point.Y, point.Z, 1),
                new(point.X, point.Y, point.Z, 2),
                new(point.X, point.Y, point.Z, 3),
                new(point.X, point.Y, point.Z, 4),
                new(point.X, point.Y, point.Z, 5)
            },
            point.SubterrainId
        ) {
            m_subsystemGVPistonBlockBehavior = SubsystemGVElectricity.Project.FindSubsystem<SubsystemGVPistonBlockBehavior>(true);
            m_complex = complex;
            if (complex) {
                m_pistonData = new GVPistonData { MaxExtension = 0xFF };
                m_subsystemGVPistonBlockBehavior.m_complexPistonData[point] = m_pistonData;
            }
            m_point = point;
        }

        public bool m_firstSimulate = true;

        public override bool Simulate() {
            if (m_firstSimulate) {
                m_firstSimulate = false;
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
                    m_subsystemGVPistonBlockBehavior.AdjustPiston(m_point, (int)(input & 0xFFu), m_pistonData);
                }
            }
            else {
                int length = MathUint.ToIntWithClamp(input);
                if (length != m_lastLength) {
                    m_lastLength = length;
                    m_subsystemGVPistonBlockBehavior.AdjustPiston(m_point, length, null);
                }
            }
            return false;
        }
    }
}