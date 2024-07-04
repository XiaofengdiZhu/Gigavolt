using System.Collections.Generic;
using System.Linq;
using Engine;
using Engine.Serialization;
using TemplatesDatabase;

namespace Game {
    public abstract class SubsystemGVEditableItemBehavior<T> : SubsystemBlockBehavior, IGVBlockBehavior where T : IEditableItemData, new() {
        public SubsystemItemsScanner m_subsystemItemsScanner;
        public SubsystemTerrain m_subsystemTerrain;

        public readonly int m_contents;

        public readonly Dictionary<int, T> m_itemsData = new();
        public readonly HashSet<int> m_existingIds = [];

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
            foreach (KeyValuePair<string, object> item in valuesDictionary.GetValue<ValuesDictionary>("Items")) {
                int id = HumanReadableConverter.ConvertFromString<int>(item.Key);
                if (id == 0) {
                    foreach (string numberString in ((string)item.Value).Split(',')) {
                        if (int.TryParse(numberString, out int number)) {
                            m_existingIds.Add(number);
                        }
                    }
                }
                else {
                    T value2 = new();
                    value2.LoadString((string)item.Value);
                    m_itemsData[id] = value2;
                }
            }
            //兼容原版SubsystemEditableItemBehavior
            ValuesDictionary blocks = valuesDictionary.GetValue<ValuesDictionary>("Blocks", null);
            if (blocks != null) {
                Terrain terrain = m_subsystemTerrain.Terrain;
                foreach (KeyValuePair<string, object> block in blocks) {
                    Point3 point = HumanReadableConverter.ConvertFromString<Point3>(block.Key);
                    int chunkX = point.X >> 4;
                    int chunkZ = point.Z >> 4;
                    TerrainChunk chunkAtCell = terrain.GetChunkAtCoords(chunkX, chunkZ);
                    if (chunkAtCell == null) {
                        chunkAtCell = terrain.AllocateChunk(chunkX, chunkZ);
                        GVStaticStorage.EditableItemBehaviorChangedChunks.Add(chunkAtCell);
                        while (chunkAtCell.ThreadState <= TerrainChunkState.NotLoaded) {
                            m_subsystemTerrain.TerrainUpdater.UpdateChunkSingleStep(chunkAtCell, 0);
                        }
                    }
                    int value = terrain.GetCellValue(point.X, point.Y, point.Z);
                    if (HandledBlocks.Contains(Terrain.ExtractContents(value))) {
                        T data = new();
                        data.LoadString((string)block.Value);
                        int id = StoreItemDataAtUniqueId(data);
                        m_subsystemTerrain.ChangeCell(point.X, point.Y, point.Z, SetIdToValue(value, id));
                        m_existingIds.Add(id);
                        m_itemsData[id] = data;
                    }
                }
            }
            m_subsystemItemsScanner.ItemsScanned += GarbageCollectItems;
        }

        public override void Save(ValuesDictionary valuesDictionary) {
            base.Save(valuesDictionary);
            ValuesDictionary valuesDictionary3 = new();
            valuesDictionary.SetValue("Items", valuesDictionary3);
            foreach (KeyValuePair<int, T> itemsData in m_itemsData) {
                valuesDictionary3.SetValue(HumanReadableConverter.ConvertToString(itemsData.Key), itemsData.Value.SaveString());
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