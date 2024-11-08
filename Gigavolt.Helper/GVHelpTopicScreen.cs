using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Engine;
using Engine.Graphics;
using Engine.Media;

namespace Game {
    public class GVHelpTopicScreen : RecipaediaDescriptionScreen {
        public class I18NHelp {
            public string WebUrl;
            public string WebUrlSuffixZh;
            public string WebUrlSuffixEn;
            public string ImageName;
        }

        //Windows 175%缩放时，Chrome浏览器，html宽度为1191.43时，使用 https://chromewebstore.google.com/detail/screenshot/pjeaanicajghlcpekmmopjffbllcniee 对.main进行截图，可得到1440宽度的结果
        public static Dictionary<Type, I18NHelp> m_type2I18NHelp = new() {
            { typeof(GVDigitalToAnalogConverterBlock), new I18NHelp { WebUrl = "base/shift/converter.html", WebUrlSuffixZh = "#数模转换器", WebUrlSuffixEn = "#digital-to-analog-converter", ImageName = "digital-to-analog-converter" } },
            { typeof(GVAnalogToDigitalConverterBlock), new I18NHelp { WebUrl = "base/shift/converter.html", WebUrlSuffixZh = "#模数转换器", WebUrlSuffixEn = "#analog-to-digital-converter", ImageName = "analog-to-digital-converter" } },
            { typeof(GVMemoryBankBlock), new I18NHelp { WebUrl = "base/shift/memory_bank.html", ImageName = "memory_bank" } },
            { typeof(GVTruthTableCircuitBlock), new I18NHelp { WebUrl = "base/shift/truth_table.html", ImageName = "truth_table" } },
            { typeof(GVRealTimeClockBlock), new I18NHelp { WebUrl = "base/shift/real_time_clock.html", ImageName = "real_time_clock" } },
            { typeof(GVSoundGeneratorBlock), new I18NHelp { WebUrl = "base/shift/sound_generator.html", ImageName = "sound_generator" } },
            { typeof(GVPistonBlock), new I18NHelp { WebUrl = "base/shift/complex_piston.html", ImageName = "complex_piston" } },
            { typeof(GVSignBlock), new I18NHelp { WebUrl = "base/shift/sign.html", ImageName = "sign" } },
            { typeof(GVDispenserBlock), new I18NHelp { WebUrl = "base/shift/dispenser.html", ImageName = "dispenser" } },
            { typeof(GVDebugBlock), new I18NHelp { WebUrl = "base/new/debug.html", ImageName = "debug" } },
            { typeof(GVCopperHammerBlock), new I18NHelp { WebUrl = "expand/wires/copper_hammer.html", ImageName = "copper_hammer" } },
            { typeof(GVEWireThroughBlock), new I18NHelp { WebUrl = "expand/wires/copper_hammer.html", ImageName = "copper_hammer" } },
            { typeof(GVJumpWireBlock), new I18NHelp { WebUrl = "expand/wires/jumper.html", ImageName = "jumper" } },
            { typeof(GVMultiplexerBlock), new I18NHelp { WebUrl = "expand/wires/multiplexer.html", ImageName = "multiplexer" } },
            { typeof(GVMoreTwoInTwoOutBlock), new I18NHelp { WebUrl = "expand/gates/more_two_in_two_out.html", ImageName = "more_two_in_two_out" } },
            { typeof(GVMoreOneInOneOutBlock), new I18NHelp { WebUrl = "expand/gates/more_one_in_one_out.html", ImageName = "more_one_in_one_out" } },
            { typeof(GVJavascriptMicrocontrollerBlock), new I18NHelp { WebUrl = "expand/gates/javascript_microcontroller.html", ImageName = "javascript_microcontroller" } },
            { typeof(GVVolatileMemoryBankBlock), new I18NHelp { WebUrl = "base/shift/memory_bank.html", ImageName = "memory_bank" } },
            { typeof(GVListMemoryBankBlock), new I18NHelp { WebUrl = "expand/memory_banks/list_memory_bank.html", ImageName = "list_memory_bank" } },
            { typeof(GVVolatileListMemoryBankBlock), new I18NHelp { WebUrl = "expand/memory_banks/list_memory_bank.html", ImageName = "list_memory_bank" } },
            { typeof(GVFourDimensionalMemoryBankBlock), new I18NHelp { WebUrl = "expand/memory_banks/four_dimensional_memory_bank.html", ImageName = "four_dimensional_memory_bank" } },
            { typeof(GVVolatileFourDimensionalMemoryBankBlock), new I18NHelp { WebUrl = "expand/memory_banks/four_dimensional_memory_bank.html", ImageName = "four_dimensional_memory_bank" } },
            { typeof(GVMemoryBanksOperatorBlock), new I18NHelp { WebUrl = "expand/memory_banks/memory_banks_operator.html", ImageName = "memory_banks_operator" } },
            { typeof(GVOscilloscopeBlock), new I18NHelp { WebUrl = "expand/displays/oscilloscope.html", ImageName = "oscilloscope" } },
            { typeof(GVNesEmulatorBlock), new I18NHelp { WebUrl = "expand/displays/nes_emulator.html", ImageName = "nes_emulator" } },
            { typeof(GVTerrainRaycastDetectorBlock), new I18NHelp { WebUrl = "expand/sensors/terrain_raycast_detector.html", ImageName = "terrain_raycast_detector" } },
            { typeof(GVTerrainScannerBlock), new I18NHelp { WebUrl = "expand/sensors/terrain_scanner.html", ImageName = "terrain_scanner" } },
            { typeof(GVPlayerMonitorBlock), new I18NHelp { WebUrl = "expand/sensors/player_monitor.html", ImageName = "player_monitor" } },
            { typeof(GVCameraBlock), new I18NHelp { WebUrl = "expand/sensors/camera.html", ImageName = "camera" } },
            { typeof(GVGuidedDispenserBlock), new I18NHelp { WebUrl = "expand/transportation/guided_dispenser.html", ImageName = "guided_dispenser" } },
            { typeof(GVAttractorBlock), new I18NHelp { WebUrl = "expand/transportation/attractor.html", ImageName = "attractor" } },
            { typeof(GVInventoryFetcherBlock), new I18NHelp { WebUrl = "expand/transportation/inventory_fetcher.html", ImageName = "inventory_fetcher" } },
            { typeof(GVInventoryControllerBlock), new I18NHelp { WebUrl = "expand/transportation/inventory_controller.html", ImageName = "inventory_controller" } },
            { typeof(GVTractorBeamBlock), new I18NHelp { WebUrl = "expand/transportation/tractor_beam.html", ImageName = "tractor_beam" } },
            { typeof(GVSignalGeneratorBlock), new I18NHelp { WebUrl = "expand/others/signal_generator.html", ImageName = "signal_generator" } },
            { typeof(GVPlayerControllerBlock), new I18NHelp { WebUrl = "expand/others/player_controller.html", ImageName = "player_controller" } }
        };

