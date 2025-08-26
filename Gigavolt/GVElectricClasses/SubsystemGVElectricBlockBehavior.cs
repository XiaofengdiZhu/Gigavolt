using System.Collections.Generic;
using Engine;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVElectricBlockBehavior : SubsystemBlockBehavior, IGVBlockBehavior {
        public SubsystemGVElectricity m_subsystemGVElectric;
        public HashSet<Point2> m_usingChunks = [];

        public override int[] HandledBlocks => [
            GVBlocksManager.GetBlockIndex<GVOneLedCBlock>(),
            GVBlocksManager.GetBlockIndex<GVMulticoloredLedCBlock>(),
            GVBlocksManager.GetBlockIndex<GVSoundGeneratorCBlock>(),
            GVBlocksManager.GetBlockIndex<GVCounterCBlock>(),
            GVBlocksManager.GetBlockIndex<GVMemoryBankCBlock>(),
            GVBlocksManager.GetBlockIndex<GVTruthTableCircuitCBlock>(),
            GVBlocksManager.GetBlockIndex<GVWireBlock>(),
            GVBlocksManager.GetBlockIndex<GVNandGateBlock>(),
            GVBlocksManager.GetBlockIndex<GVNorGateBlock>(),
            GVBlocksManager.GetBlockIndex<GVAndGateBlock>(),
            GVBlocksManager.GetBlockIndex<GVBatteryBlock>(),
            GVBlocksManager.GetBlockIndex<GVLightbulbBlock>(),
            GVBlocksManager.GetBlockIndex<GVNotGateBlock>(),
            GVBlocksManager.GetBlockIndex<GVSwitchBlock>(),
            GVBlocksManager.GetBlockIndex<GVButtonBlock>(),
            GVBlocksManager.GetBlockIndex<GVOrGateBlock>(),
            GVBlocksManager.GetBlockIndex<GVPressurePlateBlock>(),
            GVBlocksManager.GetBlockIndex<GVDelayGateBlock>(),
            GVBlocksManager.GetBlockIndex<GVSRLatchBlock>(),
            GVBlocksManager.GetBlockIndex<GVDetonatorBlock>(),
            GVBlocksManager.GetBlockIndex<GVPhotodiodeBlock>(),
            GVBlocksManager.GetBlockIndex<GVLedBlock>(),
            GVBlocksManager.GetBlockIndex<GVWireThroughPlanksBlock>(),
            GVBlocksManager.GetBlockIndex<GVWireThroughStoneBlock>(),
            GVBlocksManager.GetBlockIndex<GVWireThroughSemiconductorBlock>(),
            GVBlocksManager.GetBlockIndex<GVXorGateBlock>(),
            GVBlocksManager.GetBlockIndex<GVRandomGeneratorBlock>(),
            GVBlocksManager.GetBlockIndex<GVMotionDetectorBlock>(),
            GVBlocksManager.GetBlockIndex<GVDigitalToAnalogConverterBlock>(),
            GVBlocksManager.GetBlockIndex<GVAnalogToDigitalConverterBlock>(),
            GVBlocksManager.GetBlockIndex<GVFourLedBlock>(),
            GVBlocksManager.GetBlockIndex<GVSoundGeneratorBlock>(),
            GVBlocksManager.GetBlockIndex<GVCounterBlock>(),
            GVBlocksManager.GetBlockIndex<GVSevenSegmentDisplayBlock>(),
            GVBlocksManager.GetBlockIndex<GVMemoryBankBlock>(),
            GVBlocksManager.GetBlockIndex<GVRealTimeClockBlock>(),
            GVBlocksManager.GetBlockIndex<GVTruthTableCircuitBlock>(),
            GVBlocksManager.GetBlockIndex<GVTargetBlock>(),
            GVBlocksManager.GetBlockIndex<GVWireThroughBricksBlock>(),
            GVBlocksManager.GetBlockIndex<GVAdjustableDelayGateBlock>(),
            GVBlocksManager.GetBlockIndex<GVPistonBlock>(),
            GVBlocksManager.GetBlockIndex<GVWireThroughCobblestoneBlock>(),
            GVBlocksManager.GetBlockIndex<GVOneLedBlock>(),
            GVBlocksManager.GetBlockIndex<GVMulticoloredLedBlock>(),
            GVBlocksManager.GetBlockIndex<GV8x4LedBlock>(),
            GVBlocksManager.GetBlockIndex<GVSignBlock>(),
            GVBlocksManager.GetBlockIndex<GV2OTransformerBlock>(),
            GVBlocksManager.GetBlockIndex<O2GVTransformerBlock>(),
            GVBlocksManager.GetBlockIndex<GVDoorBlock>(),
            GVBlocksManager.GetBlockIndex<GVTrapdoorBlock>(),
            GVBlocksManager.GetBlockIndex<GVFenceGateBlock>(),
            GVBlocksManager.GetBlockIndex<GV8NumberLedBlock>(),
            GVBlocksManager.GetBlockIndex<GVDispenserCBlock>(),
            GVBlocksManager.GetBlockIndex<GVDispenserBlock>(),
            GVBlocksManager.GetBlockIndex<GVDebugBlock>(),
            GVBlocksManager.GetBlockIndex<GVWireHarnessBlock>(),
            GVBlocksManager.GetBlockIndex<FurnitureBlock>(),
            GVBlocksManager.GetBlockIndex<GVSignCBlock>()
        ];

        public override void OnBlockGenerated(int value, int x, int y, int z, bool isLoaded) {
            m_subsystemGVElectric.OnGVElectricElementBlockGenerated(x, y, z, 0);
            m_usingChunks.Add(new Point2(x >> 4, z >> 4));
        }

        public void OnBlockGenerated(int value, int x, int y, int z, bool isLoaded, GVSubterrainSystem system) =>
            m_subsystemGVElectric.OnGVElectricElementBlockGenerated(x, y, z, system.ID);

        public override void OnBlockAdded(int value, int oldValue, int x, int y, int z) {
            m_subsystemGVElectric.OnGVElectricElementBlockAdded(x, y, z, 0);
            m_usingChunks.Add(new Point2(x >> 4, z >> 4));
        }

        public void OnBlockAdded(int value, int oldValue, int x, int y, int z, GVSubterrainSystem system) =>
            m_subsystemGVElectric.OnGVElectricElementBlockAdded(x, y, z, system.ID);

        public override void OnBlockRemoved(int value, int newValue, int x, int y, int z) =>
            m_subsystemGVElectric.OnGVElectricElementBlockRemoved(x, y, z, 0);

        public void OnBlockRemoved(int value, int newValue, int x, int y, int z, GVSubterrainSystem system) =>
            m_subsystemGVElectric.OnGVElectricElementBlockRemoved(x, y, z, system.ID);

        public override void OnBlockModified(int value, int oldValue, int x, int y, int z) =>
            m_subsystemGVElectric.OnGVElectricElementBlockModified(x, y, z, 0);

        public void OnBlockModified(int value, int oldValue, int x, int y, int z, GVSubterrainSystem system) =>
            m_subsystemGVElectric.OnGVElectricElementBlockModified(x, y, z, system.ID);

        public override void OnChunkDiscarding(TerrainChunk chunk) => m_subsystemGVElectric.OnChunkDiscarding(chunk);

        public override void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ) {
            for (int i = 0; i < 6; i++) {
                m_subsystemGVElectric.GetGVElectricElement(x, y, z, i, 0)
                    ?.OnNeighborBlockChanged(new CellFace(x, y, z, i), neighborX, neighborY, neighborZ);
            }
        }

        public void OnNeighborBlockChanged(int x,
            int y,
            int z,
            int neighborX,
            int neighborY,
            int neighborZ,
            GVSubterrainSystem system) {
            for (int i = 0; i < 6; i++) {
                m_subsystemGVElectric.GetGVElectricElement(x, y, z, i, system.ID)
                    ?.OnNeighborBlockChanged(new CellFace(x, y, z, i), neighborX, neighborY, neighborZ);
            }
        }

        public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner) {
            int x = raycastResult.CellFace.X;
            int y = raycastResult.CellFace.Y;
            int z = raycastResult.CellFace.Z;
            for (int i = 0; i < 6; i++) {
                GVElectricElement GVElectricElement = m_subsystemGVElectric.GetGVElectricElement(x, y, z, i, 0);
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
                    GVElectricElement = m_subsystemGVElectric.GetGVElectricElement(x, y, z, num, 0);
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
                    GVElectricElement = m_subsystemGVElectric.GetGVElectricElement(x, y, z, num, 0);
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
            string str = valuesDictionary.GetValue("UsingChunks", string.Empty);
            if (str.Length > 0) {
                foreach (string chunk in str.Split(';')) {
                    string[] split = chunk.Split(',');
                    m_usingChunks.Add(new Point2(int.Parse(split[0]), int.Parse(split[1])));
                }
            }
        }

        public override void Save(ValuesDictionary valuesDictionary) {
            HashSet<Point2> usingChunks = [];
            foreach (GVCellFace cellFace in Project.FindSubsystem<SubsystemGVElectricity>(true).m_GVElectricElementsByCellFace[0u].Keys) {
                usingChunks.Add(new Point2(cellFace.X >> 4, cellFace.Z >> 4));
            }
            valuesDictionary.SetValue("UsingChunks", string.Join(";", usingChunks));
        }

        public override void Dispose() {
            m_usingChunks.Clear();
        }
    }
}