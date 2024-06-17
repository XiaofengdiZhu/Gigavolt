using System;
using System.Collections.Generic;
using Engine;

namespace Game {
    public class AttractorGVElectricElement : GVElectricElement {
        public readonly SubsystemPickables m_subsystemPickables;
        public readonly SubsystemProjectiles m_subsystemProjectiles;
        public readonly SubsystemGVProjectiles m_subsystemGVProjectiles;
        public readonly SubsystemFireBlockBehavior m_subsystemFireBlockBehavior;
        public readonly SubsystemExplosions m_subsystemExplosions;
        public readonly SubsystemPlayers m_subsystemPlayers;
        public readonly SubsystemBodies m_subsystemBodies;
        public readonly SubsystemGVAttractorBlockBehavior m_subsystemGVAttractorBlockBehavior;
        public readonly GVSubterrainSystem m_subterrainSystem;
        public readonly Random m_random;
        public uint m_lastInput;
        public readonly Vector3 m_originalPosition;

        public AttractorGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, Point3 point, uint subterrainId) : base(subsystemGVElectricity, new List<GVCellFace> { new(point.X, point.Y, point.Z, 4) }, subterrainId) {
            m_subsystemPickables = subsystemGVElectricity.Project.FindSubsystem<SubsystemPickables>(true);
            m_subsystemProjectiles = subsystemGVElectricity.Project.FindSubsystem<SubsystemProjectiles>(true);
            m_subsystemGVProjectiles = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVProjectiles>(true);
            m_subsystemFireBlockBehavior = subsystemGVElectricity.Project.FindSubsystem<SubsystemFireBlockBehavior>(true);
            m_subsystemExplosions = subsystemGVElectricity.Project.FindSubsystem<SubsystemExplosions>(true);
            m_subsystemPlayers = subsystemGVElectricity.Project.FindSubsystem<SubsystemPlayers>(true);
            m_subsystemBodies = subsystemGVElectricity.Project.FindSubsystem<SubsystemBodies>(true);
            m_subsystemGVAttractorBlockBehavior = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVAttractorBlockBehavior>(true);
            m_subterrainSystem = subterrainId == 0 ? null : GVStaticStorage.GVSubterrainSystemDictionary[subterrainId];
            m_random = new Random();
            m_originalPosition = new Vector3(point.X + 0.5f, point.Y + 0.5f, point.Z + 0.5f);
        }

