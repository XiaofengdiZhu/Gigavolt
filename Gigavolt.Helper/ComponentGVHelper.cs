using Engine.Input;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game {
    public class ComponentGVHelper : Component, IUpdateable {
        public WidgetInput m_input;
        public ComponentPlayer m_componentPlayer;
        public ComponentGui m_componentGui;
        public ComponentBlockHighlight m_componentBlockHighlight;
        public GVHelperInventorySlotWidget m_GVHelperInventorySlotWidget;
        public bool m_slotAdded;
        public UpdateOrder UpdateOrder => UpdateOrder.Default;

        public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap) {
            GVStaticStorage.GVHelperAvailable = true;
            m_componentPlayer = Entity.FindComponent<ComponentPlayer>(true);
            m_componentGui = Entity.FindComponent<ComponentGui>(true);
            m_componentBlockHighlight = Entity.FindComponent<ComponentBlockHighlight>(true);
            m_GVHelperInventorySlotWidget = new GVHelperInventorySlotWidget(m_componentBlockHighlight);
            if (GVStaticStorage.GVHelperSlotActive) {
                m_componentGui.ShortInventoryWidget.AddChildren(m_GVHelperInventorySlotWidget);
                m_slotAdded = true;
            }
        }

        public void Update(float dt) {
            if (GVStaticStorage.GVHelperSlotActive) {
                if (!m_slotAdded) {
                    m_componentGui.ShortInventoryWidget.AddChildren(m_GVHelperInventorySlotWidget);
                    m_slotAdded = true;
                }
            }
            else {
                if (m_slotAdded) {
                    m_componentGui.ShortInventoryWidget.RemoveChildren(m_GVHelperInventorySlotWidget);
                    m_slotAdded = false;
                }
            }
            if (!m_componentPlayer.ComponentAimingSights.IsSightsVisible
                && Keyboard.IsKeyDownOnce(Key.F6)
                && m_componentBlockHighlight.m_highlightRaycastResult is TerrainRaycastResult result) {
                StaticGVHelper.GotoBlockDescriptionScreen(result.Value);
            }
        }
    }
}