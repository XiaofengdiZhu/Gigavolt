using System;
using Engine;
using TemplatesDatabase;

namespace Game
{
    public class SubsystemGVMemoryBankBlockBehavior : SubsystemEditableItemBehavior<GVMemoryBankData>
    {
        public SubsystemGameInfo m_subsystemGameInfo;
        public static string fName = "GVMemoryBankBlockBehavior";
        public override void Load(ValuesDictionary valuesDictionary)
        {
            base.Load(valuesDictionary);
            this.m_subsystemGameInfo = base.Project.FindSubsystem<SubsystemGameInfo>(true);
            if (Storage.DirectoryExists(this.m_subsystemGameInfo.DirectoryName + "/GVMB"))
            {
                Storage.CreateDirectory(this.m_subsystemGameInfo.DirectoryName + "/GVMB");
            }
        }
        public override int[] HandledBlocks
        {
            get
            {
                return new int[]
                {
                    886
                };
            }
        }
        public SubsystemGVMemoryBankBlockBehavior() : base(886)
        {
        }
        public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer)
        {
            try
            {
                bool isDragInProgress = componentPlayer.DragHostWidget.IsDragInProgress;
                if (isDragInProgress)
                {
                    return false;
                }
                int value = inventory.GetSlotValue(slotIndex);
                int count = inventory.GetSlotCount(slotIndex);
                int id = Terrain.ExtractData(value);
                GVMemoryBankData memoryBankData = base.GetItemData(id);
                memoryBankData = ((memoryBankData != null) ? ((GVMemoryBankData)memoryBankData.Copy()) : new GVMemoryBankData(Guid.NewGuid(), this.m_subsystemGameInfo.DirectoryName, null, 0u));
                if (memoryBankData.m_worldDirectory == null)
                {
                    memoryBankData.m_worldDirectory = this.m_subsystemGameInfo.DirectoryName;
                    memoryBankData.LoadData();
                }
                DialogsManager.ShowDialog(componentPlayer.GuiWidget, new EditGVMemoryBankDialog(memoryBankData, delegate ()
                {
                    int data = this.StoreItemDataAtUniqueId(memoryBankData);
                    int value2 = Terrain.ReplaceData(value, data);
                    inventory.RemoveSlotItems(slotIndex, count);
                    inventory.AddSlotItems(slotIndex, value2, count);
                }));
            }
            catch (Exception ex)
            {
                Log.Error(ex);
            }
            return true;
        }

        public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer)
        {
            GVMemoryBankData memoryBankData = base.GetBlockData(new Point3(x, y, z)) ?? new GVMemoryBankData(Guid.NewGuid(), this.m_subsystemGameInfo.DirectoryName, null, 0u);
            if (memoryBankData.m_worldDirectory == null)
            {
                memoryBankData.m_worldDirectory = this.m_subsystemGameInfo.DirectoryName;
                memoryBankData.LoadData();
                base.SetBlockData(new Point3(x, y, z), memoryBankData);
            }
            DialogsManager.ShowDialog(componentPlayer.GuiWidget, new EditGVMemoryBankDialog(memoryBankData, delegate ()
            {
                this.SetBlockData(new Point3(x, y, z), memoryBankData);
                int face = ((GVMemoryBankBlock)BlocksManager.Blocks[886]).GetFace(value);
                SubsystemGVElectricity subsystemGVElectricity = this.SubsystemTerrain.Project.FindSubsystem<SubsystemGVElectricity>(true);
                GVElectricElement GVElectricElement = subsystemGVElectricity.GetGVElectricElement(x, y, z, face);
                bool flag2 = GVElectricElement != null;
                if (flag2)
                {
                    subsystemGVElectricity.QueueGVElectricElementForSimulation(GVElectricElement, subsystemGVElectricity.CircuitStep + 1);
                }
            }));
            return true;
        }
    }
}