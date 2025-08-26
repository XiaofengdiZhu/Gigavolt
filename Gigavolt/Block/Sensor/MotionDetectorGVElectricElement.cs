using System;
using Engine;

namespace Game {
    public class MotionDetectorGVElectricElement : MountedGVElectricElement {
        public const float m_range = 8f;
        public const float m_speedThreshold = 0.25f;
        public const float m_pollingPeriod = 0.25f;

        public readonly SubsystemBodies m_subsystemBodies;
        public readonly SubsystemMovingBlocks m_subsystemMovingBlocks;
        public readonly SubsystemProjectiles m_subsystemProjectiles;
        public readonly SubsystemGVProjectiles m_subsystemGVProjectiles;
        public readonly SubsystemPickables m_subsystemPickables;
        public readonly GVSubterrainSystem m_subterrainSystem;

        public uint m_voltage;

        public Vector3 m_center;
        public Vector3 m_direction;
        public Vector2 m_corner1;
        public Vector2 m_corner2;

        public Vector3 m_centerTransformed;
        public Vector3 m_directionTransformed;
        public Vector2 m_corner2Transformed;
        public Vector2 m_corner1Transformed;

        public readonly DynamicArray<ComponentBody> m_bodies = new();

        public MotionDetectorGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, uint subterrainId) : base(
            subsystemGVElectricity,
            cellFace,
            subterrainId
        ) {
            m_subsystemBodies = subsystemGVElectricity.Project.FindSubsystem<SubsystemBodies>(true);
            m_subsystemMovingBlocks = subsystemGVElectricity.Project.FindSubsystem<SubsystemMovingBlocks>(true);
            m_subsystemProjectiles = subsystemGVElectricity.Project.FindSubsystem<SubsystemProjectiles>(true);
            m_subsystemGVProjectiles = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVProjectiles>(true);
            m_subsystemPickables = subsystemGVElectricity.Project.FindSubsystem<SubsystemPickables>(true);
            m_center = new Vector3(cellFace.X, cellFace.Y, cellFace.Z) + new Vector3(0.5f) - 0.25f * m_direction;
            m_direction = CellFace.FaceToVector3(cellFace.Face);
            Vector3 vector = Vector3.One - new Vector3(MathF.Abs(m_direction.X), MathF.Abs(m_direction.Y), MathF.Abs(m_direction.Z));
            Vector3 vector2 = m_center - 8f * vector;
            Vector3 vector3 = m_center + 8f * (vector + m_direction);
            m_corner1 = new Vector2(vector2.X, vector2.Z);
            m_corner2 = new Vector2(vector3.X, vector3.Z);
            if (subterrainId == 0) {
                m_centerTransformed = m_center;
                m_directionTransformed = m_direction;
                m_corner1Transformed = m_corner1;
                m_corner2Transformed = m_corner2;
                m_subterrainSystem = null;
            }
            else {
                m_subterrainSystem = GVStaticStorage.GVSubterrainSystemDictionary[subterrainId];
            }
        }

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override bool Simulate() {
            if (SubterrainId != 0) {
                Matrix transform = m_subterrainSystem.GlobalTransform;
                m_centerTransformed = Vector3.Transform(m_center, transform);
                m_directionTransformed = Vector3.TransformNormal(m_direction, transform) - Vector3.TransformNormal(Vector3.Zero, transform);
                m_corner1Transformed = Vector3.Transform(new Vector3(m_corner1Transformed.X, 0f, m_corner1Transformed.Y), transform).XZ;
                m_corner2Transformed = Vector3.Transform(new Vector3(m_corner2Transformed.X, 0f, m_corner2Transformed.Y), transform).XZ;
            }
            uint voltage = m_voltage;
            m_voltage = CalculateMotionVoltage();
            if (m_voltage > 0
                && voltage == 0f) {
                SubsystemGVElectricity.SubsystemAudio.PlaySound("Audio/MotionDetectorClick", 1f, 0f, m_centerTransformed, 1f, true);
            }
            SubsystemGVElectricity.QueueGVElectricElementForSimulation(
                this,
                SubsystemGVElectricity.CircuitStep + MathUtils.Max((int)(0.25f * (0.9f + 0.000200000009f * (GetHashCode() % 1000)) / 0.01f), 1)
            );
            return m_voltage != voltage;
        }

        public uint CalculateMotionVoltage() {
            float result = 0f;
            m_bodies.Clear();
            m_subsystemBodies.FindBodiesInArea(m_corner1Transformed, m_corner2Transformed, m_bodies);
            for (int i = 0; i < m_bodies.Count; i++) {
                ComponentBody componentBody = m_bodies.Array[i];
                if (componentBody.Velocity.LengthSquared() >= 0.0625f) {
                    result = MathUtils.Max(result, TestPoint(componentBody.Position + new Vector3(0f, 0.5f * componentBody.BoxSize.Y, 0f)));
                }
            }
            foreach (IMovingBlockSet movingBlockSet in m_subsystemMovingBlocks.MovingBlockSets) {
                if (movingBlockSet.CurrentVelocity.LengthSquared() < 0.0625f
                    || BoundingBox.Distance(movingBlockSet.BoundingBox(false), m_centerTransformed) > 8f) {
                    continue;
                }
                foreach (MovingBlock block in movingBlockSet.Blocks) {
                    result = MathUtils.Max(result, TestPoint(movingBlockSet.Position + new Vector3(block.Offset) + new Vector3(0.5f)));
                }
            }
            foreach (Projectile projectile in m_subsystemProjectiles.Projectiles) {
                if (!(projectile.Velocity.LengthSquared() < 0.0625f)) {
                    result = MathUtils.Max(result, TestPoint(projectile.Position));
                }
            }
            foreach (SubsystemGVProjectiles.Projectile projectile in m_subsystemGVProjectiles.m_projectiles) {
                if (!(projectile.Velocity.LengthSquared() < 0.0625f)) {
                    result = MathUtils.Max(result, TestPoint(projectile.Position));
                }
            }
            foreach (Pickable pickable in m_subsystemPickables.Pickables) {
                if (!(pickable.Velocity.LengthSquared() < 0.0625f)) {
                    result = MathUtils.Max(result, TestPoint(pickable.Position));
                }
            }
            if (!(result > 0f)) {
                return 0u;
            }
            return (uint)MathF.Round(MathUtils.Lerp(0.51f, 1f, MathUtils.Saturate(result * 1.1f)) * 15f);
        }

        public float TestPoint(Vector3 p) {
            float num = Vector3.DistanceSquared(p, m_centerTransformed);
            if (num < 64f
                && Vector3.Dot(Vector3.Normalize(p - (m_centerTransformed - 0.75f * m_directionTransformed)), m_directionTransformed) > 0.5f
                && !SubsystemGVElectricity.SubsystemTerrain.Raycast(
                        m_centerTransformed,
                        p,
                        false,
                        true,
                        delegate(int value, float _) {
                            Block block = BlocksManager.Blocks[Terrain.ExtractContents(value)];
                            return block.IsCollidable
                                && block.BlockIndex != 15
                                && block.BlockIndex != 60
                                && block.BlockIndex != 44
                                && block.BlockIndex != 18;
                        }
                    )
                    .HasValue) {
                return MathUtils.Saturate(1f - MathF.Sqrt(num) / 8f);
            }
            return 0f;
        }
    }
}