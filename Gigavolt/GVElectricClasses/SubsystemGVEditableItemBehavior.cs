using System.Collections.Generic;
using Engine;
using Engine.Serialization;
using TemplatesDatabase;

namespace Game {
    public abstract class SubsystemGVEditableItemBehavior<T> : SubsystemBlockBehavior where T : IEditableItemData, new() {
        public SubsystemItemsScanner m_subsystemItemsScanner;

        public int m_contents;

        public Dictionary<int, T> m_itemsData = new Dictionary<int, T>();

        public SubsystemGVEditableItemBehavior(int contents) => m_contents = contents;

        //最多12位
        public virtual int GetIdFromValue(int value) => Terrain.ExtractData(value) & 4095;

        public virtual int SetIdToValue(int value, int id) => Terrain.ReplaceData(value, id & 4095);

        public T GetItemData(int id) {
            m_itemsData.TryGetValue(id, out T value);
            return value;
        }

        public int StoreItemDataAtUniqueId(T t, int oldId = 0) {
            int num = FindFreeItemId();
            m_itemsData[num] = t;
            if (oldId > 0) {
                m_itemsData.Remove(oldId);
            }
            return num;
        }

        public override void Load(ValuesDictionary valuesDictionary) {
            base.Load(valuesDictionary);
            m_subsystemItemsScanner = Project.FindSubsystem<SubsystemItemsScanner>(true);
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

        public void GarbageCollectItems(ReadOnlyList<ScannedItemData> allExistingItems) {
            HashSet<int> hashSet = new HashSet<int>();
            foreach (ScannedItemData item in allExistingItems) {
                if (Terrain.ExtractContents(item.Value) == m_contents) {
                    hashSet.Add(GetIdFromValue(item.Value));
                }
            }
            List<int> list = new List<int>();
            foreach (KeyValuePair<int, T> itemsDatum in m_itemsData) {
                if (!hashSet.Contains(itemsDatum.Key)) {
                    list.Add(itemsDatum.Key);
                }
            }
            foreach (int item2 in list) {
                m_itemsData.Remove(item2);
            }
        }
    }
}