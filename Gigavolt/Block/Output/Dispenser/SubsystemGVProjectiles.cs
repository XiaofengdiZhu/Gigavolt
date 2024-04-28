using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Engine;
using Engine.Graphics;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVProjectiles : Subsystem, IUpdateable, IDrawable {
        public class Projectile : Game.Projectile {
            public bool DisableGravity;
            public bool DisableDamping;
            public bool Safe;
            public bool Transform;
            public Point3 StopAt;
            public int Count = 1;
            public Action<Projectile> OnRemoveWithSelf;
        }

        public SubsystemAudio m_subsystemAudio;

        public SubsystemSoundMaterials m_subsystemSoundMaterials;

        public SubsystemParticles m_subsystemParticles;

        public SubsystemPickables m_subsystemPickables;

        public SubsystemBodies m_subsystemBodies;

        public SubsystemTerrain m_subsystemTerrain;

        public SubsystemSky m_subsystemSky;

        public SubsystemTime m_subsystemTime;

        public SubsystemNoise m_subsystemNoise;

        public SubsystemExplosions m_subsystemExplosions;

        public SubsystemGameInfo m_subsystemGameInfo;

        public SubsystemBlockBehaviors m_subsystemBlockBehaviors;

        public SubsystemFluidBlockBehavior m_subsystemFluidBlockBehavior;

        public SubsystemFireBlockBehavior m_subsystemFireBlockBehavior;

        public List<Projectile> m_projectiles = new();

        public List<Projectile> m_projectilesToRemove = new();

        public PrimitivesRenderer3D m_primitivesRenderer = new();

        public Random m_random = new();

        public DrawBlockEnvironmentData m_drawBlockEnvironmentData = new();

        public const float BodyInflateAmount = 0.2f;

        public static readonly int[] m_drawOrders = { 10 };

        public ReadOnlyList<Projectile> Projectiles => new(m_projectiles);

        public int[] DrawOrders => m_drawOrders;

        public virtual Action<Projectile> ProjectileAdded { get; set; }

        public virtual Action<Projectile> ProjectileRemoved { get; set; }

        public UpdateOrder UpdateOrder => UpdateOrder.Default;
        public static int? m_crusherBlockValue = null;
        public static int? m_interactorBlockValue = null;
        public static int? m_dataModifierBlockContent = null;

        public virtual Projectile AddProjectile(int value, Vector3 position, Vector3 velocity, Vector3 angularVelocity, ComponentCreature owner, int count = 1, bool disableGravity = false, bool disableDamping = false, bool safe = false, bool transform = false, Point3 stopAt = default) {
            Projectile projectile = new() {
                Value = value,
                Position = position,
                Velocity = velocity,
                Rotation = Vector3.Zero,
                AngularVelocity = angularVelocity,
                CreationTime = m_subsystemGameInfo.TotalElapsedGameTime,
                IsInWater = IsWater(position),
                Owner = owner,
                ProjectileStoppedAction = ProjectileStoppedAction.TurnIntoPickable,
                DisableGravity = disableGravity,
                DisableDamping = disableDamping,
                Safe = safe,
                Transform = transform,
                StopAt = stopAt,
                Count = count
            };
            m_projectiles.Add(projectile);
            ProjectileAdded?.Invoke(projectile);
            if (owner != null
                && owner.PlayerStats != null) {
                owner.PlayerStats.RangedAttacks++;
            }
            return projectile;
        }

        public virtual Projectile FireProjectile(int value, Vector3 position, Vector3 velocity, Vector3 angularVelocity, ComponentCreature owner, int count, bool disableGravity, bool disableDamping, bool safe, bool transform, Point3 stopAt) {
            int num = Terrain.ExtractContents(value);
            Block block = BlocksManager.Blocks[num];
            Vector3 v = Vector3.Normalize(velocity);
            Vector3 vector = position;
            if (owner != null) {
                Ray3 ray = new(position + v * 5f, -v);
                BoundingBox boundingBox = owner.ComponentBody.BoundingBox;
                boundingBox.Min -= new Vector3(0.4f);
                boundingBox.Max += new Vector3(0.4f);
                float? num2 = ray.Intersection(boundingBox);
                if (num2.HasValue) {
                    if (num2.Value == 0f) {
                        return null;
                    }
                    vector = position + v * (5f - num2.Value + 0.1f);
                }
            }
            Vector3 end = vector + v * block.ProjectileTipOffset;
            if (!m_subsystemTerrain.Raycast(
                    position,
                    end,
                    false,
                    true,
                    (testValue, _) => BlocksManager.Blocks[Terrain.ExtractContents(testValue)].IsCollidable_(testValue)
                )
                .HasValue) {
                Projectile projectile = AddProjectile(
                    value,
                    vector,
                    velocity,
                    angularVelocity,
                    owner,
                    count,
                    disableGravity,
                    disableDamping,
                    safe,
                    transform,
                    stopAt
                );
                if (!safe) {
                    SubsystemBlockBehavior[] blockBehaviors = m_subsystemBlockBehaviors.GetBlockBehaviors(Terrain.ExtractContents(value));
                    for (int i = 0; i < blockBehaviors.Length; i++) {
                        blockBehaviors[i].OnFiredAsProjectile(projectile);
                    }
                }
                return projectile;
            }
            return null;
        }

        public virtual void AddTrail(Projectile projectile, Vector3 offset, ITrailParticleSystem particleSystem) {
            RemoveTrail(projectile);
            projectile.TrailParticleSystem = particleSystem;
            projectile.TrailOffset = offset;
        }

        public virtual void RemoveTrail(Projectile projectile) {
            if (projectile.TrailParticleSystem != null) {
                if (m_subsystemParticles.ContainsParticleSystem((ParticleSystemBase)projectile.TrailParticleSystem)) {
                    m_subsystemParticles.RemoveParticleSystem((ParticleSystemBase)projectile.TrailParticleSystem);
                }
                projectile.TrailParticleSystem = null;
            }
        }

        public void Draw(Camera camera, int drawOrder) {
            m_drawBlockEnvironmentData.SubsystemTerrain = m_subsystemTerrain;
            m_drawBlockEnvironmentData.InWorldMatrix = Matrix.Identity;
            float num = MathUtils.Sqr(m_subsystemSky.VisibilityRange);
            foreach (Projectile projectile in m_projectiles) {
                Vector3 position = projectile.Position;
                if (!projectile.NoChunk
                    && Vector3.DistanceSquared(camera.ViewPosition, position) < num
                    && camera.ViewFrustum.Intersection(position)) {
                    int x = Terrain.ToCell(position.X);
                    int num2 = Terrain.ToCell(position.Y);
                    int z = Terrain.ToCell(position.Z);
                    int num3 = Terrain.ExtractContents(projectile.Value);
                    Block block = BlocksManager.Blocks[num3];
                    TerrainChunk chunkAtCell = m_subsystemTerrain.Terrain.GetChunkAtCell(x, z);
                    if (chunkAtCell != null
                        && chunkAtCell.State >= TerrainChunkState.InvalidVertices1
                        && num2 >= 0
                        && num2 < 255) {
                        m_drawBlockEnvironmentData.Humidity = m_subsystemTerrain.Terrain.GetSeasonalHumidity(x, z);
                        m_drawBlockEnvironmentData.Temperature = m_subsystemTerrain.Terrain.GetSeasonalTemperature(x, z) + SubsystemWeather.GetTemperatureAdjustmentAtHeight(num2);
                        projectile.Light = m_subsystemTerrain.Terrain.GetCellLightFast(x, num2, z);
                    }
                    m_drawBlockEnvironmentData.Light = projectile.Light;
                    m_drawBlockEnvironmentData.BillboardDirection = block.GetAlignToVelocity(projectile.Value) ? null : new Vector3?(camera.ViewDirection);
                    m_drawBlockEnvironmentData.InWorldMatrix.Translation = position;
                    Matrix matrix;
                    if (block.GetAlignToVelocity(projectile.Value)) {
                        CalculateVelocityAlignMatrix(block, position, projectile.Velocity, out matrix);
                    }
                    else if (projectile.Rotation != Vector3.Zero) {
                        matrix = Matrix.CreateFromAxisAngle(Vector3.Normalize(projectile.Rotation), projectile.Rotation.Length());
                        matrix.Translation = position;
                    }
                    else {
                        matrix = Matrix.CreateTranslation(position);
                    }
                    block.DrawBlock(
                        m_primitivesRenderer,
                        projectile.Value,
                        Color.White,
                        0.3f,
                        ref matrix,
                        m_drawBlockEnvironmentData
                    );
                }
            }
            m_primitivesRenderer.Flush(camera.ViewProjectionMatrix);
        }

        public void Update(float dt) {
            double totalElapsedGameTime = m_subsystemGameInfo.TotalElapsedGameTime;
            foreach (Projectile projectile in m_projectiles) {
                if (projectile.ToRemove) {
                    m_projectilesToRemove.Add(projectile);
                }
                else {
                    Block block = BlocksManager.Blocks[Terrain.ExtractContents(projectile.Value)];
                    if (totalElapsedGameTime - projectile.CreationTime > 40.0) {
                        projectile.ToRemove = true;
                    }
                    Vector3 position = projectile.Position;
                    TerrainChunk chunkAtCell = m_subsystemTerrain.Terrain.GetChunkAtCell(Terrain.ToCell(position.X), Terrain.ToCell(position.Z));
                    if (chunkAtCell == null
                        || chunkAtCell.State <= TerrainChunkState.InvalidContents4) {
                        projectile.NoChunk = true;
                        if (projectile.TrailParticleSystem != null) {
                            projectile.TrailParticleSystem.IsStopped = true;
                        }
                    }
                    else {
                        projectile.NoChunk = false;
                        Point3 positionInt = Terrain.ToCell(position);
                        Vector3 stopAtVector3 = new Vector3(projectile.StopAt) + new Vector3(0.5f);
                        Vector3 newPosition = position + projectile.Velocity * dt;
                        Point3 newPositionInt = Terrain.ToCell(newPosition);
                        Vector3 v = block.ProjectileTipOffset * Vector3.Normalize(projectile.Velocity);
                        bool isCrusher = m_crusherBlockValue.HasValue && m_crusherBlockValue.Value == projectile.Value;
                        bool isInteractor = m_interactorBlockValue.HasValue && m_interactorBlockValue.Value == projectile.Value;
                        bool isDataModifier = m_dataModifierBlockContent.HasValue && m_dataModifierBlockContent.Value == Terrain.ExtractContents(projectile.Value);
                        bool stopAtIsPassable = projectile.StopAt.Y is >= 0 and < 256;
                        if (stopAtIsPassable) {
                            int stopAtBlock = Terrain.ExtractContents(m_subsystemTerrain.Terrain.GetCellValue(projectile.StopAt.X, projectile.StopAt.Y, projectile.StopAt.Z));
                            stopAtIsPassable = !BlocksManager.Blocks[Terrain.ExtractContents(stopAtBlock)].IsCollidable_(stopAtBlock);
                        }
                        if (stopAtIsPassable && Vector3.DistanceSquared(position, stopAtVector3) < 3f) {
                            if (projectile.Transform
                                && block.IsPlaceable) {
                                m_subsystemTerrain.ChangeCell(projectile.StopAt.X, projectile.StopAt.Y, projectile.StopAt.Z, projectile.Value);
                                m_subsystemAudio.PlaySound(
                                    "Audio/BlockPlaced",
                                    1f,
                                    0f,
                                    stopAtVector3,
                                    5f,
                                    false
                                );
                                projectile.ToRemove = true;
                                continue;
                            }
                            if (projectile.ProjectileStoppedAction != ProjectileStoppedAction.Disappear) {
                                m_subsystemPickables.AddPickable(
                                    projectile.Value,
                                    projectile.Count,
                                    stopAtVector3,
                                    Vector3.Zero,
                                    null
                                );
                            }
                            projectile.ToRemove = true;
                        }
                        BodyRaycastResult? bodyRaycastResult = m_subsystemBodies.Raycast(position + v, newPosition + v, 0.2f, (_, _) => true);
                        TerrainRaycastResult? terrainRaycastResult = m_subsystemTerrain.Raycast(
                            position + v,
                            newPosition + v,
                            isInteractor,
                            true,
                            (value, _) => isCrusher || isInteractor || isDataModifier || BlocksManager.Blocks[Terrain.ExtractContents(value)].IsCollidable_(value)
                        );
                        bool flag = block.DisintegratesOnHit;
                        if (!projectile.Safe
                            && (terrainRaycastResult.HasValue || bodyRaycastResult.HasValue)) {
                            CellFace? cellFace = terrainRaycastResult.HasValue ? new CellFace?(terrainRaycastResult.Value.CellFace) : null;
                            ComponentBody componentBody = bodyRaycastResult?.ComponentBody;
                            SubsystemBlockBehavior[] blockBehaviors = m_subsystemBlockBehaviors.GetBlockBehaviors(Terrain.ExtractContents(projectile.Value));
                            for (int i = 0; i < blockBehaviors.Length; i++) {
                                flag |= blockBehaviors[i].OnHitAsProjectile(cellFace, componentBody, projectile);
                            }
                            projectile.ToRemove |= flag;
                        }
                        Vector3? vector2 = null;
                        if (bodyRaycastResult.HasValue
                            && (!terrainRaycastResult.HasValue || bodyRaycastResult.Value.Distance < terrainRaycastResult.Value.Distance)) {
                            if (projectile.Velocity.Length() > 10f) {
                                ComponentMiner.AttackBody(
                                    bodyRaycastResult.Value.ComponentBody,
                                    projectile.Owner,
                                    bodyRaycastResult.Value.HitPoint(),
                                    Vector3.Normalize(projectile.Velocity),
                                    block.GetProjectilePower(projectile.Value),
                                    false
                                );
                                if (projectile.Owner != null
                                    && projectile.Owner.PlayerStats != null) {
                                    projectile.Owner.PlayerStats.RangedHits++;
                                }
                            }
                            if (projectile.IsIncendiary) {
                                bodyRaycastResult.Value.ComponentBody.Entity.FindComponent<ComponentOnFire>()?.SetOnFire(projectile?.Owner, m_random.Float(6f, 8f));
                            }
                            newPosition = position;
                            projectile.Velocity *= -0.05f;
                            projectile.AngularVelocity *= -0.05f;
                        }
                        else if (terrainRaycastResult.HasValue) {
                            CellFace cellFace2 = terrainRaycastResult.Value.CellFace;
                            int cellValue = m_subsystemTerrain.Terrain.GetCellValue(cellFace2.X, cellFace2.Y, cellFace2.Z);
                            int num = Terrain.ExtractContents(cellValue);
                            Block block2 = BlocksManager.Blocks[num];
                            float num2 = projectile.Velocity.Length();
                            SubsystemBlockBehavior[] blockBehaviors2 = m_subsystemBlockBehaviors.GetBlockBehaviors(Terrain.ExtractContents(cellValue));
                            for (int j = 0; j < blockBehaviors2.Length; j++) {
                                for (int k = 0; k < projectile.Count; k++) {
                                    blockBehaviors2[j].OnHitByProjectile(cellFace2, projectile);
                                }
                            }
                            if (num2 > 10f
                                && m_random.Float(0f, 1f) > block2.GetProjectileResilience(cellValue)) {
                                m_subsystemTerrain.DestroyCell(
                                    0,
                                    cellFace2.X,
                                    cellFace2.Y,
                                    cellFace2.Z,
                                    0,
                                    true,
                                    false
                                );
                                m_subsystemSoundMaterials.PlayImpactSound(cellValue, position, 1f);
                            }
                            if (!projectile.Safe
                                && projectile.IsIncendiary) {
                                m_subsystemFireBlockBehavior.SetCellOnFire(terrainRaycastResult.Value.CellFace.X, terrainRaycastResult.Value.CellFace.Y, terrainRaycastResult.Value.CellFace.Z, 1f);
                                Vector3 vector3 = position - 0.75f * Vector3.Normalize(projectile.Velocity);
                                for (int k = 0; k < 8; k++) {
                                    Vector3 v2 = k == 0 ? Vector3.Normalize(projectile.Velocity) : m_random.Vector3(1.5f);
                                    TerrainRaycastResult? terrainRaycastResult2 = m_subsystemTerrain.Raycast(
                                        vector3,
                                        vector3 + v2,
                                        false,
                                        true,
                                        (_, _) => true
                                    );
                                    if (terrainRaycastResult2.HasValue) {
                                        m_subsystemFireBlockBehavior.SetCellOnFire(terrainRaycastResult2.Value.CellFace.X, terrainRaycastResult2.Value.CellFace.Y, terrainRaycastResult2.Value.CellFace.Z, 1f);
                                    }
                                }
                            }
                            if (isCrusher && !projectile.ToRemove) {
                                if (Terrain.ExtractContents(m_subsystemTerrain.Terrain.GetCellValue(positionInt.X, positionInt.Y, positionInt.Z)) == 0) {
                                    m_subsystemTerrain.DestroyCell(
                                        int.MaxValue,
                                        cellFace2.X,
                                        cellFace2.Y,
                                        cellFace2.Z,
                                        0,
                                        false,
                                        false
                                    );
                                }
                                else {
                                    m_subsystemTerrain.DestroyCell(
                                        int.MaxValue,
                                        positionInt.X,
                                        positionInt.Y,
                                        positionInt.Z,
                                        0,
                                        false,
                                        false
                                    );
                                }
                                projectile.ToRemove = true;
                                continue;
                            }
                            if (isInteractor && !projectile.ToRemove) {
                                SubsystemBlockBehavior[] blockBehaviors = m_subsystemBlockBehaviors.GetBlockBehaviors(Terrain.ExtractContents(m_subsystemTerrain.Terrain.GetCellValue(positionInt.X, positionInt.Y, positionInt.Z)));
                                if (blockBehaviors.Length > 0) {
                                    foreach (SubsystemBlockBehavior behavior in blockBehaviors) {
                                        behavior.OnInteract(new TerrainRaycastResult { CellFace = new CellFace(positionInt.X, positionInt.Y, positionInt.Z, 0) }, null);
                                    }
                                }
                                else {
                                    blockBehaviors = m_subsystemBlockBehaviors.GetBlockBehaviors(num);
                                    foreach (SubsystemBlockBehavior behavior in blockBehaviors) {
                                        behavior.OnInteract(new TerrainRaycastResult { CellFace = new CellFace(cellFace2.X, cellFace2.Y, cellFace2.Z, cellFace2.Face) }, null);
                                    }
                                }
                                projectile.ToRemove = true;
                                continue;
                            }
                            if (isDataModifier && !projectile.ToRemove) {
                                int tempContent = Terrain.ExtractContents(m_subsystemTerrain.Terrain.GetCellValue(positionInt.X, positionInt.Y, positionInt.Z));
                                if (tempContent == 0) {
                                    m_subsystemTerrain.ChangeCell(cellFace2.X, cellFace2.Y, cellFace2.Z, Terrain.MakeBlockValue(num, m_subsystemTerrain.Terrain.GetCellLightFast(cellFace2.X, cellFace2.Y, cellFace2.Z), Terrain.ExtractData(projectile.Value)));
                                }
                                else {
                                    m_subsystemTerrain.ChangeCell(positionInt.X, positionInt.Y, positionInt.Z, Terrain.MakeBlockValue(tempContent, m_subsystemTerrain.Terrain.GetCellLightFast(positionInt.X, positionInt.Y, positionInt.Z), Terrain.ExtractData(projectile.Value)));
                                }
                                projectile.ToRemove = true;
                                continue;
                            }
                            if (projectile.Transform
                                && !projectile.ToRemove
                                && block.IsPlaceable) {
                                if (stopAtIsPassable && Vector3.DistanceSquared(position, stopAtVector3) < 5f) {
                                    m_subsystemTerrain.ChangeCell(projectile.StopAt.X, projectile.StopAt.Y, projectile.StopAt.Z, projectile.Value);
                                    m_subsystemAudio.PlaySound(
                                        "Audio/BlockPlaced",
                                        1f,
                                        0f,
                                        stopAtVector3,
                                        5f,
                                        false
                                    );
                                    projectile.ToRemove = true;
                                    continue;
                                }
                                if (Terrain.ExtractContents(m_subsystemTerrain.Terrain.GetCellValue(positionInt.X, positionInt.Y, positionInt.Z)) > 0) {
                                    m_subsystemTerrain.DestroyCell(
                                        int.MaxValue,
                                        positionInt.X,
                                        positionInt.Y,
                                        positionInt.Z,
                                        0,
                                        false,
                                        false
                                    );
                                }
                                m_subsystemTerrain.ChangeCell(positionInt.X, positionInt.Y, positionInt.Z, projectile.Value);
                                m_subsystemAudio.PlaySound(
                                    "Audio/BlockPlaced",
                                    1f,
                                    0f,
                                    position,
                                    5f,
                                    false
                                );
                                projectile.ToRemove = true;
                                continue;
                            }
                            if (stopAtIsPassable && Vector3.DistanceSquared(position, stopAtVector3) < 3f) {
                                if (projectile.ProjectileStoppedAction != ProjectileStoppedAction.Disappear) {
                                    m_subsystemPickables.AddPickable(
                                        projectile.Value,
                                        projectile.Count,
                                        stopAtVector3,
                                        Vector3.Zero,
                                        null
                                    );
                                }
                                projectile.ToRemove = true;
                                continue;
                            }
                            if (num2 > 5f) {
                                m_subsystemSoundMaterials.PlayImpactSound(cellValue, position, 1f);
                            }
                            if (block.IsStickable_(projectile.Value)
                                && num2 > 10f
                                && m_random.Bool(block2.GetProjectileStickProbability(projectile.Value))) {
                                Vector3 v3 = Vector3.Normalize(projectile.Velocity);
                                float s = MathUtils.Lerp(0.1f, 0.2f, MathUtils.Saturate((num2 - 15f) / 20f));
                                vector2 = position + terrainRaycastResult.Value.Distance * Vector3.Normalize(projectile.Velocity) + v3 * s;
                            }
                            else {
                                Plane plane = cellFace2.CalculatePlane();
                                newPosition = position;
                                if (plane.Normal.X != 0f) {
                                    projectile.Velocity *= new Vector3(-0.3f, 0.3f, 0.3f);
                                }
                                if (plane.Normal.Y != 0f) {
                                    projectile.Velocity *= new Vector3(0.3f, -0.3f, 0.3f);
                                }
                                if (plane.Normal.Z != 0f) {
                                    projectile.Velocity *= new Vector3(0.3f, 0.3f, -0.3f);
                                }
                                float num3 = projectile.Velocity.Length();
                                projectile.Velocity = num3 * Vector3.Normalize(projectile.Velocity);
                                projectile.AngularVelocity *= -0.3f;
                            }
                            MakeProjectileNoise(projectile);
                        }
                        if (terrainRaycastResult.HasValue
                            || bodyRaycastResult.HasValue) {
                            if (flag && !projectile.Safe) {
                                m_subsystemParticles.AddParticleSystem(block.CreateDebrisParticleSystem(m_subsystemTerrain, position, projectile.Value, 1f));
                            }
                            else if (!projectile.ToRemove
                                && (vector2.HasValue || projectile.Velocity.Length() < 1f)) {
                                if (projectile.ProjectileStoppedAction == ProjectileStoppedAction.TurnIntoPickable) {
                                    int num4 = BlocksManager.DamageItem(projectile.Value, 1);
                                    if (num4 != 0) {
                                        if (vector2.HasValue) {
                                            CalculateVelocityAlignMatrix(block, vector2.Value, projectile.Velocity, out Matrix matrix);
                                            m_subsystemPickables.AddPickable(
                                                num4,
                                                projectile.Count,
                                                position,
                                                Vector3.Zero,
                                                matrix
                                            );
                                        }
                                        else {
                                            m_subsystemPickables.AddPickable(
                                                num4,
                                                projectile.Count,
                                                position,
                                                Vector3.Zero,
                                                null
                                            );
                                        }
                                    }
                                    projectile.ToRemove = true;
                                }
                                else if (projectile.ProjectileStoppedAction == ProjectileStoppedAction.Disappear) {
                                    projectile.ToRemove = true;
                                }
                            }
                        }
                        float num5 = projectile.IsInWater ? MathF.Pow(0.001f, dt) : MathF.Pow(block.GetProjectileDamping(projectile.Value), dt);
                        if (!projectile.DisableGravity) {
                            projectile.Velocity.Y += -10f * dt;
                        }
                        if (!projectile.DisableDamping) {
                            projectile.Velocity *= num5;
                        }
                        projectile.AngularVelocity *= num5;
                        projectile.Position = newPosition;
                        projectile.Rotation += projectile.AngularVelocity * dt;
                        if (projectile.TrailParticleSystem != null) {
                            if (!m_subsystemParticles.ContainsParticleSystem((ParticleSystemBase)projectile.TrailParticleSystem)) {
                                m_subsystemParticles.AddParticleSystem((ParticleSystemBase)projectile.TrailParticleSystem);
                            }
                            Vector3 v4 = projectile.TrailOffset != Vector3.Zero ? Vector3.TransformNormal(projectile.TrailOffset, Matrix.CreateFromAxisAngle(Vector3.Normalize(projectile.Rotation), projectile.Rotation.Length())) : Vector3.Zero;
                            projectile.TrailParticleSystem.Position = newPosition + v4;
                            if (projectile.IsInWater) {
                                projectile.TrailParticleSystem.IsStopped = true;
                            }
                        }
                        bool flag2 = IsWater(newPosition);
                        if (projectile.IsInWater != flag2) {
                            if (flag2) {
                                float num6 = new Vector2(projectile.Velocity.X + projectile.Velocity.Z).Length();
                                if (num6 > 6f
                                    && num6 > 4f * MathUtils.Abs(projectile.Velocity.Y)) {
                                    projectile.Velocity *= 0.5f;
                                    projectile.Velocity.Y *= -1f;
                                    flag2 = false;
                                }
                                else {
                                    projectile.Velocity *= 0.2f;
                                }
                                float? surfaceHeight = m_subsystemFluidBlockBehavior.GetSurfaceHeight(newPositionInt.X, newPositionInt.Y, newPositionInt.Z);
                                if (surfaceHeight.HasValue) {
                                    m_subsystemParticles.AddParticleSystem(new WaterSplashParticleSystem(m_subsystemTerrain, new Vector3(newPosition.X, surfaceHeight.Value, newPosition.Z), false));
                                    m_subsystemAudio.PlayRandomSound(
                                        "Audio/Splashes",
                                        1f,
                                        m_random.Float(-0.2f, 0.2f),
                                        newPosition,
                                        6f,
                                        true
                                    );
                                    MakeProjectileNoise(projectile);
                                }
                            }
                            projectile.IsInWater = flag2;
                        }
                        if (IsMagma(newPosition)) {
                            m_subsystemParticles.AddParticleSystem(new MagmaSplashParticleSystem(m_subsystemTerrain, newPosition, false));
                            m_subsystemAudio.PlayRandomSound(
                                "Audio/Sizzles",
                                1f,
                                m_random.Float(-0.2f, 0.2f),
                                newPosition,
                                3f,
                                true
                            );
                            projectile.ToRemove = true;
                            m_subsystemExplosions.TryExplodeBlock(newPositionInt.X, newPositionInt.Y, newPositionInt.Z, projectile.Value);
                        }
                        if (m_subsystemTime.PeriodicGameTimeEvent(1.0, projectile.GetHashCode() % 100 / 100.0)
                            && (m_subsystemFireBlockBehavior.IsCellOnFire(newPositionInt.X, Terrain.ToCell(newPosition.Y + 0.1f), newPositionInt.Z) || m_subsystemFireBlockBehavior.IsCellOnFire(newPositionInt.X, Terrain.ToCell(newPosition.Y + 0.1f) - 1, newPositionInt.Z))) {
                            m_subsystemAudio.PlayRandomSound(
                                "Audio/Sizzles",
                                1f,
                                m_random.Float(-0.2f, 0.2f),
                                newPosition,
                                3f,
                                true
                            );
                            projectile.ToRemove = true;
                            m_subsystemExplosions.TryExplodeBlock(newPositionInt.X, newPositionInt.Y, newPositionInt.Z, projectile.Value);
                        }
                    }
                }
            }
            foreach (Projectile item in m_projectilesToRemove) {
                if (item.TrailParticleSystem != null) {
                    item.TrailParticleSystem.IsStopped = true;
                }
                if (!item.Safe) {
                    item.OnRemove?.Invoke();
                    item.OnRemoveWithSelf?.Invoke(item);
                }
                m_projectiles.Remove(item);
                ProjectileRemoved?.Invoke(item);
            }
            m_projectilesToRemove.Clear();
        }

        public override void Load(ValuesDictionary valuesDictionary) {
            m_subsystemAudio = Project.FindSubsystem<SubsystemAudio>(true);
            m_subsystemSoundMaterials = Project.FindSubsystem<SubsystemSoundMaterials>(true);
            m_subsystemParticles = Project.FindSubsystem<SubsystemParticles>(true);
            m_subsystemPickables = Project.FindSubsystem<SubsystemPickables>(true);
            m_subsystemBodies = Project.FindSubsystem<SubsystemBodies>(true);
            m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(true);
            m_subsystemSky = Project.FindSubsystem<SubsystemSky>(true);
            m_subsystemTime = Project.FindSubsystem<SubsystemTime>(true);
            m_subsystemNoise = Project.FindSubsystem<SubsystemNoise>(true);
            m_subsystemExplosions = Project.FindSubsystem<SubsystemExplosions>(true);
            m_subsystemGameInfo = Project.FindSubsystem<SubsystemGameInfo>(true);
            m_subsystemBlockBehaviors = Project.FindSubsystem<SubsystemBlockBehaviors>(true);
            m_subsystemFluidBlockBehavior = Project.FindSubsystem<SubsystemFluidBlockBehavior>(true);
            m_subsystemFireBlockBehavior = Project.FindSubsystem<SubsystemFireBlockBehavior>(true);
            foreach (ValuesDictionary item in valuesDictionary.GetValue<ValuesDictionary>("GVProjectiles").Values.Where(v => v is ValuesDictionary)) {
                Projectile projectile = new() {
                    Value = item.GetValue<int>("Value"),
                    Position = item.GetValue<Vector3>("Position"),
                    Velocity = item.GetValue<Vector3>("Velocity"),
                    CreationTime = item.GetValue<double>("CreationTime"),
                    DisableGravity = item.GetValue<bool>("DisableGravity"),
                    DisableDamping = item.GetValue<bool>("DisableDamping"),
                    Transform = item.GetValue<bool>("Transform"),
                    StopAt = item.GetValue<Point3>("StopAt"),
                    Count = item.GetValue<int>("Count")
                };
                projectile.ProjectileStoppedAction = item.GetValue("ProjectileStoppedAction", projectile.ProjectileStoppedAction);
                m_projectiles.Add(projectile);
            }
        }

        public override void Save(ValuesDictionary valuesDictionary) {
            ValuesDictionary valuesDictionary2 = new();
            valuesDictionary.SetValue("GVProjectiles", valuesDictionary2);
            int num = 0;
            foreach (Projectile projectile in m_projectiles) {
                ValuesDictionary valuesDictionary3 = new();
                valuesDictionary2.SetValue(num.ToString(CultureInfo.InvariantCulture), valuesDictionary3);
                valuesDictionary3.SetValue("Value", projectile.Value);
                valuesDictionary3.SetValue("Position", projectile.Position);
                valuesDictionary3.SetValue("Velocity", projectile.Velocity);
                valuesDictionary3.SetValue("CreationTime", projectile.CreationTime);
                valuesDictionary3.SetValue("DisableGravity", projectile.DisableGravity);
                valuesDictionary3.SetValue("DisableDamping", projectile.DisableDamping);
                valuesDictionary3.SetValue("Transform", projectile.Transform);
                valuesDictionary3.SetValue("StopAt", projectile.StopAt);
                valuesDictionary3.SetValue("ProjectileStoppedAction", projectile.ProjectileStoppedAction);
                valuesDictionary3.SetValue("Count", projectile.Count);
                num++;
            }
        }

        public virtual bool IsWater(Vector3 position) {
            int cellContents = m_subsystemTerrain.Terrain.GetCellContents(Terrain.ToCell(position.X), Terrain.ToCell(position.Y), Terrain.ToCell(position.Z));
            return BlocksManager.Blocks[cellContents] is WaterBlock;
        }

        public virtual bool IsMagma(Vector3 position) {
            int cellContents = m_subsystemTerrain.Terrain.GetCellContents(Terrain.ToCell(position.X), Terrain.ToCell(position.Y), Terrain.ToCell(position.Z));
            return BlocksManager.Blocks[cellContents] is MagmaBlock;
        }

        public virtual void MakeProjectileNoise(Projectile projectile) {
            if (m_subsystemTime.GameTime - projectile.LastNoiseTime > 0.5) {
                m_subsystemNoise.MakeNoise(projectile.Position, 0.25f, 6f);
                projectile.LastNoiseTime = m_subsystemTime.GameTime;
            }
        }

        public static void CalculateVelocityAlignMatrix(Block projectileBlock, Vector3 position, Vector3 velocity, out Matrix matrix) {
            matrix = Matrix.Identity;
            matrix.Up = Vector3.Normalize(velocity);
            matrix.Right = Vector3.Normalize(Vector3.Cross(matrix.Up, Vector3.UnitY));
            matrix.Forward = Vector3.Normalize(Vector3.Cross(matrix.Up, matrix.Right));
            matrix.Translation = position;
        }
    }
}