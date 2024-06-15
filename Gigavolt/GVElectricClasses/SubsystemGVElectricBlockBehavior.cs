using Engine;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVElectricBlockBehavior : SubsystemBlockBehavior, IGVBlockBehavior {
        public SubsystemGVElectricity m_subsystemGVElectric;

        public override int[] HandledBlocks => new[] {
            GVPressurePlateCBlock.Index,
            GVSRLatchCBlock.Index,
            GVDetonatorCBlock.Index,
            GVOneLedCBlock.Index,
            GVMulticoloredLedCBlock.Index,
            GVAnalogToDigitalConverterCBlock.Index,
            GVDigitalToAnalogConverterCBlock.Index,
            GVSoundGeneratorCBlock.Index,
            GVCounterCBlock.Index,
            GVMemoryBankCBlock.Index,
            GVTruthTableCircuitCBlock.Index,
            GVTargetCBlock.Index,
            GVWoodenPostedSignCBlock.Index,
            GVWoodenAttachedSignCBlock.Index,
            GVWireBlock.Index,
            GVNandGateBlock.Index,
            GVNorGateBlock.Index,
            GVAndGateBlock.Index,
            GVBatteryBlock.Index,
            GVLightbulbBlock.Index,
            GVNotGateBlock.Index,
            GVSwitchBlock.Index,
            GVButtonBlock.Index,
            GVOrGateBlock.Index,
            GVPressurePlateBlock.Index,
            GVDelayGateBlock.Index,
            GVSRLatchBlock.Index,
            GVDetonatorBlock.Index,
            GVPhotodiodeBlock.Index,
            GVLedBlock.Index,
            GVWireThroughPlanksBlock.Index,
            GVWireThroughStoneBlock.Index,
            GVWireThroughSemiconductorBlock.Index,
            GVXorGateBlock.Index,
            GVRandomGeneratorBlock.Index,
            GVMotionDetectorBlock.Index,
            GVDigitalToAnalogConverterBlock.Index,
            GVAnalogToDigitalConverterBlock.Index,
            GVFourLedBlock.Index,
            GVSoundGeneratorBlock.Index,
            GVCounterBlock.Index,
            GVSevenSegmentDisplayBlock.Index,
            GVMemoryBankBlock.Index,
            GVRealTimeClockBlock.Index,
            GVRealTimeClockCBlock.Index,
            GVTruthTableCircuitBlock.Index,
            GVTargetBlock.Index,
            GVIronPostedSignCBlock.Index,
            GVIronAttachedSignCBlock.Index,
            GVWireThroughBricksBlock.Index,
            GVAdjustableDelayGateBlock.Index,
            GVPistonBlock.Index,
            GVWireThroughCobblestoneBlock.Index,
            GVOneLedBlock.Index,
            GVMulticoloredLedBlock.Index,
            GV8x4LedBlock.Index,
            GVSignBlock.Index,
            GV2OTransformerBlock.Index,
            O2GVTransformerBlock.Index,
            GVDoorBlock.Index,
            GVTrapdoorBlock.Index,
            GVFenceGateBlock.Index,
            GV8NumberLedBlock.Index,
            GVDispenserCBlock.Index,
            GVDispenserBlock.Index,
            GVDebugBlock.Index,
            GVWireHarnessBlock.Index,
            FurnitureBlock.Index
        };

        public override void OnBlockGenerated(int value, int x, int y, int z, bool isLoaded) {
            m_subsystemGVElectric.OnGVElectricElementBlockGenerated(x, y, z, 0);
            GVStaticStorage.GVUsingChunks.Add(new Point2(x >> 4, z >> 4));
        }

        public void OnBlockGenerated(int value, int x, int y, int z, bool isLoaded, GVSubterrainSystem system) => m_subsystemGVElectric.OnGVElectricElementBlockGenerated(x, y, z, system.ID);

        public override void OnBlockAdded(int value, int oldValue, int x, int y, int z) {
            m_subsystemGVElectric.OnGVElectricElementBlockAdded(x, y, z, 0);
            GVStaticStorage.GVUsingChunks.Add(new Point2(x >> 4, z >> 4));
        }

        public void OnBlockAdded(int value, int oldValue, int x, int y, int z, GVSubterrainSystem system) => m_subsystemGVElectric.OnGVElectricElementBlockAdded(x, y, z, system.ID);

        public override void OnBlockRemoved(int value, int newValue, int x, int y, int z) => m_subsystemGVElectric.OnGVElectricElementBlockRemoved(x, y, z, 0);

        public void OnBlockRemoved(int value, int newValue, int x, int y, int z, GVSubterrainSystem system) => m_subsystemGVElectric.OnGVElectricElementBlockRemoved(x, y, z, system.ID);

        public override void OnBlockModified(int value, int oldValue, int x, int y, int z) => m_subsystemGVElectric.OnGVElectricElementBlockModified(x, y, z, 0);

        public void OnBlockModified(int value, int oldValue, int x, int y, int z, GVSubterrainSystem system) => m_subsystemGVElectric.OnGVElectricElementBlockModified(x, y, z, system.ID);

        public override void OnChunkDiscarding(TerrainChunk chunk) => m_subsystemGVElectric.OnChunkDiscarding(chunk);

        public override void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ) {
            for (int i = 0; i < 6; i++) {
                m_subsystemGVElectric.GetGVElectricElement(
                        x,
                        y,
                        z,
                        i,
                        0
                    )
                    ?.OnNeighborBlockChanged(new CellFace(x, y, z, i), neighborX, neighborY, neighborZ);
            }
        }

        public void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ, GVSubterrainSystem system) {
            for (int i = 0; i < 6; i++) {
                m_subsystemGVElectric.GetGVElectricElement(
                        x,
                        y,
                        z,
                        i,
                        system.ID
                    )
                    ?.OnNeighborBlockChanged(new CellFace(x, y, z, i), neighborX, neighborY, neighborZ);
            }
        }

        public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner) {
            int x = raycastResult.CellFace.X;
            int y = raycastResult.CellFace.Y;
            int z = raycastResult.CellFace.Z;
            for (int i = 0; i < 6; i++) {
                GVElectricElement GVElectricElement = m_subsystemGVElectric.GetGVElectricElement(
                    x,
                    y,
                    z,
                    i,
                    0
                );
                if (GVElectricElement != null) {
                    return GVElectricElement.OnInteract(raycastResult, componentMiner);
                }
            }
            return false;
        }

        public override void OnCollide(CellFace cellFace, float velocity, ComponentBody componentBody) {
            int x = cellFace.X;
            int y = cellFace.Y;
            int z = cellFace.Z;
            int num = 0;
            GVElectricElement GVElectricElement;
            while (true) {
                if (num < 6) {
                    GVElectricElement = m_subsystemGVElectric.GetGVElectricElement(
                        x,
                        y,
                        z,
                        num,
                        0
                    );
                    if (GVElectricElement != null) {
                        break;
                    }
                    num++;
                    continue;
                }
                return;
            }
            GVElectricElement.OnCollide(cellFace, velocity, componentBody);
        }

        public override void OnHitByProjectile(CellFace cellFace, WorldItem worldItem) {
            int x = cellFace.X;
            int y = cellFace.Y;
            int z = cellFace.Z;
            int num = 0;
            GVElectricElement GVElectricElement;
            while (true) {
                if (num < 6) {
                    GVElectricElement = m_subsystemGVElectric.GetGVElectricElement(
                        x,
                        y,
                        z,
                        num,
                        0
                    );
                    if (GVElectricElement != null) {
                        break;
                    }
                    num++;
                    continue;
                }
                return;
            }
            GVElectricElement.OnHitByProjectile(cellFace, worldItem);
        }

        public override void Load(ValuesDictionary valuesDictionary) {
            base.Load(valuesDictionary);
            m_subsystemGVElectric = Project.FindSubsystem<SubsystemGVElectricity>(true);
        }
    }
}