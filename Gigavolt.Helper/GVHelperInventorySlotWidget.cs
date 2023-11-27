using System.Xml.Linq;

namespace Game {
    public class GVHelperInventorySlotWidget : CanvasWidget, IDragTargetWidget {
        public readonly ComponentBlockHighlight m_componentBlockHighlight;
        public readonly ClickableWidget m_clickableWidget;

        public GVHelperInventorySlotWidget(ComponentBlockHighlight componentBlockHighlight) {
            XElement node = ContentManager.Get<XElement>("Widgets/GVHelperInventorySlotWidget");
            LoadContents(this, node);
            m_clickableWidget = Children.Find<ClickableWidget>("GVHelperInventorySlotWidget.Clickable");
            m_componentBlockHighlight = componentBlockHighlight;
        }

        public void DragOver(Widget dragWidget, object data) { }

        public void DragDrop(Widget dragWidget, object data) {
            if (data is InventoryDragData inventoryDragData) {
                StaticGVHelper.GotoBlockDescriptionScreen(inventoryDragData.Inventory.GetSlotValue(inventoryDragData.SlotIndex));
            }
        }

        public override void Update() {
            if (m_clickableWidget.IsClicked
                && m_componentBlockHighlight.m_highlightRaycastResult is TerrainRaycastResult result) {
                StaticGVHelper.GotoBlockDescriptionScreen(result.Value);
            }
        }
    }
}