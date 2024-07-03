using Engine;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVFenceGateBlockBehavior : SubsystemBlockBehavior {
        public SubsystemGVElectricity m_subsystemElectricity;

        public static Random m_random = new();

        public override int[] HandledBlocks => new[] { GVFenceGateBlock.Index };

        public bool OpenCloseGate(int x, int y, int z, bool open) {
            int cellValue = SubsystemTerrain.Terrain.GetCellValue(x, y, z);
            int num = Terrain.ExtractContents(cellValue);
            if (BlocksManager.Blocks[num] is GVFenceGateBlock) {
                int data = GVFenceGateBlock.SetOpen(Terrain.ExtractData(cellValue), open ? 90 : 0);
                int value = Terrain.ReplaceData(cellValue, data);
                SubsystemTerrain.ChangeCell(x, y, z, value);
                string name = open ? "Audio/Doors/DoorOpen" : "Audio/Doors/DoorClose";
                SubsystemTerrain.Project.FindSubsystem<SubsystemAudio>(true)
                .PlaySound(
                    name,
                    0.7f,
                    m_random.Float(-0.1f, 0.1f),
                    new Vector3(x, y, z),
                    4f,
                    true
                );
                return true;
            }
            return false;
        }

        public void OpenGate(int x, int y, int z, uint subterrainId, int open) {
            if (subterrainId == 0) {
                int cellValue = SubsystemTerrain.Terrain.GetCellValue(x, y, z);
                if (BlocksManager.Blocks[Terrain.ExtractContents(cellValue)] is GVFenceGateBlock) {
                    SubsystemTerrain.ChangeCell(x, y, z, Terrain.ReplaceData(cellValue, GVFenceGateBlock.SetOpen(Terrain.ExtractData(cellValue), open)));
                }
            }
            else {
                GVSubterrainSystem subterrainSystem = GVStaticStorage.GVSubterrainSystemDictionary[subterrainId];
                int cellValue = subterrainSystem.Terrain.GetCellValue(x, y, z);
                if (BlocksManager.Blocks[Terrain.ExtractContents(cellValue)] is GVFenceGateBlock) {
                    subterrainSystem.ChangeCell(x, y, z, Terrain.ReplaceData(cellValue, GVFenceGateBlock.SetOpen(Terrain.ExtractData(cellValue), open)));
                }
            }
        }

        public bool IsGateElectricallyConnected(int x, int y, int z, uint subterrainId) {
            int cellValue = SubsystemTerrain.Terrain.GetCellValue(x, y, z);
            int num = Terrain.ExtractContents(cellValue);
            int data = Terrain.ExtractData(cellValue);
            if (BlocksManager.Blocks[num] is GVFenceGateBlock) {
                GVElectricElement electricElement = m_subsystemElectricity.GetGVElectricElement(
                    x,
                    y,
                    z,
                    GVFenceGateBlock.GetHingeFace(data),
                    subterrainId
                );
                if (electricElement != null
                    && electricElement.Connections.Count > 0) {
                    return true;
                }
            }
            return false;
        }

        public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner) {
            CellFace cellFace = raycastResult.CellFace;
            int cellValue = SubsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z);
            int data = Terrain.ExtractData(cellValue);
            if (GVFenceGateBlock.GetModel(data) == 0
                || !IsGateElectricallyConnected(cellFace.X, cellFace.Y, cellFace.Z, 0)) {
                bool open = GVFenceGateBlock.GetOpen(data) > 0;
                return OpenCloseGate(cellFace.X, cellFace.Y, cellFace.Z, !open);
            }
            return true;
        }

        public override void Load(ValuesDictionary valuesDictionary) {
            base.Load(valuesDictionary);
            m_subsystemElectricity = Project.FindSubsystem<SubsystemGVElectricity>(true);
        }
    }
}