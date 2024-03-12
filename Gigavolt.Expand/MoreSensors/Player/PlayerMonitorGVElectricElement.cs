using System;
using Engine;

namespace Game {
    public class PlayerMonitorGVElectricElement : RotateableGVElectricElement {
        readonly Terrain m_terrain;
        readonly SubsystemPlayers m_subsystemPlayers;
        uint m_rightOutput;
        uint m_topOutput;
        uint m_leftOutput;
        uint m_bottomInput;
        uint m_inInput;

        public PlayerMonitorGVElectricElement(SubsystemGVElectricity subsystemGVElectric, CellFace cellFace) : base(subsystemGVElectric, cellFace) {
            m_subsystemPlayers = SubsystemGVElectricity.Project.FindSubsystem<SubsystemPlayers>(true);
            m_terrain = SubsystemGVElectricity.SubsystemTerrain.Terrain;
        }

        public override uint GetOutputVoltage(int face) {
            GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, Rotation, face);
            if (connectorDirection.HasValue) {
                switch (connectorDirection.Value) {
                    case GVElectricConnectorDirection.Right: return m_rightOutput;
                    case GVElectricConnectorDirection.Top: return m_topOutput;
                    case GVElectricConnectorDirection.Left: return m_leftOutput;
                }
            }
            return 0u;
        }

        public override bool Simulate() {
            uint bottomInput = m_bottomInput;
            uint inInput = m_inInput;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, Rotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue) {
                        if (connectorDirection == GVElectricConnectorDirection.Bottom) {
                            m_bottomInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                        }
                        else if (connectorDirection == GVElectricConnectorDirection.In) {
                            m_inInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                        }
                    }
                }
            }
            if (inInput == m_inInput
                && bottomInput == m_bottomInput) {
                return false;
            }
            int playerIndex = (int)(m_bottomInput >> 16);
            uint operation = m_bottomInput & 0xffff;
            uint rightOutput = m_rightOutput;
            uint leftOutput = m_leftOutput;
            uint topOutput = m_topOutput;
            m_rightOutput = 0u;
            m_leftOutput = 0u;
            m_topOutput = 0u;
            if (playerIndex < m_subsystemPlayers.ComponentPlayers.Count) {
                ComponentPlayer componentPlayer = m_subsystemPlayers.ComponentPlayers[playerIndex];
                switch (operation) {
                    case 1: {
                        Vector3 position = componentPlayer.ComponentBody.Position;
                        m_rightOutput = Float2Uint(position.X);
                        m_topOutput = Float2Uint(position.Y);
                        m_leftOutput = Float2Uint(position.Z);
                        break;
                    }
                }
            }
            return rightOutput != m_rightOutput || leftOutput != m_leftOutput || topOutput != m_topOutput;
        }

        public static uint Float2Uint(float num) => ((num < 0 ? 1u : 0u) << 31) | (((uint)Math.Truncate(num) & 0x7fff) << 16) | (uint)Math.Round(num % 1 * 0xffff);
    }
}