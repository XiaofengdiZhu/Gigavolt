using System.Collections.Generic;
using Engine;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVDebugBlockBehavior : SubsystemEditableItemBehavior<GVDebugData> {
        public SubsystemGVElectricity m_subsystemGVElectricity;
        public readonly HashSet<DebugGVElectricElement> m_elementHashSet = [];
        public GVDebugData m_data;
        public SubsystemGVDebugBlockBehavior() : base(GVBlocksManager.GetBlockIndex<GVDebugBlock>()) { }

        public override void Load(ValuesDictionary valuesDictionary) {
            base.Load(valuesDictionary);
            m_subsystemGVElectricity = Project.FindSubsystem<SubsystemGVElectricity>(true);
            m_data = GetBlockData(new Point3(-842)) ?? new GVDebugData();
            if (m_data != null) {
                SetSpeed(m_data.Speed, true);
                SetDisplayStepFloatingButtons(m_data.DisplayStepFloatingButtons, true);
                SetKeyboardDebug(m_data.KeyboardControl, true);
            }
        }

        public override void Save(ValuesDictionary valuesDictionary) {
            SetBlockData(new Point3(-842), m_data ?? new GVDebugData());
            base.Save(valuesDictionary);
        }

        public override int[] HandledBlocks => [GVBlocksManager.GetBlockIndex<GVDebugBlock>()];

        public void SetDisplayStepFloatingButtons(bool enable, bool force = false) {
            if (force || m_data.DisplayStepFloatingButtons != enable) {
                m_data.DisplayStepFloatingButtons = enable;
                if (enable) {
                    if (m_subsystemGVElectricity.m_debugButtonsDictionary.Count == 0) {
                        foreach (ComponentPlayer componentPlayer in m_subsystemGVElectricity.Project.FindSubsystem<SubsystemPlayers>(true).ComponentPlayers) {
                            GVStepFloatingButtons buttons = new(m_subsystemGVElectricity);
                            m_subsystemGVElectricity.m_debugButtonsDictionary.Add(componentPlayer, buttons);
                            componentPlayer.GameWidget.GuiWidget.AddChildren(buttons);
                        }
                    }
                    else {
                        foreach (GVStepFloatingButtons buttons in m_subsystemGVElectricity.m_debugButtonsDictionary.Values) {
                            buttons.IsVisible = true;
                        }
                    }
                }
                else {
                    if (m_subsystemGVElectricity.m_debugButtonsDictionary.Count > 0) {
                        foreach (GVStepFloatingButtons buttons in m_subsystemGVElectricity.m_debugButtonsDictionary.Values) {
                            buttons.IsVisible = false;
                        }
                    }
                }
            }
        }

        public void SetKeyboardDebug(bool enable, bool force = false) {
            if (force || m_data.KeyboardControl != enable) {
                m_data.KeyboardControl = enable;
                m_subsystemGVElectricity.keyboardDebug = enable;
            }
        }

        public void SetSpeed(float speed, bool force = false) {
            if (force || m_data.Speed != speed) {
                m_data.Speed = speed;
                m_subsystemGVElectricity.SetSpeed(speed);
            }
        }

        public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer) {
            if (componentPlayer.DragHostWidget.IsDragInProgress) {
                return false;
            }
            DialogsManager.ShowDialog(
                componentPlayer.GuiWidget,
                new EditGVDebugDialog(
                    this,
                    m_subsystemGVElectricity,
                    delegate {
                        foreach (DebugGVElectricElement element in m_elementHashSet) {
                            m_subsystemGVElectricity.QueueGVElectricElementForSimulation(element, m_subsystemGVElectricity.CircuitStep + 1);
                        }
                    }
                )
            );
            return true;
        }

        public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer) {
            DialogsManager.ShowDialog(
                componentPlayer.GuiWidget,
                new EditGVDebugDialog(
                    this,
                    m_subsystemGVElectricity,
                    delegate {
                        foreach (DebugGVElectricElement element in m_elementHashSet) {
                            m_subsystemGVElectricity.QueueGVElectricElementForSimulation(element, m_subsystemGVElectricity.CircuitStep + 1);
                        }
                    }
                )
            );
            return true;
        }

        public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner) {
            if (componentMiner.ComponentPlayer != null) {
                DialogsManager.ShowDialog(
                    componentMiner.ComponentPlayer.GuiWidget,
                    new EditGVDebugDialog(
                        this,
                        m_subsystemGVElectricity,
                        delegate {
                            foreach (DebugGVElectricElement element in m_elementHashSet) {
                                m_subsystemGVElectricity.QueueGVElectricElementForSimulation(element, m_subsystemGVElectricity.CircuitStep + 1);
                            }
                        }
                    )
                );
            }
            return true;
        }
    }
}