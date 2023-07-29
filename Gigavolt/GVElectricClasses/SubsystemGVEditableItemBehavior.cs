using System;
using System.Collections.Generic;
using Engine;
using Engine.Serialization;
using TemplatesDatabase;

namespace Game {
    public abstract class SubsystemGVEditableItemBehavior<T> : SubsystemBlockBehavior where T : IEditableItemData, new() {
        public SubsystemItemsScanner m_subsystemItemsScanner;
        public SubsystemTerrain m_subsystemTerrain;

        public int m_contents;

        public Dictionary<int, T> m_itemsData = new Dictionary<int, T>();
        public HashSet<int> m_existingIds = new HashSet<int>();

        public SubsystemGVEditableItemBehavior(int contents) => m_contents = contents;

        //最多12位
        public virtual int GetIdFromValue(int value) => Terrain.ExtractData(value) & 4095;

        public virtual int SetIdToValue(int value, int id) => Terrain.ReplaceData(value, id & 4095);

        public T GetItemData(int id, bool returnNew = false) {
            m_itemsData.TryGetValue(id, out T value);
            if (value == null && returnNew) {
                //Debugger.Log(0, "SubsystemGVEditableItemBehavior", $"{GetType().FullName}.GetItemData: 未找到{id}，完整ID列表：{string.Join(",", m_itemsData.Keys)}\n");
                return new T();
            }
            return value;
        }

        public T GetItemData(int x, int y, int z, bool returnNew = false) {
            int value = m_subsystemTerrain.Terrain.GetCellValue(x, y, z);
            if (Array.IndexOf(HandledBlocks, Terrain.ExtractContents(value)) == -1) {
                if (returnNew) {
                    return new T();
                }
                return default;
            }
            int id = GetIdFromValue(value);
            return GetItemData(id, returnNew);
        }

        public T GetItemData(Point3 position, bool returnNew = false) => GetItemData(position.X, position.Y, position.Z, returnNew);

        public int StoreItemDataAtUniqueId(T t, int oldId = 0) {
            int num = FindFreeItemId();
            m_itemsData[num] = t;
            if (oldId > 0
                && num != oldId) {
                m_itemsData.Remove(oldId);
            }
            return num;
        }

        public override void Load(ValuesDictionary valuesDictionary) {
            base.Load(valuesDictionary);
            m_subsystemItemsScanner = Project.FindSubsystem<SubsystemItemsScanner>(true);
            m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(true);
            foreach (KeyValuePair<string, object> item2 in valuesDictionary.GetValue<ValuesDictionary>("Items")) {
                int key2 = HumanReadableConverter.ConvertFromString<int>(item2.Key);
                T value2 = new T();
                value2.LoadString((string)item2.Value);
                m_itemsData[key2] = value2;
            }
            m_subsystemItemsScanner.ItemsScanned += GarbageCollectItems;
        }

        public override void Save(ValuesDictionary valuesDictionary) {
            base.Save(valuesDictionary);
            ValuesDictionary valuesDictionary3 = new ValuesDictionary();
            valuesDictionary.SetValue("Items", valuesDictionary3);
            foreach (KeyValuePair<int, T> itemsDatum in m_itemsData) {
                valuesDictionary3.SetValue(HumanReadableConverter.ConvertToString(itemsDatum.Key), itemsDatum.Value.SaveString());
            }
        }

        public int FindFreeItemId() {
            for (int i = 1; i < 4095; i++) {
                if (!m_itemsData.ContainsKey(i)) {
                    return i;
                }
            }
            return 0;
        }

        public override void OnBlockAdded(int value, int oldValue, int x, int y, int z) {
            int id = GetIdFromValue(value);
            if (id > 0) {
                m_existingIds.Add(id);
                //Debugger.Log(0, "SubsystemGVEditableItemBehavior", $"{GetType().FullName}.OnBlockAdded: {id}\n");
            }
        }

        public override void OnBlockModified(int value, int oldValue, int x, int y, int z) {
            int id = GetIdFromValue(value);
            if (id > 0) {
                m_existingIds.Add(id);
                //Debugger.Log(0, "SubsystemGVEditableItemBehavior", $"{GetType().FullName}.OnBlockModified: {id}\n");
            }
        }

        public override void OnBlockGenerated(int value, int x, int y, int z, bool isLoaded) {
            int id = GetIdFromValue(value);
            if (id > 0) {
                m_existingIds.Add(GetIdFromValue(value));
                //Debugger.Log(0, "SubsystemGVEditableItemBehavior", $"{GetType().FullName}.OnBlockGenerated: {id}\n");
            }
        }

        public override void OnBlockRemoved(int value, int newValue, int x, int y, int z) {
            int id = GetIdFromValue(value);
            if (id > 0) {
                m_existingIds.Remove(id);
                //Debugger.Log(0, "SubsystemGVEditableItemBehavior", $"{GetType().FullName}.OnBlockRemoved: {id}\n");
            }
        }

        public void GarbageCollectItems(ReadOnlyList<ScannedItemData> allExistingItems) {
            HashSet<int> hashSet = new HashSet<int>();
            foreach (ScannedItemData item in allExistingItems) {
                if (Terrain.ExtractContents(item.Value) == m_contents) {
                    hashSet.Add(GetIdFromValue(item.Value));
                }
            }
            //Debugger.Log(0, "SubsystemGVEditableItemBehavior", $"{GetType().FullName}.GarbageCollectItems: 背包有以下ID: {string.Join(",", hashSet)}\n");
            //Debugger.Log(0, "SubsystemGVEditableItemBehavior", $"{GetType().FullName}.GarbageCollectItems: 世界有以下ID: {string.Join(",", m_existingIds)}\n");
            List<int> list = new List<int>();
            foreach (KeyValuePair<int, T> itemsDatum in m_itemsData) {
                if (!hashSet.Contains(itemsDatum.Key)
                    && !m_existingIds.Contains(itemsDatum.Key)) {
                    list.Add(itemsDatum.Key);
                }
            }
            //Debugger.Log(0, "SubsystemGVEditableItemBehavior", $"{GetType().FullName}.GarbageCollectItems: 移除以下ID: {string.Join(",", list)}\n");
            foreach (int item2 in list) {
                m_itemsData.Remove(item2);
            }
        }
    }
}