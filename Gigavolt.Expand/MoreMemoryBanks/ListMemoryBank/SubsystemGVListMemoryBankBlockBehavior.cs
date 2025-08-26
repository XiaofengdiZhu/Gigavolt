﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Engine;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVListMemoryBankBlockBehavior : SubsystemGVEditableItemBehavior<GVListMemoryBankData> {
        public SubsystemGameInfo m_subsystemGameInfo;

        public override void Load(ValuesDictionary valuesDictionary) {
            base.Load(valuesDictionary);
            m_subsystemGameInfo = Project.FindSubsystem<SubsystemGameInfo>(true);
            if (!Storage.DirectoryExists(m_subsystemGameInfo.DirectoryName + "/GVLMB")) {
                Storage.CreateDirectory(m_subsystemGameInfo.DirectoryName + "/GVLMB");
            }
        }

        public override int[] HandledBlocks => [GVBlocksManager.GetBlockIndex<GVListMemoryBankBlock>()];

        public SubsystemGVListMemoryBankBlockBehavior() : base(GVBlocksManager.GetBlockIndex<GVListMemoryBankBlock>()) { }

        public override int GetIdFromValue(int value) => (Terrain.ExtractData(value) >> 5) & 8191;

        public override int SetIdToValue(int value, int id) =>
            Terrain.ReplaceData(value, (Terrain.ExtractData(value) & -262113) | ((id & 8191) << 5));

        public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer) {
            bool isDragInProgress = componentPlayer.DragHostWidget.IsDragInProgress;
            if (isDragInProgress) {
                return false;
            }
            int value = inventory.GetSlotValue(slotIndex);
            int count = inventory.GetSlotCount(slotIndex);
            int id = GetIdFromValue(value);
            GVListMemoryBankData memoryBankData = GetItemData(id, true);
            if (memoryBankData.m_worldDirectory == null) {
                memoryBankData.m_worldDirectory = m_subsystemGameInfo.DirectoryName;
                memoryBankData.LoadData();
            }
            DialogsManager.ShowDialog(
                componentPlayer.GuiWidget,
                new EditGVListMemoryBankDialog(
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
            GVListMemoryBankData memoryBankData = GetItemData(id, true);
            if (memoryBankData.m_worldDirectory == null) {
                memoryBankData.m_worldDirectory = m_subsystemGameInfo.DirectoryName;
                memoryBankData.LoadData();
            }
            DialogsManager.ShowDialog(
                componentPlayer.GuiWidget,
                new EditGVListMemoryBankDialog(
                    memoryBankData,
                    () => { SubsystemTerrain.ChangeCell(x, y, z, SetIdToValue(value, StoreItemDataAtUniqueId(memoryBankData, id))); }
                )
            );
            return true;
        }

        public override void Dispose() {
            try {
                IEnumerable<uint> worldIDList = m_itemsData.Values.Select(d => d.ID);
                List<string> fileList = Storage.ListFileNames($"{m_subsystemGameInfo.DirectoryName}/GVLMB/").ToList();
                uint[] fileNumberList = fileList.Select(fileName => {
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
                    Storage.DeleteFile($"{m_subsystemGameInfo.DirectoryName}/GVLMB/{fileName}");
                }
            }
            catch (Exception ex) {
                Log.Error(ex);
            }
            base.Dispose();
        }
    }
}