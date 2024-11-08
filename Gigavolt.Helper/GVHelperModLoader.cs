namespace Game {
    public class GVHelperModLoader : ModLoader {
        public static GVHelpTopicScreen m_GVHelpTopicScreen = new();

        public override void __ModInitialize() {
            ModsManager.RegisterHook("BlocksInitalized", this);
        }

        public override void BlocksInitalized() {
            foreach (Block block in BlocksManager.Blocks) {
                if (block is IGVBaseBlock baseBlock) {
                    switch (baseBlock) {
                        case GVMemoryBankBlock or GVVolatileMemoryBankBlock or GVListMemoryBankBlock or GVVolatileListMemoryBankBlock or GVFourDimensionalMemoryBankBlock or GVVolatileFourDimensionalMemoryBankBlock or GVTruthTableCircuitBlock or GVSoundGeneratorBlock or GVSignBlock or GVDispenserBlock or GVDebugBlock or GVCopperHammerBlock or GVJumpWireBlock or GVMultiplexerBlock or GVMoreTwoInTwoOutBlock or GVMoreOneInOneOutBlock or GVJavascriptMicrocontrollerBlock or GVOscilloscopeBlock or GVDisplayLedBlock or GVNesEmulatorBlock or GVTerrainRaycastDetectorBlock or GVTerrainScannerBlock or GVPlayerMonitorBlock or GVPlayerControllerBlock or GVCameraBlock or GVGuidedDispenserBlock or GVAttractorBlock or GVInventoryControllerBlock or GVInventoryFetcherBlock or GVTractorBeamBlock or GVSignalGeneratorBlock:
                            baseBlock.GetBlockDescriptionScreenHandler = value => m_GVHelpTopicScreen;
                            break;
                        case GVAnalogToDigitalConverterBlock:
                            baseBlock.GetBlockDescriptionScreenHandler = value => GVAnalogToDigitalConverterBlock.GetClassic(Terrain.ExtractData(value)) ? IGVBaseBlock.DefaultRecipaediaDescriptionScreen : m_GVHelpTopicScreen;
                            break;
                        case GVDigitalToAnalogConverterBlock:
                            baseBlock.GetBlockDescriptionScreenHandler = value => GVDigitalToAnalogConverterBlock.GetClassic(Terrain.ExtractData(value)) ? IGVBaseBlock.DefaultRecipaediaDescriptionScreen : m_GVHelpTopicScreen;
                            break;
                        case GVRealTimeClockBlock:
                            baseBlock.GetBlockDescriptionScreenHandler = value => GVRealTimeClockBlock.GetClassic(Terrain.ExtractData(value)) ? IGVBaseBlock.DefaultRecipaediaDescriptionScreen : m_GVHelpTopicScreen;
                            break;
                        case GVPistonBlock:
                            baseBlock.GetBlockDescriptionScreenHandler = value => GVPistonBlock.GetMode(Terrain.ExtractData(value)) == GVPistonMode.Complex ? m_GVHelpTopicScreen : IGVBaseBlock.DefaultRecipaediaDescriptionScreen;
                            break;
                        case GVEWireThroughBlock:
                            baseBlock.GetBlockDescriptionScreenHandler = value => GVEWireThroughBlock.GetIsCross(Terrain.ExtractData(value)) ? IGVBaseBlock.DefaultRecipaediaDescriptionScreen : m_GVHelpTopicScreen;
                            break;
                    }
                }
            }
        }
    }
}