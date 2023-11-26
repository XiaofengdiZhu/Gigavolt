using Engine.Input;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game {
    public class ComponentGVHelper : Component, IUpdateable {
        public WidgetInput m_input;
        public ComponentPlayer m_componentPlayer;
        public ComponentBlockHighlight m_componentBlockHighlight;
        public GVHelperInventorySlotWidget m_inventorySlotWidget;
        public UpdateOrder UpdateOrder => UpdateOrder.Default;

        public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap) {
            m_inventorySlotWidget = new GVHelperInventorySlotWidget(Entity.FindComponent<ComponentBlockHighlight>(true));
            Entity.FindComponent<ComponentGui>(false)?.ShortInventoryWidget.AddChildren(m_inventorySlotWidget);
            m_componentPlayer = Entity.FindComponent<ComponentPlayer>(true);
            m_componentBlockHighlight = Entity.FindComponent<ComponentBlockHighlight>(true);
        }

        public void Update(float dt) {
            if (!m_componentPlayer.ComponentAimingSights.IsSightsVisible
                && Keyboard.IsKeyDownOnce(Key.F6)
                && m_componentBlockHighlight.m_highlightRaycastResult is TerrainRaycastResult result) {
                m_inventorySlotWidget.GotoBlockDescriptionScreen(result.Value);
            }
        }
    }
}