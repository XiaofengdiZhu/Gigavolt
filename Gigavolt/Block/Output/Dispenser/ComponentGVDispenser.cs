using System;
using Engine;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game {
    public class ComponentGVDispenser : ComponentInventoryBase {
        public SubsystemTerrain m_subsystemTerrain;

        public SubsystemAudio m_subsystemAudio;

        public SubsystemPickables m_subsystemPickables;

        public SubsystemGVProjectiles m_subsystemGVProjectiles;
        public SubsystemProjectiles m_subsystemProjectiles;

        public ComponentBlockEntity m_componentBlockEntity;

        public virtual void Dispense(uint param) {
            Point3 coordinates = m_componentBlockEntity.Coordinates;
            int data = Terrain.ExtractData(m_subsystemTerrain.Terrain.GetCellValue(coordinates.X, coordinates.Y, coordinates.Z));
            int face = GVDispenserBlock.GetDirection(data);
            int slotIndex = 0;
            int slotValue = 0;
            bool specifiedSlotIndex = ((param >> 24) & 1u) == 1u;
            if (specifiedSlotIndex) {
                slotIndex = (int)((param >> 25) & 7u);
                slotValue = GetSlotValue(slotIndex);
                if (slotValue == 0) {
                    return;
                }
                int slotCount = GetSlotCount(slotIndex);
                if (slotCount <= 0) {
                    return;
                }
            }
            else {
                for (; slotIndex < SlotsCount; slotIndex++) {
                    slotValue = GetSlotValue(slotIndex);
                    if (slotValue == 0) {
                        continue;
                    }
                    int slotCount = GetSlotCount(slotIndex);
                    if (slotCount <= 0) {
                        continue;
                    }
                    break;
                }
                if (slotValue == 0) {
                    return;
                }
            }
            int removedCount = RemoveSlotItems(slotIndex, 1);
            if (removedCount <= 0) {
                return;
            }
            GVDispenserBlock.Mode mode = GVDispenserBlock.GetMode(data);
            if (mode == GVDispenserBlock.Mode.Shoot) {
                float velocity = param & 0xFFu;
                if (velocity <= 0) {
                    return;
                }
                double radiusX = MathUtils.DegToRad((double)MathUint.Min((param >> 8) & 0x7Fu, 90) * (((param >> 15) & 1u) == 1u ? -1 : 1));
                double radiusY = MathUtils.DegToRad((double)MathUint.Min((param >> 16) & 0x7Fu, 90) * (((param >> 23) & 1u) == 1u ? -1 : 1));
                Vector3 direction = Vector3.Zero;
                Vector3 forward = CellFace.FaceToVector3(face);
                if (forward.Y != 0) {
                    direction.Y = forward.Y;
                    direction.X = (float)Math.Tan(radiusX);
                    direction.Z = (float)Math.Tan(radiusY);
                }
                else if (forward.X != 0) {
                    direction.X = forward.X;
                    direction.Z = (float)Math.Tan(radiusX) * (forward.X > 0 ? 1 : -1);
                    direction.Y = (float)Math.Tan(radiusY);
                }
                else {
                    direction.Z = forward.Z;
                    direction.X = (float)Math.Tan(radiusX) * (forward.Z > 0 ? -1 : 1);
                    direction.Y = (float)Math.Tan(radiusY);
                }
                direction = Vector3.Normalize(direction) * velocity;
                bool disableGravity = ((param >> 28) & 1u) == 1u;
                bool disableDamping = ((param >> 29) & 1u) == 1u;
                bool transform = ((param >> 30) & 1u) == 1u;
                ShootItem(
                    coordinates,
                    face,
                    slotValue,
                    direction,
                    disableGravity,
                    disableDamping,
                    transform
                );
            }
            else {
                for (int i = 0; i < removedCount; i++) {
                    DispenseItem(coordinates, face, slotValue);
                }
            }
        }

        public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap) {
            base.Load(valuesDictionary, idToEntityMap);
            m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(true);
            m_subsystemAudio = Project.FindSubsystem<SubsystemAudio>(true);
            m_subsystemPickables = Project.FindSubsystem<SubsystemPickables>(true);
            m_subsystemGVProjectiles = Project.FindSubsystem<SubsystemGVProjectiles>(true);
            m_subsystemProjectiles = Project.FindSubsystem<SubsystemProjectiles>(true);
            m_componentBlockEntity = Entity.FindComponent<ComponentBlockEntity>(true);
        }

        public void DispenseItem(Point3 point, int face, int value) {
            Vector3 vector = CellFace.FaceToVector3(face);
            Vector3 position = new Vector3(point.X + 0.5f, point.Y + 0.5f, point.Z + 0.5f) + 0.6f * vector;
            m_subsystemPickables.AddPickable(
                value,
                1,
                position,
                1.8f * vector,
                null
            );
            m_subsystemAudio.PlaySound(
                "Audio/DispenserDispense",
                1f,
                0f,
                new Vector3(position.X, position.Y, position.Z),
                3f,
                true
            );
        }

        public void ShootItem(Point3 point, int face, int value, Vector3 velocity, bool disableGravity, bool disableDamping, bool transform) {
            Vector3 position = new Vector3(point.X + 0.5f, point.Y + 0.5f, point.Z + 0.5f) + 0.6f * CellFace.FaceToVector3(face);
            bool flag = false;
            if (disableGravity
                || disableDamping
                || transform) {
                flag = m_subsystemGVProjectiles.FireProjectile(
                        value,
                        position,
                        velocity,
                        Vector3.Zero,
                        null,
                        disableGravity,
                        disableDamping,
                        transform
                    )
                    != null;
            }
            else {
                flag = m_subsystemProjectiles.FireProjectile(
                        value,
                        position,
                        velocity,
                        Vector3.Zero,
                        null
                    )
                    != null;
            }
            if (flag) {
                m_subsystemAudio.PlaySound(
                    "Audio/DispenserShoot",
                    1f,
                    0f,
                    new Vector3(position.X, position.Y, position.Z),
                    4f,
                    true
                );
            }
            else {
                DispenseItem(point, face, value);
            }
        }
    }
}