        public static I18NHelp DisplayLedData2I18NHelp(int data) {
            switch (GVDisplayLedBlock.GetType(data)) {
                case 2 or 3: return new I18NHelp { WebUrl = "expand/displays/image_display_led.html", ImageName = "image_display_led" };
                case 4 or 5: return new I18NHelp { WebUrl = "expand/displays/terrain_display_led.html", ImageName = "terrain_display_led" };
                default: return new I18NHelp { WebUrl = "base/shift/block_display_led.html", ImageName = "block_display_led" };
            }
        }

        public readonly BevelledButtonWidget m_urlButton;
        public readonly LabelWidget m_urlLabel;
        public readonly RectangleWidget m_imageWidget;
        public readonly ScrollPanelWidget m_scrollPanel;
        public readonly BevelledButtonWidget m_copyButton;
        public readonly ButtonWidget m_backButton;
        public I18NHelp m_lastI18NHelp;
        public Subtexture m_lastSubtexture;

        public GVHelpTopicScreen() {
            ClearChildren();
            LoadContents(this, ContentManager.Get<XElement>("Screens/GVHelpTopicScreen"));
            m_urlButton = Children.Find<BevelledButtonWidget>("GVHelpTopicScreen.UrlButton");
            m_urlLabel = Children.Find<LabelWidget>("GVHelpTopicScreen.UrlLabel");
            m_imageWidget = Children.Find<RectangleWidget>("GVHelpTopicScreen.Image");
            m_scrollPanel = Children.Find<ScrollPanelWidget>("ScrollPanel");
            m_copyButton = Children.Find<BevelledButtonWidget>("GVHelpTopicScreen.Copy");
            m_backButton = Children.Find<ButtonWidget>("TopBar.Back");
        }

        public override void Enter(object[] parameters) {
            try {
                int blockValue = (int)parameters[0];
                Type blockType = BlocksManager.Blocks[Terrain.ExtractContents(blockValue)].GetType();
                I18NHelp i18nHelp = null;
                if (blockType == typeof(GVDisplayLedBlock)) {
                    i18nHelp = DisplayLedData2I18NHelp(Terrain.ExtractData(blockValue));
                }
                if ((i18nHelp != null || m_type2I18NHelp.TryGetValue(blockType, out i18nHelp))
                    && i18nHelp != null
                    && i18nHelp != m_lastI18NHelp) {
                    bool isZh = ModsManager.Configs["Language"].StartsWith("zh");
                    m_lastSubtexture?.Texture.Dispose();
                    m_urlLabel.Text = $"https://xiaofengdizhu.github.io/GigavoltDoc/{(isZh ? "zh" : "en")}/{i18nHelp.WebUrl}{(isZh ? i18nHelp.WebUrlSuffixZh : i18nHelp.WebUrlSuffixEn) ?? string.Empty}";
                    Image image = ContentManager.Get<Image>($"GVHelperImages/{(isZh ? "zh" : "en")}/{i18nHelp.ImageName}");
                    m_lastSubtexture = new Subtexture(Texture2D.Load(image), Vector2.Zero, Vector2.One);
                    m_imageWidget.Subtexture = m_lastSubtexture;
                    float width = RootWidget.ActualSize.X - 64;
                    m_imageWidget.Size = new Vector2(width, width / image.Width * image.Height);
                    m_lastI18NHelp = i18nHelp;
                }
                m_scrollPanel.ScrollPosition = 0f;
            }
            catch (Exception e) {
                Log.Error(e);
            }
        }

        public override void Update() {
            if (m_copyButton.IsClicked) {
                ClipboardManager.ClipboardString = m_urlLabel.Text;
            }
            if (m_urlButton.IsClicked) {
                WebBrowserManager.LaunchBrowser(m_urlLabel.Text);
            }
            if (Input.Back
                || Input.Cancel
                || m_backButton.IsClicked
                || Children.Find<ButtonWidget>("TopBar.Back").IsClicked) {
                ScreensManager.SwitchScreen(ScreensManager.PreviousScreen);
            }
        }
    }
}