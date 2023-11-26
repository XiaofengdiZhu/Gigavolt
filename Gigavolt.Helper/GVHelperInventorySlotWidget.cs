using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Game {
    public class GVHelperInventorySlotWidget : CanvasWidget, IDragTargetWidget {
        public readonly ComponentBlockHighlight m_componentBlockHighlight;
        public readonly ClickableWidget m_clickableWidget;

        public readonly Dictionary<int, string[]> m_dictionary = new() {
            { GVMemoryBankBlock.Index, new[] { "存储器-memory-bank", "GVMemoryBankBlock" } },
            { GVTruthTableCircuitBlock.Index, new[] { "真值表-truth-table", "GVTruthTableCircuitBlock" } },
            { GVRealTimeClockBlock.Index, new[] { "实时钟-real-time-clock", "GVRealTimeClockBlock" } },
            { GVSoundGeneratorBlock.Index, new[] { "声音生成器-sound-generator", "GVSoundGeneratorBlock" } },
            { GVDispenserBlock.Index, new[] { "发射器-dispenser", "GVDispenserBlock" } },
            { GVSignBlock.Index, new[] { "告示牌-sign", "GVSignBlock" } },
            { GVMultiplexerBlock.Index, new[] { "路选器-multiplexer", "GVMultiplexerBlock" } },
            { GVListMemoryBankBlock.Index, new[] { "一维存储器-list-memory-bank", "GVListMemoryBankBlock" } },
            { GVMemoryBanksOperatorBlock.Index, new[] { "多存储器操作器-memory-banks-operator", "GVMemoryBanksOperatorBlock" } },
            { GVTerrainRaycastDetectorBlock.Index, new[] { "地形射线探测器-terrain-raycast-detector", "GVTerrainRaycastDetectorBlock" } },
            { GVTerrainScannerBlock.Index, new[] { "地形扫描仪-terrain-scanner", "GVTerrainScannerBlock" } },
            { GVNesEmulatorBlock.Index, new[] { "红白机模拟器-nes-emulator", "GVNesEmulatorBlock" } },
            { GVGuidedDispenserBlock.Index, new[] { "制导发射器-guided-dispenser", "GVGuidedDispenserBlock" } },
            { GVInventoryControllerBlock.Index, new[] { "箱子控制器-inventory-controller", "GVInventoryControllerBlock" } },
            { GVJavascriptMicrocontrollerBlock.Index, new[] { "js单片机-javascript-microcontroller", "GVJavascriptMicrocontrollerBlock" } }
        };

        public GVHelperInventorySlotWidget(ComponentBlockHighlight componentBlockHighlight) {
            XElement node = ContentManager.Get<XElement>("Widgets/GVHelperInventorySlotWidget");
            LoadContents(this, node);
            m_clickableWidget = Children.Find<ClickableWidget>("GVHelperInventorySlotWidget.Clickable");
            m_componentBlockHighlight = componentBlockHighlight;
        }

        public void DragOver(Widget dragWidget, object data) { }

        public void DragDrop(Widget dragWidget, object data) {
            if (data is InventoryDragData inventoryDragData) {
                GotoBlockDescriptionScreen(inventoryDragData.Inventory.GetSlotValue(inventoryDragData.SlotIndex));
            }
        }

        public override void Update() {
            if (m_clickableWidget.IsClicked
                && m_componentBlockHighlight.m_highlightRaycastResult is TerrainRaycastResult result) {
                GotoBlockDescriptionScreen(result.Value);
            }
        }

        public void GotoBlockDescriptionScreen(int blockValue) {
            int blockContent = Terrain.ExtractContents(blockValue);
            Block block = BlocksManager.Blocks[blockContent];
            int id = blockContent;
            if (block.GetCreativeValues().Contains(blockValue)) {
                id = blockValue;
            }
            if (m_dictionary.TryGetValue(blockContent, out string[] value)) {
                GotoGVHelpScreen(value[0], value[1]);
            }
            else {
                ScreensManager.SwitchScreen("RecipaediaDescription", id, new List<int> { id });
            }
        }

        public static void GotoGVHelpScreen(string url, string blockClassName) {
            if (!ScreensManager.m_screens.ContainsKey("GVHelpTopicScreen")) {
                ScreensManager.AddScreen("GVHelpTopicScreen", new GVHelpTopicScreen());
            }
            ScreensManager.SwitchScreen("GVHelpTopicScreen", url, blockClassName);
        }
    }
}