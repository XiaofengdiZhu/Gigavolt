using Engine;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVDebugBlockBehavior : SubsystemEditableItemBehavior<GVDebugData> {
        public SubsystemGVElectricity m_subsystemGVElectricity;
        public SubsystemGVDebugBlockBehavior() : base(GVDebugBlock.Index) { }

        public override void Load(ValuesDictionary valuesDictionary) {
            base.Load(valuesDictionary);
            m_subsystemGVElectricity = Project.FindSubsystem<SubsystemGVElectricity>(true);
            GVDebugData data = GetBlockData(new Point3(-GVDebugBlock.Index));
            if (data != null
                && data.Data.Length > 0
                && float.TryParse(data.Data, out float speed)) {
                m_subsystemGVElectricity.SetSpeed(speed);
            }
        }

        public override int[] HandledBlocks => new[] { GVDebugBlock.Index };

        public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer) {
            if (componentPlayer.DragHostWidget.IsDragInProgress) {
                return false;
            }
            GVDebugData Data = GetBlockData(new Point3(-GVDebugBlock.Index)) ?? new GVDebugData();
            DialogsManager.ShowDialog(componentPlayer.GuiWidget, new EditGVDebugDialog(Data, m_subsystemGVElectricity, delegate { SetBlockData(new Point3(-GVDebugBlock.Index), Data); }));
            return true;
        }

        public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer) {
            GVDebugData Data = GetBlockData(new Point3(-GVDebugBlock.Index)) ?? new GVDebugData();
            DialogsManager.ShowDialog(componentPlayer.GuiWidget, new EditGVDebugDialog(Data, m_subsystemGVElectricity, delegate { SetBlockData(new Point3(-GVDebugBlock.Index), Data); }));
            return true;
        }
    }
}