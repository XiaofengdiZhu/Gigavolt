using System;
using Engine;
using Engine.Graphics;
using Engine.Media;

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
                    case 1u: {
                        Vector3 position = componentPlayer.ComponentBody.Position;
                        m_rightOutput = Float2Uint(position.X);
                        m_topOutput = Float2Uint(position.Y);
                        m_leftOutput = Float2Uint(position.Z);
                        break;
                    }
                    case 2u: {
                        Vector3 direction = componentPlayer.ComponentBody.Rotation.ToYawPitchRoll();
                        m_rightOutput = MoreOneInOneOutGVElectricElement.Double2Uint(direction.X * 180 / Math.PI);
                        m_topOutput = MoreOneInOneOutGVElectricElement.Double2Uint(direction.Y * 180 / Math.PI);
                        m_leftOutput = MoreOneInOneOutGVElectricElement.Double2Uint(direction.Z * 180 / Math.PI);
                        break;
                    }
                    case 3u: {
                        Vector3 velocity = componentPlayer.ComponentBody.Velocity;
                        m_rightOutput = Float2Uint(velocity.X);
                        m_topOutput = Float2Uint(velocity.Y);
                        m_leftOutput = Float2Uint(velocity.Z);
                        break;
                    }
                    case 4u: {
                        Vector3 position = componentPlayer.ComponentCreatureModel.EyePosition;
                        m_rightOutput = Float2Uint(position.X);
                        m_topOutput = Float2Uint(position.Y);
                        m_leftOutput = Float2Uint(position.Z);
                        break;
                    }
                    case 5u: {
                        Vector2 direction = componentPlayer.ComponentLocomotion.LookAngles;
                        m_rightOutput = MoreOneInOneOutGVElectricElement.Double2Uint(direction.X * 180 / Math.PI);
                        m_topOutput = MoreOneInOneOutGVElectricElement.Double2Uint(direction.Y * 180 / Math.PI);
                        break;
                    }
                    case 6u: {
                        Vector3 direction = componentPlayer.ComponentCreatureModel.EyeRotation.ToYawPitchRoll();
                        m_rightOutput = MoreOneInOneOutGVElectricElement.Double2Uint(direction.X * 180 / Math.PI);
                        m_topOutput = MoreOneInOneOutGVElectricElement.Double2Uint(direction.Y * 180 / Math.PI);
                        m_leftOutput = MoreOneInOneOutGVElectricElement.Double2Uint(direction.Z * 180 / Math.PI);
                        break;
                    }
                    case 7u: {
                        Vector3 position = componentPlayer.ComponentBody.Position;
                        int x = (int)position.X;
                        int y = (int)position.Y;
                        int z = (int)position.Z;
                        m_rightOutput = (uint)m_terrain.GetTemperature(x, z);
                        m_topOutput = (uint)m_terrain.GetHumidity(x, z);
                        m_leftOutput = (uint)m_terrain.GetCellLight(x, y, z);
                        break;
                    }
                    case 8u: {
                        Vector3 position = componentPlayer.PlayerData.SpawnPosition;
                        m_rightOutput = Float2Uint(position.X);
                        m_topOutput = Float2Uint(position.Y);
                        m_leftOutput = Float2Uint(position.Z);
                        break;
                    }
                    case 16u: {
                        m_rightOutput = Float2Uint(componentPlayer.ComponentLocomotion.m_componentCreature.ComponentHealth.Health);
                        m_topOutput = Float2Uint(componentPlayer.ComponentVitalStats.Stamina);
                        m_leftOutput = Float2Uint(componentPlayer.ComponentVitalStats.Sleep);
                        break;
                    }
                    case 17u: {
                        ComponentVitalStats stats = componentPlayer.ComponentVitalStats;
                        m_rightOutput = Float2Uint(stats.Food);
                        m_topOutput = Float2Uint(stats.Temperature);
                        m_leftOutput = Float2Uint(stats.Wetness);
                        break;
                    }
                    case 18u: {
                        float level = componentPlayer.PlayerData.Level;
                        m_rightOutput = Float2Uint(level);
                        m_topOutput = (uint)MathUtils.Ceiling(MathUtils.Pow(1.08f, MathUtils.Floor(level - 1f)) / 0.012f * (1f - level % 1f));
                        IInventory inventory = componentPlayer.ComponentMiner.Inventory;
                        m_leftOutput = (uint)inventory.GetSlotValue(inventory.ActiveSlotIndex);
                        break;
                    }
                    case 19u: {
                        m_rightOutput = Float2Uint(componentPlayer.ComponentFlu.m_fluDuration);
                        m_topOutput = Float2Uint(componentPlayer.ComponentSickness.m_sicknessDuration);
                        m_leftOutput = Float2Uint(componentPlayer.Entity.FindComponent<ComponentOnFire>(true).m_fireDuration);
                        break;
                    }
                    case 32u: {
                        ComponentLocomotion locomotion = componentPlayer.ComponentLocomotion;
                        m_rightOutput = locomotion.m_walking ? uint.MaxValue : 0u;
                        m_topOutput = locomotion.m_flying ? uint.MaxValue : 0u;
                        m_leftOutput = componentPlayer.ComponentRider.Mount != null ? uint.MaxValue : 0u;
                        break;
                    }
                    case 33u: {
                        m_rightOutput = componentPlayer.ComponentBody.IsSneaking ? uint.MaxValue : 0u;
                        m_topOutput = componentPlayer.ComponentLocomotion.m_falling ? uint.MaxValue : 0u;
                        break;
                    }
                    case 34u: {
                        m_rightOutput = componentPlayer.ComponentLocomotion.m_swimming ? uint.MaxValue : 0u;
                        m_topOutput = Float2Uint(componentPlayer.ComponentBody.ImmersionDepth);
                        break;
                    }
                    case 48u: {
                        if (componentPlayer.ViewWidget.m_scalingRenderTarget == null) {
                            GameWidget gameWidget = componentPlayer.PlayerData.GameWidget;
                            HookViewWidget hookViewWidget = gameWidget.Children.Find<HookViewWidget>("HookViewWidget", false);
                            if (hookViewWidget == null) {
                                hookViewWidget = new HookViewWidget();
                                gameWidget.Children.InsertAfter(componentPlayer.ViewWidget, hookViewWidget);
                                m_rightOutput = uint.MaxValue;
                                m_topOutput = uint.MaxValue;
                                m_leftOutput = uint.MaxValue;
                            }
                            else if (hookViewWidget.m_stopHook) {
                                hookViewWidget.m_stopHook = false;
                                m_rightOutput = uint.MaxValue;
                                m_topOutput = uint.MaxValue;
                                m_leftOutput = uint.MaxValue;
                            }
                            else {
                                hookViewWidget.m_stopHook = true;
                                m_rightOutput = 7u;
                                m_topOutput = 7u;
                                m_leftOutput = 7u;
                            }
                        }
                        else {
                            m_rightOutput = uint.MaxValue;
                            m_topOutput = uint.MaxValue;
                            m_leftOutput = uint.MaxValue;
                        }
                        break;
                    }
                    case 49u: {
                        if (GVStaticStorage.GVMBIDDataDictionary.TryGetValue(m_inInput, out GVArrayData memory)) {
                            RenderTarget2D renderTarget = componentPlayer.ViewWidget.m_scalingRenderTarget;
                            if (componentPlayer.ViewWidget.m_scalingRenderTarget == null) {
                                HookViewWidget hookViewWidget = componentPlayer.PlayerData.GameWidget.Children.Find<HookViewWidget>("HookViewWidget", false);
                                if (hookViewWidget != null
                                    && !hookViewWidget.m_stopHook) {
                                    memory.UintArray2Data(hookViewWidget.m_hookedImage, Window.Size.X, Window.Size.Y);
                                    hookViewWidget.m_hookedImage = null;
                                }
                            }
                            else {
                                Image image = new(renderTarget.Width, renderTarget.Height);
                                renderTarget.GetData(image.Pixels, 0, new Rectangle(0, 0, renderTarget.Width, renderTarget.Height));
                                memory.Image2Data(image);
                            }
                        }
                        break;
                    }
                }
            }
            return rightOutput != m_rightOutput || leftOutput != m_leftOutput || topOutput != m_topOutput;
        }

        public static uint Float2Uint(float num) => ((num < 0 ? 1u : 0u) << 31) | (((uint)Math.Truncate(Math.Abs(num)) & 0x7fff) << 16) | (uint)Math.Round(num % 1 * 0xffff);
    }
}