        public override bool Simulate() {
            uint lastInput = m_lastInput;
            m_lastInput = 0u;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.NeighborConnectorType != GVElectricConnectorType.Input) {
                    m_lastInput |= connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                }
            }
            if (m_lastInput != lastInput) {
                float rangeSquared = m_lastInput & 0xFFu;
                if (rangeSquared <= 0) {
                    return false;
                }
                bool ignoreDistance = (m_lastInput & 0xFFu) == 0xFFu;
                rangeSquared *= rangeSquared;
                int specifiedContent = (int)(m_lastInput >> 22);
                bool rebound = ((m_lastInput >> 15) & 1u) == 1u;
                float speed = ((m_lastInput >> 16) & 0x3Fu) * (rebound ? -1f : 1f);
                bool attractAnimals = ((m_lastInput >> 14) & 1u) == 1u;
                bool attractPlayers = ((m_lastInput >> 13) & 1u) == 1u;
                bool attractExplosion = ((m_lastInput >> 12) & 1u) == 1u;
                bool attractFire = ((m_lastInput >> 11) & 1u) == 1u;
                bool attractProjectiles = ((m_lastInput >> 10) & 1u) == 1u;
                bool attractPickables = ((m_lastInput >> 9) & 1u) == 1u;
                bool parabola = ((m_lastInput >> 8) & 1u) == 1u;
                Vector3 originalPosition = SubterrainId == 0 ? m_originalPosition : Vector3.Transform(m_originalPosition, m_subterrainSystem.GlobalTransform);
                Point3 originalPositionPoint = Terrain.ToCell(originalPosition);
                if (speed != 0f || parabola) {
                    if (attractProjectiles) {
                        foreach (SubsystemGVProjectiles.Projectile projectile in m_subsystemGVProjectiles.m_projectiles) {
                            if (!projectile.ToRemove
                                && (specifiedContent == 0 || projectile.Value == specifiedContent)) {
                                bool inDistance = ignoreDistance;
                                if (!inDistance) {
                                    float distanceSquared = Vector3.DistanceSquared(originalPosition, projectile.Position);
                                    inDistance = distanceSquared > 0.5f && distanceSquared < rangeSquared;
                                }
                                if (inDistance) {
                                    projectile.Velocity = parabola ? GuidedDispenserGVElectricElement.GetDirection(projectile.Position, originalPosition) : Vector3.Normalize(originalPosition - projectile.Position) * speed;
                                    projectile.DisableGravity = !parabola;
                                    projectile.DisableDamping = parabola || !rebound;
                                    projectile.Transform = false;
                                    projectile.StopAt = originalPositionPoint;
                                }
                            }
                        }
                        foreach (Projectile projectile in m_subsystemProjectiles.m_projectiles) {
                            if (!projectile.ToRemove
                                && (specifiedContent == 0 || projectile.Value == specifiedContent)) {
                                bool inDistance = ignoreDistance;
                                if (!inDistance) {
                                    float distanceSquared = Vector3.DistanceSquared(originalPosition, projectile.Position);
                                    inDistance = distanceSquared > 0.5f && distanceSquared < rangeSquared;
                                }
                                if (inDistance) {
                                    SubsystemGVProjectiles.Projectile newProjectile = m_subsystemGVProjectiles.AddProjectile(
                                        projectile.Value,
                                        projectile.Position,
                                        parabola ? GuidedDispenserGVElectricElement.GetDirection(projectile.Position, originalPosition) : Vector3.Normalize(originalPosition - projectile.Position) * speed,
                                        projectile.AngularVelocity,
                                        projectile.Owner,
                                        1,
                                        !parabola,
                                        parabola || !rebound,
                                        false,
                                        false,
                                        originalPositionPoint
                                    );
                                    newProjectile.IsIncendiary = projectile.IsIncendiary;
                                    newProjectile.Light = projectile.Light;
                                    newProjectile.NoChunk = projectile.NoChunk;
                                    newProjectile.CreationTime = projectile.CreationTime;
                                    newProjectile.TrailOffset = projectile.TrailOffset;
                                    newProjectile.IsInWater = projectile.IsInWater;
                                    newProjectile.LastNoiseTime = projectile.LastNoiseTime;
                                    newProjectile.TrailParticleSystem = projectile.TrailParticleSystem;
                                    projectile.ToRemove = true;
                                }
                            }
                        }
                    }
                    if (attractPickables) {
                        foreach (Pickable pickable in m_subsystemPickables.m_pickables) {
                            if (!pickable.ToRemove
                                && (specifiedContent == 0 || pickable.Value == specifiedContent)) {
                                bool inDistance = ignoreDistance;
                                if (!inDistance) {
                                    float distanceSquared = Vector3.DistanceSquared(originalPosition, pickable.Position);
                                    inDistance = distanceSquared > 0.5f && distanceSquared < rangeSquared;
                                }
                                if (inDistance) {
                                    m_subsystemGVProjectiles.FireProjectile(
                                        pickable.Value,
                                        pickable.Position,
                                        parabola ? GuidedDispenserGVElectricElement.GetDirection(pickable.Position, originalPosition) : Vector3.Normalize(originalPosition - pickable.Position) * speed,
                                        Vector3.Zero,
                                        null,
                                        pickable.Count,
                                        !parabola,
                                        parabola || !rebound,
                                        false,
                                        false,
                                        originalPositionPoint
                                    );
                                    pickable.ToRemove = true;
                                }
                            }
                        }
                    }
                    if (attractExplosion) {
                        HashSet<SubsystemExplosions.ExplosionData> toRemove = new();
                        foreach (SubsystemExplosions.ExplosionData explosion in m_subsystemExplosions.m_queuedExplosions) {
                            Vector3 explosionPosition = new(explosion.X + 0.5f, explosion.Y + 0.5f, explosion.Z + 0.5f);
                            bool inDistance = ignoreDistance;
                            if (!inDistance) {
                                float distanceSquared = Vector3.DistanceSquared(originalPosition, explosionPosition);
                                inDistance = distanceSquared > 0.5f && distanceSquared < rangeSquared;
                            }
                            if (inDistance) {
                                toRemove.Add(explosion);
                                if (explosion.Pressure < 2f) {
                                    continue;
                                }
                                SubsystemGVProjectiles.Projectile projectile = m_subsystemGVProjectiles.FireProjectile(
                                    FireBlock.Index,
                                    explosionPosition,
                                    parabola ? GuidedDispenserGVElectricElement.GetDirection(explosionPosition, originalPosition) : Vector3.Normalize(originalPosition - explosionPosition) * speed,
                                    Vector3.Zero,
                                    null,
                                    (int)explosion.Pressure,
                                    !parabola,
                                    parabola || !rebound,
                                    false,
                                    false,
                                    originalPositionPoint
                                );
                                projectile.IsIncendiary = explosion.IsIncendiary;
                                projectile.OnRemoveWithSelf = self => {
                                    Vector3 stopAtVector3 = new Vector3(self.StopAt) + new Vector3(0.5f);
                                    bool nearEnough = Vector3.DistanceSquared(self.Position, stopAtVector3) < 3f;
                                    m_subsystemExplosions.AddExplosion(
                                        nearEnough ? self.StopAt.X : Terrain.ToCell(self.Position.X),
                                        nearEnough ? self.StopAt.Y : Terrain.ToCell(self.Position.Y),
                                        nearEnough ? self.StopAt.Z : Terrain.ToCell(self.Position.Z),
                                        self.Count,
                                        self.IsIncendiary,
                                        false
                                    );
                                };
                                projectile.ProjectileStoppedAction = ProjectileStoppedAction.Disappear;
                                m_subsystemGVProjectiles.AddTrail(projectile, Vector3.Zero, new SmokeTrailParticleSystem((int)MathF.Min(MathF.Log10(explosion.Pressure) * 10f, 28f), MathF.Min(MathF.Log(explosion.Pressure), 6.4f), float.MaxValue, Color.White));
                            }
                        }
                        foreach (SubsystemExplosions.ExplosionData explosion in toRemove) {
                            m_subsystemExplosions.m_queuedExplosions.Remove(explosion);
                        }
                    }
                    if (attractFire) {
                        HashSet<Point3> toRemove = new();
                        foreach (SubsystemFireBlockBehavior.FireData fire in m_subsystemFireBlockBehavior.m_fireData.Values) {
                            Vector3 firePosition = new(fire.Point.X + 0.5f, fire.Point.Y + 0.5f, fire.Point.Z + 0.5f);
                            bool inDistance = ignoreDistance;
                            if (!inDistance) {
                                float distanceSquared = Vector3.DistanceSquared(originalPosition, firePosition);
                                inDistance = distanceSquared > 2.1f && distanceSquared < rangeSquared;
                            }
                            if (inDistance) {
                                toRemove.Add(fire.Point);
                                SubsystemGVProjectiles.Projectile projectile = m_subsystemGVProjectiles.FireProjectile(
                                    FireBlock.Index,
                                    firePosition,
                                    parabola ? GuidedDispenserGVElectricElement.GetDirection(firePosition, originalPosition) : Vector3.Normalize(originalPosition - firePosition) * speed,
                                    Vector3.Zero,
                                    null,
                                    1,
                                    !parabola,
                                    parabola || !rebound,
                                    false,
                                    false,
                                    originalPositionPoint
                                );
                                projectile.OnRemoveWithSelf = self => {
                                    Vector3 stopAtVector3 = new Vector3(self.StopAt) + new Vector3(0.5f);
                                    bool nearEnough = Vector3.DistanceSquared(self.Position, stopAtVector3) < 3f;
                                    m_subsystemExplosions.AddExplosion(
                                        nearEnough ? self.StopAt.X : Terrain.ToCell(self.Position.X),
                                        nearEnough ? self.StopAt.Y : Terrain.ToCell(self.Position.Y),
                                        nearEnough ? self.StopAt.Z : Terrain.ToCell(self.Position.Z),
                                        m_random.Float(2f, 6f),
                                        true,
                                        false
                                    );
                                };
                                projectile.ProjectileStoppedAction = ProjectileStoppedAction.Disappear;
                                m_subsystemGVProjectiles.AddTrail(projectile, Vector3.Zero, new SmokeTrailParticleSystem(m_random.Int(8, 20), m_random.Float(0.5f, 1.5f), float.MaxValue, Color.White));
                            }
                        }
                        foreach (Point3 point in toRemove) {
                            SubsystemGVElectricity.SubsystemTerrain.ChangeCell(point.X, point.Y, point.Z, 0);
                        }
                    }
                }
                if (speed != 0f) {
                    if (attractPlayers) {
                        foreach (ComponentPlayer player in m_subsystemPlayers.m_componentPlayers) {
                            ComponentBody body = player.ComponentBody;
                            bool inDistance = ignoreDistance;
                            if (!inDistance) {
                                float distanceSquared = Vector3.DistanceSquared(originalPosition, body.Position);
                                inDistance = distanceSquared > body.BoxSize.LengthSquared() / 4 && distanceSquared < rangeSquared;
                            }
                            if (inDistance) {
                                m_subsystemGVAttractorBlockBehavior.StartMoveBody(body, originalPosition, speed, rebound);
                            }
                        }
                    }
                    if (attractAnimals) {
                        foreach (ComponentBody body in m_subsystemBodies.m_areaByComponentBody.Keys) {
                            if (body.Entity.FindComponent<ComponentPlayer>(false) != null) {
                                continue;
                            }
                            bool inDistance = ignoreDistance;
                            if (!inDistance) {
                                float distanceSquared = Vector3.DistanceSquared(originalPosition, body.Position);
                                inDistance = distanceSquared > body.BoxSize.LengthSquared() / 4 && distanceSquared < rangeSquared;
                            }
                            if (inDistance) {
                                m_subsystemGVAttractorBlockBehavior.StartMoveBody(body, originalPosition, speed, rebound);
                            }
                        }
                    }
                }
            }
            return false;
        }
    }
}