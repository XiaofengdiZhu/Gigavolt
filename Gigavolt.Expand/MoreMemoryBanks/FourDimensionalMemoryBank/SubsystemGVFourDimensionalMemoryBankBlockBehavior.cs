using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Engine;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVFourDimensionalMemoryBankBlockBehavior : SubsystemGVEditableItemBehavior<GVFourDimensionalMemoryBankData> {
        public SubsystemGameInfo m_subsystemGameInfo;

        public override void Load(ValuesDictionary valuesDictionary) {
            base.Load(valuesDictionary);
            m_subsystemGameInfo = Project.FindSubsystem<SubsystemGameInfo>(true);
            if (!Storage.DirectoryExists(m_subsystemGameInfo.DirectoryName + "/GVFDMB")) {
                Storage.CreateDirectory(m_subsystemGameInfo.DirectoryName + "/GVFDMB");
            }
        }

        public override int[] HandledBlocks => [GVFourDimensionalMemoryBankBlock.Index];

        public SubsystemGVFourDimensionalMemoryBankBlockBehavior() : base(GVFourDimensionalMemoryBankBlock.Index) { }
        public override int GetIdFromValue(int value) => (Terrain.ExtractData(value) >> 5) & 8191;
        public override int SetIdToValue(int value, int id) => Terrain.ReplaceData(value, (Terrain.ExtractData(value) & -262113) | ((id & 8191) << 5));

        public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer) {
            bool isDragInProgress = componentPlayer.DragHostWidget.IsDragInProgress;
            if (isDragInProgress) {
                return false;
            }
            int value = inventory.GetSlotValue(slotIndex);
            int count = inventory.GetSlotCount(slotIndex);
            int id = GetIdFromValue(value);
            GVFourDimensionalMemoryBankData memoryBankData = GetItemData(id, true);
            memoryBankData = memoryBankData ?? new GVFourDimensionalMemoryBankData(GVStaticStorage.GetUniqueGVMBID(), m_subsystemGameInfo.DirectoryName);
            if (memoryBankData.m_worldDirectory == null) {
                memoryBankData.m_worldDirectory = m_subsystemGameInfo.DirectoryName;
                memoryBankData.LoadData();
            }
            DialogsManager.ShowDialog(
                componentPlayer.GuiWidget,
                new EditGVFourDimensionalMemoryBankDialog(
                    memoryBankData,
                    () => {
                        inventory.RemoveSlotItems(slotIndex, count);
                        inventory.AddSlotItems(slotIndex, SetIdToValue(value, StoreItemDataAtUniqueId(memoryBankData, id)), count);
                    }
                )
            );
            return true;
        }

        public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer) {
            int id = GetIdFromValue(value);
            GVFourDimensionalMemoryBankData memoryBankData = GetItemData(id, true);
            if (memoryBankData.m_worldDirectory == null) {
                memoryBankData.m_worldDirectory = m_subsystemGameInfo.DirectoryName;
                memoryBankData.LoadData();
            }
            DialogsManager.ShowDialog(componentPlayer.GuiWidget, new EditGVFourDimensionalMemoryBankDialog(memoryBankData, () => { SubsystemTerrain.ChangeCell(x, y, z, SetIdToValue(value, StoreItemDataAtUniqueId(memoryBankData, id))); }));
            return true;
        }

        public override void Dispose() {
            try {
                IEnumerable<uint> worldIDList = m_itemsData.Values.Select(d => d.m_ID);
                List<string> fileList = Storage.ListDirectoryNames($"{m_subsystemGameInfo.DirectoryName}/GVFDMB/").ToList();
                uint[] fileNumberList = fileList.Select(
                        fileName => {
                            int index = fileName.LastIndexOf('.');
                            if (index >= 0) {
                                fileName = fileName.Substring(0, index);
                            }
                            return uint.TryParse(fileName, NumberStyles.HexNumber, null, out uint number) ? number : 0u;
                        }
                    )
                    .ToArray();
                IEnumerable<uint> deleteList = fileNumberList.Except(worldIDList);
                foreach (uint id in deleteList) {
                    if (id == 0) {
                        continue;
                    }
                    string fileName = fileList[Array.IndexOf(fileNumberList, id)];
                    Storage.DeleteDirectory($"{m_subsystemGameInfo.DirectoryName}/GVFDMB/{fileName}", true);
                }
            }
            catch (Exception ex) {
                Log.Error(ex);
            }
            base.Dispose();
        }
    }
}