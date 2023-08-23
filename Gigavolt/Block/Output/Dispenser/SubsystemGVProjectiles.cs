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

        public List<Projectile> m_projectiles = new List<Projectile>();

        public List<Projectile> m_projectilesToRemove = new List<Projectile>();

        public PrimitivesRenderer3D m_primitivesRenderer = new PrimitivesRenderer3D();

        public Random m_random = new Random();

        public DrawBlockEnvironmentData m_drawBlockEnvironmentData = new DrawBlockEnvironmentData();

        public const float BodyInflateAmount = 0.2f;

        public static readonly int[] m_drawOrders = { 10 };

        public ReadOnlyList<Projectile> Projectiles => new ReadOnlyList<Projectile>(m_projectiles);

        public int[] DrawOrders => m_drawOrders;

        public virtual Action<Projectile> ProjectileAdded { get; set; }

        public virtual Action<Projectile> ProjectileRemoved { get; set; }

        public UpdateOrder UpdateOrder => UpdateOrder.Default;
        public static int? m_crusherBlockValue = null;
        public static int? m_interactorBlockValue = null;
        public static int? m_dataModifierBlockContent = null;

        public virtual Projectile AddProjectile(int value, Vector3 position, Vector3 velocity, Vector3 angularVelocity, ComponentCreature owner, bool disableGravity = false, bool disableDamping = false, bool safe = false, bool transform = false, Point3 stopAt = default) {
            Projectile projectile = new Projectile {
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
                StopAt = stopAt
            };
            m_projectiles.Add(projectile);
            ProjectileAdded?.Invoke(projectile);
            if (owner != null
                && owner.PlayerStats != null) {
                owner.PlayerStats.RangedAttacks++;
            }
            return projectile;
        }

        public virtual Projectile FireProjectile(int value, Vector3 position, Vector3 velocity, Vector3 angularVelocity, ComponentCreature owner, bool disableGravity, bool disableDamping, bool safe, bool transform, Point3 stopAt) {
            int num = Terrain.ExtractContents(value);
            Block block = BlocksManager.Blocks[num];
            Vector3 v = Vector3.Normalize(velocity);
            Vector3 vector = position;
            if (owner != null) {
                Ray3 ray = new Ray3(position + v * 5f, -v);
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
                    (testValue, distance) => BlocksManager.Blocks[Terrain.ExtractContents(testValue)].IsCollidable_(testValue)
                )
                .HasValue) {
                Projectile projectile = AddProjectile(
                    value,
                    vector,
                    velocity,
                    angularVelocity,
                    owner,
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
                        matrix.Translation = projectile.Position;
                    }
                    else {
                        matrix = Matrix.CreateTranslation(projectile.Position);
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
                    TerrainChunk chunkAtCell = m_subsystemTerrain.Terrain.GetChunkAtCell(Terrain.ToCell(projectile.Position.X), Terrain.ToCell(projectile.Position.Z));
                    if (chunkAtCell == null
                        || chunkAtCell.State <= TerrainChunkState.InvalidContents4) {
                        projectile.NoChunk = true;
                        if (projectile.TrailParticleSystem != null) {
                            projectile.TrailParticleSystem.IsStopped = true;
                        }
                    }
                    else {
                        projectile.NoChunk = false;
                        Vector3 position = projectile.Position;
                        Vector3 vector = position + projectile.Velocity * dt;
                        Vector3 v = block.ProjectileTipOffset * Vector3.Normalize(projectile.Velocity);
                        bool isCrusher = m_crusherBlockValue.HasValue && m_crusherBlockValue.Value == projectile.Value;
                        bool isInteractor = m_interactorBlockValue.HasValue && m_interactorBlockValue.Value == projectile.Value;
                        bool isDataModifier = m_dataModifierBlockContent.HasValue && m_dataModifierBlockContent.Value == Terrain.ExtractContents(projectile.Value);
                        if (projectile.Transform
                            && block.IsPlaceable
                            && projectile.StopAt.Y > 0) {
                            int x = (int)position.X - 1;
                            int y = (int)position.Y;
                            int z = (int)position.Z;
                            if (projectile.StopAt.X == x
                                && projectile.StopAt.Y == y
                                && projectile.StopAt.Z == z) {
                                m_subsystemTerrain.ChangeCell(x, y, z, projectile.Value);
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
                        }
                        BodyRaycastResult? bodyRaycastResult = m_subsystemBodies.Raycast(position + v, vector + v, 0.2f, (body, distance) => true);
                        TerrainRaycastResult? terrainRaycastResult = m_subsystemTerrain.Raycast(
                            position + v,
                            vector + v,
                            isInteractor,
                            true,
                            (value, distance) => isCrusher || isInteractor || isDataModifier || BlocksManager.Blocks[Terrain.ExtractContents(value)].IsCollidable_(value)
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
                            vector = position;
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
                                blockBehaviors2[j].OnHitByProjectile(cellFace2, projectile);
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
                                Vector3 vector3 = projectile.Position - 0.75f * Vector3.Normalize(projectile.Velocity);
                                for (int k = 0; k < 8; k++) {
                                    Vector3 v2 = k == 0 ? Vector3.Normalize(projectile.Velocity) : m_random.Vector3(1.5f);
                                    TerrainRaycastResult? terrainRaycastResult2 = m_subsystemTerrain.Raycast(
                                        vector3,
                                        vector3 + v2,
                                        false,
                                        true,
                                        (value, distance) => true
                                    );
                                    if (terrainRaycastResult2.HasValue) {
                                        m_subsystemFireBlockBehavior.SetCellOnFire(terrainRaycastResult2.Value.CellFace.X, terrainRaycastResult2.Value.CellFace.Y, terrainRaycastResult2.Value.CellFace.Z, 1f);
                                    }
                                }
                            }
                            if (projectile.Transform
                                && !projectile.ToRemove) {
                                int tempX = (int)position.X - 1;
                                int tempY = (int)position.Y;
                                int tempZ = (int)position.Z;
                                if (isCrusher) {
                                    m_subsystemTerrain.DestroyCell(
                                        int.MaxValue,
                                        tempX,
                                        tempY,
                                        tempZ,
                                        0,
                                        false,
                                        false
                                    );
                                    projectile.ToRemove = true;
                                }
                                else if (isInteractor) {
                                    SubsystemBlockBehavior[] blockBehaviors = m_subsystemBlockBehaviors.GetBlockBehaviors(Terrain.ExtractContents(m_subsystemTerrain.Terrain.GetCellValue(tempX, tempY, tempZ)));
                                    for (int i = 0; i < blockBehaviors.Length; i++) {
                                        blockBehaviors[i].OnInteract(new TerrainRaycastResult { CellFace = new CellFace(tempX, tempY, tempZ, 0) }, null);
                                    }
                                    projectile.ToRemove = true;
                                }
                                else if (isDataModifier) {
                                    int tempContent = Terrain.ExtractContents(m_subsystemTerrain.Terrain.GetCellValue(tempX, tempY, tempZ));
                                    if (tempContent != 0) {
                                        m_subsystemTerrain.ChangeCell(tempX, tempY, tempZ, Terrain.MakeBlockValue(tempContent, m_subsystemTerrain.Terrain.GetCellLightFast(tempX, tempY, tempZ), Terrain.ExtractData(projectile.Value)));
                                    }
                                    projectile.ToRemove = true;
                                }
                                else if (block.IsPlaceable) {
                                    m_subsystemTerrain.ChangeCell(tempX, tempY, tempZ, projectile.Value);
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
                                vector = position;
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
                                m_subsystemParticles.AddParticleSystem(block.CreateDebrisParticleSystem(m_subsystemTerrain, projectile.Position, projectile.Value, 1f));
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
                                                1,
                                                projectile.Position,
                                                Vector3.Zero,
                                                matrix
                                            );
                                        }
                                        else {
                                            m_subsystemPickables.AddPickable(
                                                num4,
                                                1,
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
                        float num5 = projectile.IsInWater ? MathUtils.Pow(0.001f, dt) : MathUtils.Pow(block.GetProjectileDamping(projectile.Value), dt);
                        if (!projectile.DisableGravity) {
                            projectile.Velocity.Y += -10f * dt;
                        }
                        if (!projectile.DisableDamping) {
                            projectile.Velocity *= num5;
                        }
                        projectile.AngularVelocity *= num5;
                        projectile.Position = vector;
                        projectile.Rotation += projectile.AngularVelocity * dt;
                        if (projectile.TrailParticleSystem != null) {
                            if (!m_subsystemParticles.ContainsParticleSystem((ParticleSystemBase)projectile.TrailParticleSystem)) {
                                m_subsystemParticles.AddParticleSystem((ParticleSystemBase)projectile.TrailParticleSystem);
                            }
                            Vector3 v4 = projectile.TrailOffset != Vector3.Zero ? Vector3.TransformNormal(projectile.TrailOffset, Matrix.CreateFromAxisAngle(Vector3.Normalize(projectile.Rotation), projectile.Rotation.Length())) : Vector3.Zero;
                            projectile.TrailParticleSystem.Position = projectile.Position + v4;
                            if (projectile.IsInWater) {
                                projectile.TrailParticleSystem.IsStopped = true;
                            }
                        }
                        bool flag2 = IsWater(projectile.Position);
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
                                float? surfaceHeight = m_subsystemFluidBlockBehavior.GetSurfaceHeight(Terrain.ToCell(projectile.Position.X), Terrain.ToCell(projectile.Position.Y), Terrain.ToCell(projectile.Position.Z));
                                if (surfaceHeight.HasValue) {
                                    m_subsystemParticles.AddParticleSystem(new WaterSplashParticleSystem(m_subsystemTerrain, new Vector3(projectile.Position.X, surfaceHeight.Value, projectile.Position.Z), false));
                                    m_subsystemAudio.PlayRandomSound(
                                        "Audio/Splashes",
                                        1f,
                                        m_random.Float(-0.2f, 0.2f),
                                        projectile.Position,
                                        6f,
                                        true
                                    );
                                    MakeProjectileNoise(projectile);
                                }
                            }
                            projectile.IsInWater = flag2;
                        }
                        if (IsMagma(projectile.Position)) {
                            m_subsystemParticles.AddParticleSystem(new MagmaSplashParticleSystem(m_subsystemTerrain, projectile.Position, false));
                            m_subsystemAudio.PlayRandomSound(
                                "Audio/Sizzles",
                                1f,
                                m_random.Float(-0.2f, 0.2f),
                                projectile.Position,
                                3f,
                                true
                            );
                            projectile.ToRemove = true;
                            m_subsystemExplosions.TryExplodeBlock(Terrain.ToCell(projectile.Position.X), Terrain.ToCell(projectile.Position.Y), Terrain.ToCell(projectile.Position.Z), projectile.Value);
                        }
                        if (m_subsystemTime.PeriodicGameTimeEvent(1.0, projectile.GetHashCode() % 100 / 100.0)
                            && (m_subsystemFireBlockBehavior.IsCellOnFire(Terrain.ToCell(projectile.Position.X), Terrain.ToCell(projectile.Position.Y + 0.1f), Terrain.ToCell(projectile.Position.Z)) || m_subsystemFireBlockBehavior.IsCellOnFire(Terrain.ToCell(projectile.Position.X), Terrain.ToCell(projectile.Position.Y + 0.1f) - 1, Terrain.ToCell(projectile.Position.Z)))) {
                            m_subsystemAudio.PlayRandomSound(
                                "Audio/Sizzles",
                                1f,
                                m_random.Float(-0.2f, 0.2f),
                                projectile.Position,
                                3f,
                                true
                            );
                            projectile.ToRemove = true;
                            m_subsystemExplosions.TryExplodeBlock(Terrain.ToCell(projectile.Position.X), Terrain.ToCell(projectile.Position.Y), Terrain.ToCell(projectile.Position.Z), projectile.Value);
                        }
                    }
                }
            }
            foreach (Projectile item in m_projectilesToRemove) {
                if (item.TrailParticleSystem != null) {
                    item.TrailParticleSystem.IsStopped = true;
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
                Projectile projectile = new Projectile {
                    Value = item.GetValue<int>("Value"),
                    Position = item.GetValue<Vector3>("Position"),
                    Velocity = item.GetValue<Vector3>("Velocity"),
                    CreationTime = item.GetValue<double>("CreationTime"),
                    DisableGravity = item.GetValue<bool>("DisableGravity"),
                    DisableDamping = item.GetValue<bool>("DisableDamping"),
                    Transform = item.GetValue<bool>("Transform"),
                    StopAt = item.GetValue<Point3>("StopAt")
                };
                projectile.ProjectileStoppedAction = item.GetValue("ProjectileStoppedAction", projectile.ProjectileStoppedAction);
                m_projectiles.Add(projectile);
            }
        }

        public override void Save(ValuesDictionary valuesDictionary) {
            ValuesDictionary valuesDictionary2 = new ValuesDictionary();
            valuesDictionary.SetValue("GVProjectiles", valuesDictionary2);
            int num = 0;
            foreach (Projectile projectile in m_projectiles) {
                ValuesDictionary valuesDictionary3 = new ValuesDictionary();
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