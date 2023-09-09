using System;
using System.Collections.Generic;
using Engine;

namespace Game {
    public class InventoryFetcherGVElectricElement : GVElectricElement {
        public readonly SubsystemBlockEntities m_subsystemBlockEntities;
        public readonly SubsystemPickables m_subsystemPickables;
        public uint m_voltage;

        public InventoryFetcherGVElectricElement(SubsystemGVElectricity subsystemElectricity, Point3 point) : base(
            subsystemElectricity,
            new List<CellFace> {
                new CellFace(point.X, point.Y, point.Z, 0),
                new CellFace(point.X, point.Y, point.Z, 1),
                new CellFace(point.X, point.Y, point.Z, 2),
                new CellFace(point.X, point.Y, point.Z, 3),
                new CellFace(point.X, point.Y, point.Z, 4),
                new CellFace(point.X, point.Y, point.Z, 5)
            }
        ) {
            m_subsystemBlockEntities = SubsystemGVElectricity.Project.FindSubsystem<SubsystemBlockEntities>(true);
            m_subsystemPickables = SubsystemGVElectricity.Project.FindSubsystem<SubsystemPickables>(true);
        }

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
                CellFace cellFace = CellFaces[0];
                int originData = Terrain.ExtractData(SubsystemGVElectricity.SubsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z));
                int originType = GVInventoryFetcherBlock.GetType(originData);
                int originFace = GVInventoryFetcherBlock.GetFace(originData);
                Point3 originFaceDirection = CellFace.FaceToPoint3(originFace);
                ComponentInventoryBase originInventory = m_subsystemBlockEntities.GetBlockEntity(cellFace.X - originFaceDirection.X, cellFace.Y - originFaceDirection.Y, cellFace.Z - originFaceDirection.Z)?.Entity.FindComponent<ComponentInventoryBase>();
                if (originInventory == null) {
                    return false;
                }
                int itemValue;
                int itemCount;
                bool throwOut;
                HashSet<int> itemAtSlots = new HashSet<int>();
                if (originType == 0) {
                    int slot = (int)(m_voltage & 0xffu);
                    itemValue = originInventory.GetSlotValue(slot);
                    itemCount = Math.Min(m_voltage >> 16 == 1u ? int.MaxValue : (int)((m_voltage >> 8) & 0xffu), originInventory.GetSlotCount(slot));
                    if (itemCount == 0) {
                        return false;
                    }
                    itemAtSlots.Add(slot);
                    throwOut = m_voltage >> 17 == 1u;
                }
                else if (originType == 2) {
                    bool specifyData = m_voltage >> 10 == 1u;
                    int itemContents = (int)(m_voltage & 0x3ffu);
                    itemValue = Terrain.MakeBlockValue(itemContents, 0, specifyData ? (int)((m_voltage >> 14) & 0x3ffffu) : 0);
                    itemCount = m_voltage >> 11 == 1u ? int.MaxValue : 1;
                    throwOut = m_voltage >> 12 == 1u;
                    int nowCount = 0;
                    for (int i = 0; i < originInventory.SlotsCount; i++) {
                        int value = originInventory.GetSlotValue(i);
                        if (specifyData ? value == itemValue : value == itemContents) {
                            int count = originInventory.GetSlotCount(i);
                            nowCount += count;
                            itemAtSlots.Add(i);
                            if (nowCount >= itemCount) {
                                break;
                            }
                        }
                    }
                    itemCount = Math.Min(nowCount, itemCount);
                    if (itemCount == 0) {
                        return false;
                    }
                }
                else {
                    return false;
                }
                Point3 position = new Point3(cellFace.X, cellFace.Y, cellFace.Z);
                while (true) {
                    position += originFaceDirection;
                    int value = SubsystemGVElectricity.SubsystemTerrain.Terrain.GetCellValue(position.X, position.Y, position.Z);
                    int contents = Terrain.ExtractContents(value);
                    if (contents == GVInventoryFetcherBlock.Index) {
                        int data = Terrain.ExtractData(value);
                        int type = GVInventoryFetcherBlock.GetType(data);
                        int face = GVInventoryFetcherBlock.GetFace(data);
                        int oppositeFace = CellFace.OppositeFace(face);
                        if (type == 1
                            && (face == originFace || oppositeFace == originFace)) {
                            continue;
                        }
                        if ((type == 0 || type == 2)
                            && oppositeFace == originFace) {
                            ComponentInventoryBase inventory = m_subsystemBlockEntities.GetBlockEntity(position.X + originFaceDirection.X, position.Y + originFaceDirection.Y, position.Z + originFaceDirection.Z)?.Entity.FindComponent<ComponentInventoryBase>();
                            if (inventory != null) {
                                int removedCount = itemCount - ComponentInventoryBase.AcquireItems(inventory, itemValue, itemCount);
                                if (removedCount > 0) {
                                    foreach (int slot in itemAtSlots) {
                                        removedCount -= originInventory.RemoveSlotItems(slot, removedCount);
                                        if (itemCount <= 0) {
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else {
                        ComponentInventoryBase inventory = m_subsystemBlockEntities.GetBlockEntity(position.X, position.Y, position.Z)?.Entity.FindComponent<ComponentInventoryBase>();
                        if (inventory != null) {
                            int removedCount = itemCount - ComponentInventoryBase.AcquireItems(inventory, itemValue, itemCount);
                            if (removedCount > 0) {
                                foreach (int slot in itemAtSlots) {
                                    removedCount -= originInventory.RemoveSlotItems(slot, removedCount);
                                    if (itemCount <= 0) {
                                        break;
                                    }
                                }
                            }
                        }
                        else if (throwOut && !BlocksManager.Blocks[contents].IsCollidable_(value)) {
                            Vector3 originFaceVector3 = CellFace.FaceToVector3(originFace);
                            m_subsystemPickables.AddPickable(
                                itemValue,
                                itemCount,
                                new Vector3(position.X + 0.5f, position.Y + 0.5f, position.Z + 0.5f) - 0.4f * originFaceVector3,
                                1.8f * originFaceVector3,
                                null
                            );
                            foreach (int slot in itemAtSlots) {
                                itemCount -= originInventory.RemoveSlotItems(slot, itemCount);
                                if (itemCount <= 0) {
                                    break;
                                }
                            }
                        }
                    }
                    break;
                }
            }
            return false;
        }
    }
}