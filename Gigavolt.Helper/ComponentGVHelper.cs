using Engine.Input;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game {
    public class ComponentGVHelper : Component, IUpdateable {
        public ComponentPlayer m_componentPlayer;
        public ComponentGui m_componentGui;
        public ComponentBlockHighlight m_componentBlockHighlight;

        public GVHelperInventorySlotWidget m_GVHelperInventorySlotWidget;
        public StackPanelWidget m_shortInventoryPanel;

        public bool m_slotAdded;
        public UpdateOrder UpdateOrder => UpdateOrder.Default;

        public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap) {
            GVStaticStorage.GVHelperAvailable = true;
            m_componentPlayer = Entity.FindComponent<ComponentPlayer>(true);
            m_componentGui = Entity.FindComponent<ComponentGui>(true);
            m_componentBlockHighlight = Entity.FindComponent<ComponentBlockHighlight>(true);
            m_GVHelperInventorySlotWidget = new GVHelperInventorySlotWidget(m_componentBlockHighlight);
            m_shortInventoryPanel = new StackPanelWidget { Direction = LayoutDirection.Horizontal };
            m_componentGui.ShortInventoryWidget.AddChildren(m_shortInventoryPanel);
            GridPanelWidget temp = m_componentGui.ShortInventoryWidget.Children.Find<GridPanelWidget>("InventoryGrid");
            m_componentGui.ShortInventoryWidget.RemoveChildren(temp);
            m_shortInventoryPanel.AddChildren(temp);
            temp.ChangeParent(m_shortInventoryPanel);
            if (GVStaticStorage.GVHelperSlotActive) {
                m_shortInventoryPanel.AddChildren(m_GVHelperInventorySlotWidget);
                m_slotAdded = true;
            }
        }

        public void Update(float dt) {
            if (GVStaticStorage.GVHelperSlotActive) {
                if (!m_slotAdded) {
                    m_shortInventoryPanel.AddChildren(m_GVHelperInventorySlotWidget);
                    m_slotAdded = true;
                }
            }
            else {
                if (m_slotAdded) {
                    m_shortInventoryPanel.RemoveChildren(m_GVHelperInventorySlotWidget);
                    m_slotAdded = false;
                }
            }
            if (!m_componentPlayer.ComponentAimingSights.IsSightsVisible
                && m_componentPlayer.GameWidget.Input.IsKeyDownOnce(Key.F8)
                && m_componentBlockHighlight.m_highlightRaycastResult is TerrainRaycastResult result) {
                StaticGVHelper.GotoBlockDescriptionScreen(result.Value);
            }
        }
    }
}