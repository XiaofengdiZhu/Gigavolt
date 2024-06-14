using System;
using System.Collections.Generic;
using Engine;

namespace Game {
    public class InventoryFetcherGVElectricElement : GVElectricElement {
        public readonly SubsystemBlockEntities m_subsystemBlockEntities;
        public readonly SubsystemPickables m_subsystemPickables;
        public int m_originType;
        public int m_originFace;
        public uint m_voltage;

        public InventoryFetcherGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, Point3 point, uint subterrainId) : base(
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
        ) {
            if (SubterrainId != 0) {
                return;
            }
            m_subsystemBlockEntities = SubsystemGVElectricity.Project.FindSubsystem<SubsystemBlockEntities>(true);
            m_subsystemPickables = SubsystemGVElectricity.Project.FindSubsystem<SubsystemPickables>(true);
            int originData = Terrain.ExtractData(value);
            m_originType = GVInventoryFetcherBlock.GetType(originData);
            m_originFace = GVInventoryFetcherBlock.GetFace(originData);
        }

        public override bool Simulate() {
            if (SubterrainId != 0) {
                return false;
            }
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
                GVCellFace cellFace = CellFaces[0];
                Point3 originFaceDirection = CellFace.FaceToPoint3(m_originFace);
                ComponentInventoryBase originInventory = m_subsystemBlockEntities.GetBlockEntity(cellFace.X - originFaceDirection.X, cellFace.Y - originFaceDirection.Y, cellFace.Z - originFaceDirection.Z)?.Entity.FindComponent<ComponentInventoryBase>();
                if (originInventory == null) {
                    return false;
                }
                int itemValue;
                int itemCount;
                bool throwOut;
                HashSet<int> itemAtSlots = new();
                if (m_originType == 0) {
                    int slot = (int)(m_voltage & 0xffu);
                    itemValue = originInventory.GetSlotValue(slot);
                    itemCount = Math.Min(m_voltage >> 16 == 1u ? int.MaxValue : (int)((m_voltage >> 8) & 0xffu), originInventory.GetSlotCount(slot));
                    if (itemCount == 0) {
                        return false;
                    }
                    itemAtSlots.Add(slot);
                    throwOut = m_voltage >> 17 == 0u;
                }
                else if (m_originType == 2) {
                    bool specifyData = m_voltage >> 10 == 1u;
                    int itemContents = (int)(m_voltage & 0x3ffu);
                    itemValue = Terrain.MakeBlockValue(itemContents, 0, specifyData ? (int)((m_voltage >> 14) & 0x3ffffu) : 0);
                    itemCount = m_voltage >> 11 == 1u ? int.MaxValue : 1;
                    throwOut = m_voltage >> 12 == 0u;
                    int nowCount = 0;
                    for (int i = 0; i < originInventory.SlotsCount; i++) {
                        int value = originInventory.GetSlotValue(i);
                        if (specifyData ? value == itemValue : Terrain.ExtractContents(value) == itemContents) {
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
                Point3 position = new(cellFace.X, cellFace.Y, cellFace.Z);
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
                            && (face == m_originFace || oppositeFace == m_originFace)) {
                            continue;
                        }
                        if ((type == 0 || type == 2)
                            && oppositeFace == m_originFace) {
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
                            Vector3 originFaceVector3 = CellFace.FaceToVector3(m_originFace);
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