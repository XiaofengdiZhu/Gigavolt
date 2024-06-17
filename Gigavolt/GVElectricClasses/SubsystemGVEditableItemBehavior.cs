using System.Collections.Generic;
using Engine;
using Engine.Serialization;
using TemplatesDatabase;

namespace Game {
    public abstract class SubsystemGVEditableItemBehavior<T> : SubsystemBlockBehavior, IGVBlockBehavior where T : IEditableItemData, new() {
        public SubsystemItemsScanner m_subsystemItemsScanner;
        public SubsystemTerrain m_subsystemTerrain;

        public int m_contents;

        public Dictionary<int, T> m_itemsData = new();
        public HashSet<int> m_existingIds = new();

        public SubsystemGVEditableItemBehavior(int contents) => m_contents = contents;

        //最多12位
        public virtual int GetIdFromValue(int value) => Terrain.ExtractData(value) & 4095;

        public virtual int SetIdToValue(int value, int id) => Terrain.ReplaceData(value, id & 4095);

        public T GetItemData(int id, bool returnNew = false) {
            m_itemsData.TryGetValue(id, out T value);
            if (value == null && returnNew) {
                return new T();
            }
            return value;
        }

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
                if (key2 == 0) {
                    foreach (string item in ((string)item2.Value).Split(',')) {
                        if (int.TryParse(item, out int number)) {
                            m_existingIds.Add(number);
                        }
                    }
                }
                else {
                    T value2 = new();
                    value2.LoadString((string)item2.Value);
                    m_itemsData[key2] = value2;
                }
            }
            m_subsystemItemsScanner.ItemsScanned += GarbageCollectItems;
        }

        public override void Save(ValuesDictionary valuesDictionary) {
            base.Save(valuesDictionary);
            ValuesDictionary valuesDictionary3 = new();
            valuesDictionary.SetValue("Items", valuesDictionary3);
            foreach (KeyValuePair<int, T> itemsDatum in m_itemsData) {
                valuesDictionary3.SetValue(HumanReadableConverter.ConvertToString(itemsDatum.Key), itemsDatum.Value.SaveString());
            }
            valuesDictionary3.SetValue(HumanReadableConverter.ConvertToString(0), string.Join(",", m_existingIds));
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
            }
        }

        public void OnBlockAdded(int value, int oldValue, int x, int y, int z, GVSubterrainSystem system) {
            int id = GetIdFromValue(value);
            if (id > 0) {
                m_existingIds.Add(id);
            }
        }

        public override void OnBlockModified(int value, int oldValue, int x, int y, int z) {
            int oldId = GetIdFromValue(oldValue);
            if (oldId > 0) {
                m_existingIds.Remove(oldId);
            }
            int id = GetIdFromValue(value);
            if (id > 0) {
                m_existingIds.Add(id);
            }
        }

        public void OnBlockModified(int value, int oldValue, int x, int y, int z, GVSubterrainSystem system) {
            int oldId = GetIdFromValue(oldValue);
            if (oldId > 0) {
                m_existingIds.Remove(oldId);
            }
            int id = GetIdFromValue(value);
            if (id > 0) {
                m_existingIds.Add(id);
            }
        }

        public override void OnBlockGenerated(int value, int x, int y, int z, bool isLoaded) {
            int id = GetIdFromValue(value);
            if (id > 0) {
                m_existingIds.Add(id);
            }
        }

        public void OnBlockGenerated(int value, int x, int y, int z, bool isLoaded, GVSubterrainSystem system) {
            int id = GetIdFromValue(value);
            if (id > 0) {
                m_existingIds.Add(id);
            }
        }

        public override void OnBlockRemoved(int value, int newValue, int x, int y, int z) {
            int id = GetIdFromValue(value);
            if (id > 0) {
                m_existingIds.Remove(id);
            }
        }

        public virtual void OnBlockRemoved(int value, int newValue, int x, int y, int z, GVSubterrainSystem system) {
            int id = GetIdFromValue(value);
            if (id > 0) {
                m_existingIds.Remove(id);
            }
        }

        public void GarbageCollectItems(ReadOnlyList<ScannedItemData> allExistingItems) {
            HashSet<int> hashSet = new();
            foreach (ScannedItemData item in allExistingItems) {
                if (Terrain.ExtractContents(item.Value) == m_contents) {
                    hashSet.Add(GetIdFromValue(item.Value));
                }
            }
            List<int> list = new();
            foreach (KeyValuePair<int, T> itemsDatum in m_itemsData) {
                if (!hashSet.Contains(itemsDatum.Key)
                    && !m_existingIds.Contains(itemsDatum.Key)) {
                    list.Add(itemsDatum.Key);
                }
            }
            foreach (int item2 in list) {
                m_itemsData.Remove(item2);
            }
        }
    }
}