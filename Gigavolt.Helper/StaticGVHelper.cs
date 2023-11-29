using System;
using System.Collections.Generic;
using System.Linq;

namespace Game {
    public static class StaticGVHelper {
        public static readonly Dictionary<int, string[]> BlockIndex2HelperInfo = new() {
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

        public static readonly Dictionary<int, Func<int, int>> BlockIndex2DataHandler = new() {
            { GVWireBlock.Index, blockData => GVWireBlock.SetColor(0, GVWireBlock.GetColor(blockData)) },
            { GVLedBlock.Index, blockData => GVLedBlock.SetColor(0, GVLedBlock.GetColor(blockData)) },
            { GVFourLedBlock.Index, blockData => GVFourLedBlock.SetColor(0, GVFourLedBlock.GetColor(blockData)) },
            { GVSevenSegmentDisplayBlock.Index, blockData => GVSevenSegmentDisplayBlock.SetColor(0, GVSevenSegmentDisplayBlock.GetColor(blockData)) },
            { GVAnalogToDigitalConverterBlock.Index, blockData => GVAnalogToDigitalConverterBlock.SetType(0, GVAnalogToDigitalConverterBlock.GetType(blockData)) },
            { GVDigitalToAnalogConverterBlock.Index, blockData => GVDigitalToAnalogConverterBlock.SetType(0, GVDigitalToAnalogConverterBlock.GetType(blockData)) },
            { GVPistonBlock.Index, blockData => GVPistonBlock.SetMode(0, GVPistonBlock.GetMode(blockData)) },
            { GV8x4LedBlock.Index, blockData => GV8x4LedBlock.SetType(0, GV8x4LedBlock.GetType(blockData)) },
            { GVDoorBlock.Index, blockData => GVDoorBlock.SetModel(0, GVDoorBlock.GetModel(blockData)) },
            { GVTrapdoorBlock.Index, blockData => GVTrapdoorBlock.SetModel(0, GVTrapdoorBlock.GetModel(blockData)) },
            { GVFenceGateBlock.Index, blockData => GVFenceGateBlock.SetModel(GVFenceGateBlock.SetColor(0, GVFenceGateBlock.GetColor(blockData)), GVFenceGateBlock.GetModel(blockData)) },
            { GVDisplayLedBlock.Index, blockData => GVDisplayLedBlock.SetComplex(GVDisplayLedBlock.SetType(0, GVDisplayLedBlock.GetType(blockData)), GVDisplayLedBlock.GetComplex(blockData)) },
            { GVMoreTwoInTwoOutBlock.Index, blockData => GVMoreTwoInTwoOutBlock.SetType(0, GVMoreTwoInTwoOutBlock.GetType(blockData)) },
            { GVMoreOneInOneOutBlock.Index, blockData => GVMoreOneInOneOutBlock.SetType(0, GVMoreOneInOneOutBlock.GetType(blockData)) },
            { GVInventoryFetcherBlock.Index, blockData => GVInventoryFetcherBlock.SetType(0, GVInventoryFetcherBlock.GetType(blockData)) },
            { GVEWireThroughBlock.Index, blockData => GVEWireThroughBlock.SetType(0, GVEWireThroughBlock.GetType(blockData)) },
            { GVPressurePlateBlock.Index, blockData => GVPressurePlateBlock.SetMaterial(0, GVPressurePlateBlock.GetMaterial(blockData)) },
            { GVPressurePlateCBlock.Index, blockData => GVPressurePlateCBlock.SetMaterial(0, GVPressurePlateCBlock.GetMaterial(blockData)) },
            { GVLightbulbBlock.Index, blockData => GVLightbulbBlock.SetColor(0, GVLightbulbBlock.GetColor(blockData)) },
            { GVWoodenPostedSignCBlock.Index, blockData => GVPostedSignCBlock.SetColor(0, GVPostedSignCBlock.GetColor(blockData)) },
            { GVIronPostedSignCBlock.Index, blockData => GVPostedSignCBlock.SetColor(0, GVPostedSignCBlock.GetColor(blockData)) }
        };

        public static void GotoBlockDescriptionScreen(int blockValue) {
            int blockContent = Terrain.ExtractContents(blockValue);
            if (BlockIndex2HelperInfo.TryGetValue(blockContent, out string[] value)) {
                GotoGVHelpScreen(value[0], value[1]);
            }
            else {
                int newBlockValue = blockContent;
                int blockData = Terrain.ExtractData(blockValue);
                Block block = BlocksManager.Blocks[blockContent];
                if (block.GetCreativeValues().Contains(blockValue)) {
                    newBlockValue = blockValue;
                }
                else if (BlockIndex2DataHandler.TryGetValue(blockContent, out Func<int, int> blockIndex2DataHandler)) {
                    newBlockValue = Terrain.MakeBlockValue(blockContent, 0, blockIndex2DataHandler(blockData));
                }
                ScreensManager.SwitchScreen("RecipaediaDescription", newBlockValue, new List<int> { newBlockValue });
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