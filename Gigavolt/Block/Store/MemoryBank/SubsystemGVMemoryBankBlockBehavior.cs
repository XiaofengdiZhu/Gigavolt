using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Engine;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVMemoryBankBlockBehavior : SubsystemEditableItemBehavior<GVMemoryBankData> {
        public SubsystemGameInfo m_subsystemGameInfo;

        public override void Load(ValuesDictionary valuesDictionary) {
            base.Load(valuesDictionary);
            m_subsystemGameInfo = Project.FindSubsystem<SubsystemGameInfo>(true);
            if (!Storage.DirectoryExists(m_subsystemGameInfo.DirectoryName + "/GVMB")) {
                Storage.CreateDirectory(m_subsystemGameInfo.DirectoryName + "/GVMB");
            }
        }

        public override int[] HandledBlocks => new[] { GVMemoryBankBlock.Index };

        public SubsystemGVMemoryBankBlockBehavior() : base(GVMemoryBankBlock.Index) { }

        public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer) {
            bool isDragInProgress = componentPlayer.DragHostWidget.IsDragInProgress;
            if (isDragInProgress) {
                return false;
            }
            int value = inventory.GetSlotValue(slotIndex);
            int count = inventory.GetSlotCount(slotIndex);
            int id = Terrain.ExtractData(value);
            GVMemoryBankData memoryBankData = GetItemData(id);
            memoryBankData = memoryBankData ?? new GVMemoryBankData(GVStaticStorage.GetUniqueGVMBID(), m_subsystemGameInfo.DirectoryName);
            if (memoryBankData.m_worldDirectory == null) {
                memoryBankData.m_worldDirectory = m_subsystemGameInfo.DirectoryName;
                memoryBankData.LoadData();
            }
            DialogsManager.ShowDialog(
                componentPlayer.GuiWidget,
                new EditGVMemoryBankDialog(
                    memoryBankData,
                    delegate {
                        int data = StoreItemDataAtUniqueId(memoryBankData);
                        int value2 = Terrain.ReplaceData(value, data);
                        inventory.RemoveSlotItems(slotIndex, count);
                        inventory.AddSlotItems(slotIndex, value2, count);
                    }
                )
            );
            return true;
        }

        public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer) {
            GVMemoryBankData memoryBankData = GetBlockData(new Point3(x, y, z)) ?? new GVMemoryBankData(GVStaticStorage.GetUniqueGVMBID(), m_subsystemGameInfo.DirectoryName);
            if (memoryBankData.m_worldDirectory == null) {
                memoryBankData.m_worldDirectory = m_subsystemGameInfo.DirectoryName;
                memoryBankData.LoadData();
                SetBlockData(new Point3(x, y, z), memoryBankData);
            }
            DialogsManager.ShowDialog(
                componentPlayer.GuiWidget,
                new EditGVMemoryBankDialog(
                    memoryBankData,
                    delegate {
                        SetBlockData(new Point3(x, y, z), memoryBankData);
                        int face = ((GVMemoryBankBlock)BlocksManager.Blocks[GVMemoryBankBlock.Index]).GetFace(value);
                        SubsystemGVElectricity subsystemGVElectricity = SubsystemTerrain.Project.FindSubsystem<SubsystemGVElectricity>(true);
                        GVElectricElement GVElectricElement = subsystemGVElectricity.GetGVElectricElement(
                            x,
                            y,
                            z,
                            face,
                            0
                        );
                        if (GVElectricElement != null) {
                            subsystemGVElectricity.QueueGVElectricElementForSimulation(GVElectricElement, subsystemGVElectricity.CircuitStep + 1);
                        }
                    }
                )
            );
            return true;
        }

        public override void Dispose() {
            try {
                IEnumerable<uint> worldIDList = m_itemsData.Values.Select(d => d.m_ID).Concat(m_blocksData.Values.Select(d => d.m_ID));
                //Log.Information(string.Join(",", worldIDList.Select(n => n.ToString("X"))));
                List<string> fileList = Storage.ListFileNames($"{m_subsystemGameInfo.DirectoryName}/GVMB/").ToList();
                //Log.Information(string.Join(",", fileList));
                uint[] fileNumberList = fileList.Select(
                        fileName => {
                            int index = fileName.LastIndexOf('.');
                            if (index >= 0) {
                                fileName = fileName.Substring(0, index);
                            }
                            if (uint.TryParse(fileName, NumberStyles.HexNumber, null, out uint number)) {
                                return number;
                            }
                            return 0u;
                        }
                    )
                    .ToArray();
                IEnumerable<uint> deleteList = fileNumberList.Except(worldIDList);
                //Log.Information(string.Join(",", deleteList.Select(n => n.ToString("X"))));
                foreach (uint id in deleteList) {
                    if (id == 0) {
                        continue;
                    }
                    string fileName = fileList[Array.IndexOf(fileNumberList, id)];
                    Storage.DeleteFile($"{m_subsystemGameInfo.DirectoryName}/GVMB/{fileName}");
                }
            }
            catch (Exception ex) {
                Log.Error(ex);
            }
            foreach (GVArrayData data in GVStaticStorage.GVMBIDDataDictionary.Values) {
                data.ClearCache();
            }
            GVStaticStorage.GVMBIDDataDictionary.Clear();
            base.Dispose();
        }
    }
}