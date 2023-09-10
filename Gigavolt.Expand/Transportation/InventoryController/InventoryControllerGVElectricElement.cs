using System;
using System.Collections.Generic;
using System.Linq;
using Engine;

// ReSharper disable PossibleNullReferenceException

namespace Game {
    public class InventoryControllerGVElectricElement : RotateableGVElectricElement {
        public uint m_voltage;
        public uint m_lastBottomInput;
        ComponentInventoryBase m_originInventory;

        public InventoryControllerGVElectricElement(SubsystemGVElectricity subsystemGVElectric, CellFace cellFace) : base(subsystemGVElectric, cellFace) { }

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override bool Simulate() {
            uint voltage = m_voltage;
            bool bottomConnected = false;
            uint rightInput = 0u;
            uint leftInput = 0u;
            uint bottomInput = 0u;
            int rotation = Rotation;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue) {
                        switch (connectorDirection) {
                            case GVElectricConnectorDirection.Bottom:
                                bottomInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                bottomConnected = true;
                                break;
                            case GVElectricConnectorDirection.Right:
                                rightInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                break;
                            case GVElectricConnectorDirection.Left:
                                leftInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                break;
                        }
                    }
                }
            }
            if (bottomConnected) {
                m_voltage = 0xffffffffu;
                if (bottomInput == 0u) {
                    m_lastBottomInput = bottomInput;
                    return m_voltage != voltage;
                }
                if (bottomInput != m_lastBottomInput) {
                    m_lastBottomInput = bottomInput;
                    bool controlPlayerInput = ((rightInput >> 18) & 0x1u) == 1u;
                    int controlPlayerIndexInput = (int)((rightInput >> 19) & 31u);
                    ComponentInventoryBase inventory = null;
                    if (controlPlayerInput) {
                        ReadOnlyList<ComponentPlayer> players = SubsystemGVElectricity.Project.FindSubsystem<SubsystemPlayers>(true).ComponentPlayers;
                        if (controlPlayerIndexInput < players.Count) {
                            ComponentInventory playerInventory = players[controlPlayerIndexInput].ComponentMiner.Inventory as ComponentInventory;
                            inventory = playerInventory;
                        }
                    }
                    else {
                        if (m_originInventory == null) {
                            CellFace cellFace = CellFaces[0];
                            Point3 faceDirection = CellFace.FaceToPoint3(cellFace.Face);
                            m_originInventory = SubsystemGVElectricity.Project.FindSubsystem<SubsystemBlockEntities>(true).GetBlockEntity(cellFace.X - faceDirection.X, cellFace.Y - faceDirection.Y, cellFace.Z - faceDirection.Z)?.Entity.FindComponent<ComponentInventoryBase>();
                            inventory = m_originInventory;
                        }
                    }
                    if (inventory == null) {
                        return m_voltage != voltage;
                    }
                    int sourceSlotInput = (int)(rightInput & 0xffu);
                    bool specifyDataInput = ((rightInput >> 16) & 0x1u) == 1u;
                    int countInput = ((rightInput >> 17) & 0x1u) == 1u ? int.MaxValue : (int)((rightInput >> 8) & 0xffu);
                    int targetSlotInput = (int)((rightInput >> 24) & 0xffu);
                    int contentsInput = (int)(leftInput & 0x3ffu);
                    int dataInput = (int)((leftInput >> 14) & 0x3ffffu);
                    int valueInput = Terrain.MakeBlockValue(contentsInput, 0, dataInput);
                    switch (bottomInput) {
                        case 1u:
                            m_voltage = (uint)inventory.GetSlotValue(sourceSlotInput);
                            break;
                        case 2u:
                            m_voltage = (uint)inventory.GetSlotCount(sourceSlotInput);
                            break;
                        case 3u:
                            m_voltage = (uint)inventory.GetSlotCapacity(sourceSlotInput, inventory.GetSlotValue(sourceSlotInput));
                            break;
                        case 4u:
                            m_voltage = (uint)(inventory.GetSlotCapacity(sourceSlotInput, inventory.GetSlotValue(sourceSlotInput)) - inventory.GetSlotCount(sourceSlotInput));
                            break;
                        case 5u:
                            m_voltage = (uint)inventory.m_slots.Sum(
                                slot => specifyDataInput ? slot.Value == valueInput ? slot.Count : 0 :
                                    Terrain.ExtractContents(slot.Value) == contentsInput ? slot.Count : 0
                            );
                            break;
                        case 6u:
                            m_voltage = (uint)inventory.m_slots.FindIndex(slot => specifyDataInput ? slot.Value == valueInput : Terrain.ExtractContents(slot.Value) == contentsInput);
                            break;
                        case 7u:
                            m_voltage = (uint)inventory.m_slots.Sum(
                                slot => specifyDataInput ? slot.Value == valueInput ? 1 : 0 :
                                    Terrain.ExtractContents(slot.Value) == contentsInput ? 1 : 0
                            );
                            break;
                        case 8u: {
                            int output = 0;
                            for (int i = 0; i < inventory.SlotsCount; i++) {
                                if (inventory.m_slots[i].Count == 0) {
                                    output += inventory.GetSlotCapacity(i, valueInput);
                                }
                                else {
                                    int value = inventory.m_slots[i].Value;
                                    if (value == valueInput) {
                                        output += inventory.GetSlotCapacity(i, valueInput) - inventory.m_slots[i].Count;
                                    }
                                }
                            }
                            m_voltage = (uint)output;
                            break;
                        }
                        case 9u:
                            m_voltage = (uint)inventory.SlotsCount;
                            break;
                        case 10u:
                            m_voltage = (uint)inventory.m_slots.Count(slot => slot.Count > 0);
                            break;
                        case 11u:
                            m_voltage = (uint)inventory.m_slots.Count(slot => slot.Count == 0);
                            break;
                        case 12u:
                            m_voltage = (uint)inventory.m_slots.FindIndex(slot => slot.Count > 0);
                            break;
                        case 13u:
                            m_voltage = (uint)inventory.m_slots.FindLastIndex(slot => slot.Count > 0);
                            break;
                        case 14u:
                            m_voltage = (uint)inventory.m_slots.FindIndex(slot => slot.Count == 0);
                            break;
                        case 15u:
                            m_voltage = (uint)inventory.m_slots.FindLastIndex(slot => slot.Count == 0);
                            break;
                        case 16u: {
                            int sourceValue = inventory.GetSlotValue(sourceSlotInput);
                            int sourceCount = inventory.GetSlotCount(sourceSlotInput);
                            int moveCount = Math.Min(countInput, sourceCount);
                            int targetValue = inventory.GetSlotValue(targetSlotInput);
                            int targetCount = inventory.GetSlotCount(targetSlotInput);
                            bool sourceSlotShouldRemove = false;
                            if (targetCount > 0) {
                                if (targetValue == sourceValue) {
                                    moveCount = Math.Min(moveCount, inventory.GetSlotCapacity(targetSlotInput, targetValue) - targetCount);
                                }
                                else {
                                    moveCount = Math.Min(moveCount, inventory.GetSlotCapacity(targetSlotInput, targetValue));
                                    sourceSlotShouldRemove = sourceCount == moveCount;
                                    int leftCount = targetCount;
                                    Dictionary<int, int> slotsIndex = new Dictionary<int, int>();
                                    for (int i = 0; i < inventory.SlotsCount; i++) {
                                        if ((i != sourceSlotInput || sourceSlotShouldRemove)
                                            && i != targetSlotInput) {
                                            int count = inventory.GetSlotCount(i);
                                            int addCount = 0;
                                            if (count == 0) {
                                                addCount = Math.Min(leftCount, inventory.GetSlotCapacity(i, targetValue));
                                            }
                                            else if (inventory.GetSlotValue(i) == targetValue) {
                                                addCount = Math.Min(leftCount, inventory.GetSlotCapacity(i, targetValue) - count);
                                            }
                                            slotsIndex.Add(i, addCount);
                                            leftCount -= addCount;
                                            if (leftCount <= 0) {
                                                break;
                                            }
                                        }
                                    }
                                    if (leftCount > 0) {
                                        m_voltage = 0u;
                                        return m_voltage != voltage;
                                    }
                                    if (sourceSlotShouldRemove) {
                                        inventory.RemoveSlotItems(sourceSlotInput, moveCount);
                                    }
                                    foreach (KeyValuePair<int, int> pair in slotsIndex) {
                                        inventory.AddSlotItems(pair.Key, targetValue, pair.Value);
                                    }
                                }
                            }
                            else {
                                moveCount = Math.Min(moveCount, inventory.GetSlotCapacity(targetSlotInput, targetValue));
                            }
                            if (moveCount > 0) {
                                if (!sourceSlotShouldRemove) {
                                    inventory.RemoveSlotItems(sourceSlotInput, moveCount);
                                }
                                inventory.AddSlotItems(targetSlotInput, sourceValue, moveCount);
                            }
                            m_voltage = (uint)moveCount;
                            break;
                        }
                        case 17u: {
                            if (valueInput == 0
                                || targetSlotInput >= inventory.SlotsCount) {
                                return m_voltage != voltage;
                            }
                            int totalCount = 0;
                            IEnumerable<int> originalSlotsIndex = inventory.m_slots.Select(
                                    (slot, index) => {
                                        if (slot.Value == valueInput) {
                                            totalCount += slot.Count;
                                            return index;
                                        }
                                        return -1;
                                    }
                                )
                                .Where(index => index > -1);
                            totalCount = Math.Min(totalCount, countInput);
                            int toUseSlotsCount = 0;
                            int toRemoveCount = 0;
                            for (int i = targetSlotInput; i < inventory.SlotsCount; i++) {
                                int capacity = inventory.GetSlotCapacity(i, valueInput);
                                toRemoveCount += capacity;
                                toUseSlotsCount++;
                                if (toRemoveCount >= totalCount) {
                                    break;
                                }
                            }
                            if (toUseSlotsCount == 0) {
                                return m_voltage != voltage;
                            }
                            toRemoveCount = Math.Min(toRemoveCount, totalCount);
                            totalCount = toRemoveCount;
                            List<int> toRemoveSlotsIndex = new List<int>();
                            foreach (int index in originalSlotsIndex) {
                                toRemoveCount -= inventory.GetSlotCount(index);
                                if (toRemoveCount <= 0) {
                                    break;
                                }
                                toRemoveSlotsIndex.Add(index);
                            }
                            Dictionary<int, ComponentInventoryBase.Slot> toMoveSlots = new Dictionary<int, ComponentInventoryBase.Slot>();
                            for (int i = 0; i < toUseSlotsCount; i++) {
                                int index = targetSlotInput + i;
                                int value = inventory.GetSlotValue(index);
                                int count = inventory.GetSlotCount(index);
                                if (value == valueInput) {
                                    continue;
                                }
                                toMoveSlots.Add(index, new ComponentInventoryBase.Slot { Count = count, Value = value });
                            }
                            int removedCount = 0;
                            foreach (KeyValuePair<int, ComponentInventoryBase.Slot> item in toMoveSlots) {
                                int newCapacity = inventory.GetSlotCapacity(item.Key, valueInput);
                                List<int> toRemoveSlotsIndex2 = new List<int>();
                                foreach (int index in toRemoveSlotsIndex) {
                                    newCapacity -= inventory.GetSlotCapacity(index, valueInput);
                                    if (newCapacity < 0) {
                                        break;
                                    }
                                    toRemoveSlotsIndex2.Add(index);
                                }
                                int leftCount = item.Value.Count;
                                int value = item.Value.Value;
                                Dictionary<int, int> toMoveIntoSlots = new Dictionary<int, int>();
                                for (int i = 0; i < inventory.SlotsCount; i++) {
                                    if (i >= targetSlotInput
                                        && i < targetSlotInput + toUseSlotsCount) {
                                        continue;
                                    }
                                    int slotCount = inventory.GetSlotCount(i);
                                    int moveIntoCount = 0;
                                    if (slotCount == 0
                                        || toRemoveSlotsIndex2.Contains(i)) {
                                        moveIntoCount = Math.Min(leftCount, inventory.GetSlotCapacity(i, value));
                                    }
                                    else if (inventory.GetSlotValue(i) == value) {
                                        moveIntoCount = Math.Min(leftCount, inventory.GetSlotCapacity(i, value) - slotCount);
                                    }
                                    toMoveIntoSlots.Add(i, moveIntoCount);
                                    leftCount -= moveIntoCount;
                                    if (leftCount <= 0) {
                                        break;
                                    }
                                }
                                if (leftCount <= 0) {
                                    foreach (int index in toRemoveSlotsIndex2) {
                                        removedCount += inventory.RemoveSlotItems(index, int.MaxValue);
                                    }
                                    foreach (KeyValuePair<int, int> item2 in toMoveIntoSlots) {
                                        inventory.AddSlotItems(item2.Key, item.Value.Value, item2.Value);
                                    }
                                }
                            }
                            foreach (int index in originalSlotsIndex) {
                                int count = inventory.GetSlotCount(index);
                                if (count > 0
                                    && inventory.GetSlotValue(index) == valueInput) {
                                    removedCount += inventory.RemoveSlotItems(index, totalCount - removedCount);
                                }
                            }
                            m_voltage = (uint)removedCount;
                            for (int i = 0; i < toUseSlotsCount; i++) {
                                int index = targetSlotInput + i;
                                int count = inventory.GetSlotCount(index);
                                int addCount = 0;
                                if (count == 0) {
                                    addCount = Math.Min(removedCount, inventory.GetSlotCapacity(index, valueInput));
                                }
                                else if (inventory.GetSlotValue(index) == valueInput) {
                                    addCount = Math.Min(removedCount, inventory.GetSlotCapacity(index, valueInput) - count);
                                }
                                inventory.AddSlotItems(index, valueInput, addCount);
                                removedCount -= addCount;
                            }
                            break;
                        }
                        case 18u:
                            m_voltage = (uint)inventory.RemoveSlotItems(sourceSlotInput, countInput);
                            break;
                        case 19u: {
                            int shouldRemove = countInput;
                            for (int i = 0; i < inventory.SlotsCount; i++) {
                                int value = inventory.GetSlotValue(i);
                                if (specifyDataInput ? value == valueInput : Terrain.ExtractContents(value) == contentsInput) {
                                    shouldRemove -= inventory.RemoveSlotItems(i, shouldRemove);
                                    if (shouldRemove <= 0) {
                                        break;
                                    }
                                }
                            }
                            m_voltage = (uint)(countInput - shouldRemove);
                            break;
                        }
                        case 20u: {
                            int oldCount = inventory.GetSlotCount(sourceSlotInput);
                            int oldValue = inventory.GetSlotValue(sourceSlotInput);
                            int newValue = Terrain.MakeBlockValue(Terrain.ExtractContents(oldValue), 0, dataInput);
                            if (oldCount <= countInput) {
                                inventory.m_slots[sourceSlotInput].Value = newValue;
                                m_voltage = (uint)oldCount;
                            }
                            else if (countInput > 0) {
                                int leftCount = oldCount - countInput;
                                Dictionary<int, int> slotsIndex = new Dictionary<int, int>();
                                for (int i = 0; i < inventory.SlotsCount; i++) {
                                    if (i != sourceSlotInput) {
                                        int count = inventory.GetSlotCount(i);
                                        int addCount = 0;
                                        if (count == 0) {
                                            addCount = Math.Min(leftCount, inventory.GetSlotCapacity(i, oldValue));
                                        }
                                        else if (inventory.GetSlotValue(i) == oldValue) {
                                            addCount = Math.Min(leftCount, inventory.GetSlotCapacity(i, oldValue) - count);
                                        }
                                        slotsIndex.Add(i, addCount);
                                        leftCount -= addCount;
                                        if (leftCount <= 0) {
                                            break;
                                        }
                                    }
                                }
                                if (leftCount > 0) {
                                    m_voltage = 0u;
                                    return m_voltage != voltage;
                                }
                                inventory.m_slots[sourceSlotInput].Value = newValue;
                                inventory.m_slots[sourceSlotInput].Count = countInput;
                                foreach (KeyValuePair<int, int> pair in slotsIndex) {
                                    inventory.AddSlotItems(pair.Key, oldValue, pair.Value);
                                }
                                m_voltage = (uint)countInput;
                            }
                            break;
                        }
                        case 32u: {
                            m_voltage = OrderInventory(inventory, false, false);
                            break;
                        }
                        case 33u: {
                            m_voltage = OrderInventory(inventory, false, true);
                            break;
                        }
                        case 34u: {
                            m_voltage = OrderInventory(inventory, true, false);
                            break;
                        }
                        case 35u: {
                            m_voltage = OrderInventory(inventory, true, true);
                            break;
                        }
                        case 48u:
                            inventory.AddSlotItems(-1, 0, 0);
                            if (inventory is ComponentCraftingTable
                                || inventory is ComponentFurnace) {
                                m_voltage = (uint)inventory.GetSlotCount(inventory.SlotsCount - 2);
                            }
                            break;
                    }
                    return m_voltage != voltage;
                }
            }
            return false;
        }

        public static uint OrderInventory(ComponentInventoryBase inventory, bool orderByValue, bool desc) {
            Dictionary<int, int> allItems = new Dictionary<int, int>();
            foreach (ComponentInventoryBase.Slot slot in inventory.m_slots) {
                if (slot.Count > 0) {
                    if (allItems.TryGetValue(slot.Value, out int count)) {
                        allItems[slot.Value] = count + slot.Count;
                    }
                    else {
                        allItems.Add(slot.Value, slot.Count);
                    }
                }
                slot.Value = 0;
                slot.Count = 0;
            }
            int index = 0;
            foreach (KeyValuePair<int, int> pair in desc ? allItems.OrderByDescending(item => orderByValue ? item.Value : item.Key) : allItems.OrderBy(item => orderByValue ? item.Value : item.Key)) {
                int leftCount = pair.Value;
                while (leftCount > 0
                    && index < inventory.SlotsCount) {
                    int capacity = inventory.GetSlotCapacity(index, pair.Key);
                    int add = Math.Min(leftCount, capacity);
                    inventory.m_slots[index].Count = add;
                    inventory.m_slots[index].Value = pair.Key;
                    leftCount -= add;
                    index++;
                }
                if (leftCount > 0) {
                    Log.Warning("对不起，没有足够的空间存储物品，部分物品遗失了。");
                    break;
                }
            }
            return (uint)(index + 1);
        }

        public override void OnNeighborBlockChanged(CellFace cellFace, int neighborX, int neighborY, int neighborZ) {
            Point3 point = CellFace.FaceToPoint3(cellFace.Face);
            int x = cellFace.X - point.X;
            int y = cellFace.Y - point.Y;
            int z = cellFace.Z - point.Z;
            if (SubsystemGVElectricity.SubsystemTerrain.Terrain.IsCellValid(x, y, z)) {
                int cellValue = SubsystemGVElectricity.SubsystemTerrain.Terrain.GetCellValue(x, y, z);
                Block block = BlocksManager.Blocks[Terrain.ExtractContents(cellValue)];
                if ((!block.IsCollidable_(cellValue) || block.IsFaceTransparent(SubsystemGVElectricity.SubsystemTerrain, cellFace.Face, cellValue))
                    && (cellFace.Face != 4 || !(block is FenceBlock))
                    && SubsystemGVElectricity.Project.FindSubsystem<SubsystemBlockEntities>(true).GetBlockEntity(x, y, z)?.Entity.FindComponent<ComponentInventoryBase>() != null) {
                    SubsystemGVElectricity.SubsystemTerrain.DestroyCell(
                        0,
                        cellFace.X,
                        cellFace.Y,
                        cellFace.Z,
                        0,
                        false,
                        false
                    );
                }
            }
        }
    }
}