using System;
using Engine;

namespace Game {
    public class PlayerControllerGVElectricElement : RotateableGVElectricElement {
        readonly SubsystemPlayers m_subsystemPlayers;
        uint? m_rightInput;
        uint? m_topInput;
        uint? m_leftInput;
        uint m_bottomInput;
        uint m_inInput;

        public PlayerControllerGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, uint subterrainId) : base(
            subsystemGVElectricity,
            cellFace,
            subterrainId
        ) => m_subsystemPlayers = SubsystemGVElectricity.Project.FindSubsystem<SubsystemPlayers>(true);

        public override uint GetOutputVoltage(int face) => 0u;

        public override bool Simulate() {
            uint bottomInput = m_bottomInput;
            uint inInput = m_inInput;
            m_rightInput = null;
            m_leftInput = null;
            m_topInput = null;
            int rotation = Rotation;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    GVElectricConnectorDirection? connectorDirection =
                        SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue) {
                        switch (connectorDirection) {
                            case GVElectricConnectorDirection.Bottom:
                                m_bottomInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace); break;
                            case GVElectricConnectorDirection.In:
                                m_inInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace); break;
                            case GVElectricConnectorDirection.Left:
                                m_leftInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace); break;
                            case GVElectricConnectorDirection.Right:
                                m_rightInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace); break;
                            case GVElectricConnectorDirection.Top:
                                m_topInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace); break;
                        }
                    }
                }
            }
            if (m_inInput > 0) {
                if ((m_inInput & 1u) == 1u) {
                    m_rightInput = null;
                }
                if ((m_inInput & 2u) == 2u) {
                    m_topInput = null;
                }
                if ((m_inInput & 4u) == 4u) {
                    m_leftInput = null;
                }
            }
            if ((inInput == m_inInput && bottomInput == m_bottomInput)
                || (!m_rightInput.HasValue && !m_topInput.HasValue && !m_leftInput.HasValue)) {
                return false;
            }
            int playerIndex = (int)(m_bottomInput >> 16);
            uint operation = m_bottomInput & 0xffff;
            if (playerIndex < m_subsystemPlayers.ComponentPlayers.Count) {
                ComponentPlayer componentPlayer = m_subsystemPlayers.ComponentPlayers[playerIndex];
                switch (operation) {
                    case 1u: {
                        Vector3 position = componentPlayer.ComponentBody.Position;
                        if (m_rightInput.HasValue) {
                            position.X = Uint2Float(m_rightInput.Value);
                        }
                        if (m_topInput.HasValue) {
                            position.Y = Uint2Float(m_topInput.Value);
                        }
                        if (m_leftInput.HasValue) {
                            position.Z = Uint2Float(m_leftInput.Value);
                        }
                        componentPlayer.ComponentBody.Position = position;
                        break;
                    }
                    case 2u: {
                        Vector3 direction = componentPlayer.ComponentBody.Rotation.ToYawPitchRoll();
                        if (m_rightInput.HasValue) {
                            direction.X = (float)(VolatileListMemoryBankGVElectricElement.Uint2Double(m_rightInput.Value) * Math.PI / 180);
                        }
                        if (m_topInput.HasValue) {
                            direction.Y = (float)(VolatileListMemoryBankGVElectricElement.Uint2Double(m_topInput.Value) * Math.PI / 180);
                        }
                        if (m_leftInput.HasValue) {
                            direction.Z = (float)(VolatileListMemoryBankGVElectricElement.Uint2Double(m_leftInput.Value) * Math.PI / 180);
                        }
                        componentPlayer.ComponentBody.Rotation = Quaternion.CreateFromYawPitchRoll(direction.X, direction.Y, direction.Z);
                        break;
                    }
                    case 3u: {
                        Vector3 velocity = componentPlayer.ComponentBody.Velocity;
                        if (m_rightInput.HasValue) {
                            velocity.X = Uint2Float(m_rightInput.Value);
                        }
                        if (m_topInput.HasValue) {
                            velocity.Y = Uint2Float(m_topInput.Value);
                        }
                        if (m_leftInput.HasValue) {
                            velocity.Z = Uint2Float(m_leftInput.Value);
                        }
                        componentPlayer.ComponentBody.m_velocity = velocity;
                        break;
                    }
                    case 5u: {
                        Vector2 direction = componentPlayer.ComponentLocomotion.LookAngles;
                        if (m_rightInput.HasValue) {
                            direction.X = (float)(VolatileListMemoryBankGVElectricElement.Uint2Double(m_rightInput.Value) * Math.PI / 180);
                        }
                        if (m_topInput.HasValue) {
                            direction.Y = (float)(VolatileListMemoryBankGVElectricElement.Uint2Double(m_topInput.Value) * Math.PI / 180);
                        }
                        componentPlayer.ComponentLocomotion.LookAngles = direction;
                        break;
                    }
                    case 8u: {
                        Vector3 position = componentPlayer.PlayerData.SpawnPosition;
                        if (m_rightInput.HasValue) {
                            position.X = Uint2Float(m_rightInput.Value);
                        }
                        if (m_topInput.HasValue) {
                            position.Y = Uint2Float(m_topInput.Value);
                        }
                        if (m_leftInput.HasValue) {
                            position.Z = Uint2Float(m_leftInput.Value);
                        }
                        componentPlayer.PlayerData.SpawnPosition = position;
                        break;
                    }
                    case 16u: {
                        if (m_rightInput.HasValue) {
                            componentPlayer.ComponentLocomotion.m_componentCreature.ComponentHealth.Health = Uint2Float(m_rightInput.Value);
                        }
                        if (m_topInput.HasValue) {
                            componentPlayer.ComponentVitalStats.Stamina = Uint2Float(m_topInput.Value);
                        }
                        if (m_leftInput.HasValue) {
                            componentPlayer.ComponentVitalStats.Sleep = Uint2Float(m_leftInput.Value);
                        }
                        break;
                    }
                    case 17u: {
                        ComponentVitalStats stats = componentPlayer.ComponentVitalStats;
                        if (m_rightInput.HasValue) {
                            stats.Food = Uint2Float(m_rightInput.Value);
                        }
                        if (m_topInput.HasValue) {
                            stats.Temperature = Uint2Float(m_topInput.Value);
                        }
                        if (m_leftInput.HasValue) {
                            stats.Wetness = Uint2Float(m_leftInput.Value);
                        }
                        break;
                    }
                    case 18u: {
                        if (m_rightInput.HasValue) {
                            componentPlayer.PlayerData.Level = Uint2Float(m_rightInput.Value);
                        }
                        if (m_leftInput.HasValue) {
                            IInventory inventory = componentPlayer.ComponentMiner.Inventory;
                            int activeSlotIndex = inventory.ActiveSlotIndex;
                            int count = inventory.RemoveSlotItems(activeSlotIndex, int.MaxValue);
                            int newValue = (int)m_leftInput.Value;
                            inventory.AddSlotItems(activeSlotIndex, newValue, Math.Min(count, inventory.GetSlotCapacity(activeSlotIndex, newValue)));
                        }
                        break;
                    }
                    case 19u: {
                        if (m_rightInput.HasValue) {
                            componentPlayer.ComponentFlu.m_fluDuration = Uint2Float(m_rightInput.Value);
                        }
                        if (m_topInput.HasValue) {
                            componentPlayer.ComponentSickness.m_sicknessDuration = Uint2Float(m_topInput.Value);
                        }
                        if (m_leftInput.HasValue) {
                            componentPlayer.Entity.FindComponent<ComponentOnFire>(true).m_fireDuration = Uint2Float(m_leftInput.Value);
                        }
                        break;
                    }
                }
            }
            return false;
        }

        public static uint Float2Uint(float num) => ((num < 0 ? 1u : 0u) << 31)
            | (((uint)Math.Truncate(Math.Abs(num)) & 0x7fff) << 16)
            | (uint)Math.Round(num % 1 * 0xffff);

        public static float Uint2Float(uint num) => (num >> 31 == 1 ? -1 : 1) * (((num >> 16) & 0x7fffu) + (float)(num & 0xffffu) / 0xffff);
    }
}