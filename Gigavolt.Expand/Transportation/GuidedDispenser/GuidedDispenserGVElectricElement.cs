using System;
using System.Collections.Generic;
using Engine;

namespace Game {
    public class GuidedDispenserGVElectricElement : GVElectricElement {
        public SubsystemBlockEntities m_subsystemBlockEntities;
        public uint m_voltage;

        public GuidedDispenserGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, Point3 point, uint subterrainId) : base(
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
        ) => m_subsystemBlockEntities = SubsystemGVElectricity.Project.FindSubsystem<SubsystemBlockEntities>(true);

        public override bool Simulate() {
            uint voltage = m_voltage;
            m_voltage = 0;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    m_voltage |= connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                }
            }
            if (m_voltage != voltage
                && m_voltage > 0) {
                Point3 position = CellFaces[0].Point;
                ComponentGVDispenser component = m_subsystemBlockEntities.GetBlockEntity(CellFaces[0].Point.X, CellFaces[0].Point.Y, CellFaces[0].Point.Z)?.Entity.FindComponent<ComponentGVDispenser>();
                if (component == null) {
                    return false;
                }
                int data = Terrain.ExtractData(SubsystemGVElectricity.SubsystemGVSubterrain.GetTerrain(SubterrainId).GetCellValue(position.X, position.Y, position.Z));
                int face = GVGuidedDispenserBlock.GetDirection(data);
                int slotIndex = 0;
                int slotValue = 0;
                bool specifiedSlotIndex = ((m_voltage >> 28) & 1u) == 1u;
                if (specifiedSlotIndex) {
                    slotIndex = (int)((m_voltage >> 29) & 7u);
                    slotValue = component.GetSlotValue(slotIndex);
                    if (slotValue == 0) {
                        return false;
                    }
                    int slotCount = component.GetSlotCount(slotIndex);
                    if (slotCount <= 0) {
                        return false;
                    }
                }
                else {
                    for (; slotIndex < component.SlotsCount; slotIndex++) {
                        slotValue = component.GetSlotValue(slotIndex);
                        if (slotValue == 0) {
                            continue;
                        }
                        int slotCount = component.GetSlotCount(slotIndex);
                        if (slotCount <= 0) {
                            continue;
                        }
                        break;
                    }
                    if (slotValue == 0) {
                        return false;
                    }
                }
                int removedCount = component.RemoveSlotItems(slotIndex, 1);
                if (removedCount <= 0) {
                    return false;
                }
                GVDispenserBlock.Mode mode = GVDispenserBlock.GetMode(data);
                if (mode == GVDispenserBlock.Mode.Shoot) {
                    Vector3 origin = new Vector3(position.X + 0.5f, position.Y + 0.5f, position.Z + 0.5f) + 0.6f * CellFace.FaceToVector3(face);
                    int offsetX = (int)(m_voltage & 0xFFu) * (((m_voltage >> 24) & 1u) == 1u ? -1 : 1);
                    int offsetY = (int)((m_voltage >> 8) & 0xFFu) * (((m_voltage >> 25) & 1u) == 1u ? -1 : 1);
                    int offsetZ = (int)((m_voltage >> 16) & 0xFFu) * (((m_voltage >> 26) & 1u) == 1u ? -1 : 1);
                    Point3 target = new(position.X + offsetX, position.Y + offsetY, position.Z + offsetZ);
                    Vector3 direction = GetDirection(origin, new Vector3(target.X + 0.5f, target.Y + 0.5f, target.Z + 0.5f));
                    bool transform = ((m_voltage >> 27) & 1u) == 1u;
                    for (int i = 0; i < removedCount; i++) {
                        component.ShootItem(
                            position,
                            face,
                            slotValue,
                            direction,
                            false,
                            true,
                            transform,
                            transform,
                            target
                        );
                    }
                }
                else {
                    for (int i = 0; i < removedCount; i++) {
                        component.DispenseItem(position, face, slotValue);
                    }
                }
            }
            return false;
        }

        public static Vector3 GetDirection(Vector3 origin, Vector3 target) {
            Vector3 direction = Vector3.Zero;
            double l = Math.Sqrt((double)(target.X - origin.X) * (target.X - origin.X) + (double)(target.Z - origin.Z) * (target.Z - origin.Z));
            double h = target.Y - origin.Y;
            if (l <= 0.05f) {
                direction.X = 0f;
                direction.Z = 0f;
                if (h > 0) {
                    direction.Y = (float)(Math.Sqrt(h / 5) * 10);
                }
                return direction;
            }
            double angle = Math.Atan(Math.Abs((double)(target.X - origin.X) / (target.Z - origin.Z)));
            if (h > l
                || h > 5) {
                double t = Math.Sqrt(h / 5);
                double vy = t * 10;
                double vx = l / t;
                direction.Y = (float)vy;
                direction.X = (float)(vx * Math.Sin(angle) * (target.X > origin.X ? 1 : -1));
                direction.Z = (float)(vx * Math.Cos(angle) * (target.Z > origin.Z ? 1 : -1));
            }
            else if (h < -5) {
                double vx = l / Math.Sqrt(-h / 5);
                direction.X = (float)(vx * Math.Sin(angle) * (target.X > origin.X ? 1 : -1));
                direction.Z = (float)(vx * Math.Cos(angle) * (target.Z > origin.Z ? 1 : -1));
            }
            else {
                double t = Math.Sqrt((l - h) / 5);
                double v = l / t;
                direction.Y = (float)v;
                direction.X = (float)(v * Math.Sin(angle) * (target.X > origin.X ? 1 : -1));
                direction.Z = (float)(v * Math.Cos(angle) * (target.Z > origin.Z ? 1 : -1));
            }
            return direction;
        }
    }
}