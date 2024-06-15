using Engine;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVTrapdoorBlockBehavior : SubsystemBlockBehavior {
        public SubsystemGVElectricity m_subsystemElectricity;

        public static Random m_random = new();

        public override int[] HandledBlocks => new[] { GVTrapdoorBlock.Index };

        public bool IsTrapdoorElectricallyConnected(int x, int y, int z, uint subterrainId) {
            int cellValue = SubsystemTerrain.Terrain.GetCellValue(x, y, z);
            int num = Terrain.ExtractContents(cellValue);
            int data = Terrain.ExtractData(cellValue);
            if (BlocksManager.Blocks[num] is GVTrapdoorBlock) {
                GVElectricElement electricElement = m_subsystemElectricity.GetGVElectricElement(
                    x,
                    y,
                    z,
                    GVTrapdoorBlock.GetMountingFace(data),
                    subterrainId
                );
                if (electricElement != null
                    && electricElement.Connections.Count > 0) {
                    return true;
                }
            }
            return false;
        }

        public bool OpenCloseTrapdoor(int x, int y, int z, bool open) {
            int cellValue = SubsystemTerrain.Terrain.GetCellValue(x, y, z);
            int num = Terrain.ExtractContents(cellValue);
            if (BlocksManager.Blocks[num] is GVTrapdoorBlock) {
                int data = GVTrapdoorBlock.SetOpen(Terrain.ExtractData(cellValue), open ? 90 : 0);
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

        public void OpenTrapdoor(int x, int y, int z, uint subterrainId, int open) {
            if (subterrainId == 0) {
                int cellValue = SubsystemTerrain.Terrain.GetCellValue(x, y, z);
                if (BlocksManager.Blocks[Terrain.ExtractContents(cellValue)] is GVTrapdoorBlock) {
                    SubsystemTerrain.ChangeCell(x, y, z, Terrain.ReplaceData(cellValue, GVTrapdoorBlock.SetOpen(Terrain.ExtractData(cellValue), open)));
                }
            }
            else {
                GVSubterrainSystem subterrainSystem = GVStaticStorage.GVSubterrainSystemDictionary[subterrainId];
                int cellValue = subterrainSystem.Terrain.GetCellValue(x, y, z);
                if (BlocksManager.Blocks[Terrain.ExtractContents(cellValue)] is GVTrapdoorBlock) {
                    subterrainSystem.ChangeCell(x, y, z, Terrain.ReplaceData(cellValue, GVTrapdoorBlock.SetOpen(Terrain.ExtractData(cellValue), open)));
                }
            }
        }

        public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner) {
            CellFace cellFace = raycastResult.CellFace;
            int cellValue = SubsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z);
            int data = Terrain.ExtractData(cellValue);
            if (GVTrapdoorBlock.GetModel(data) == 0
                || !IsTrapdoorElectricallyConnected(cellFace.X, cellFace.Y, cellFace.Z, 0)) {
                bool open = GVTrapdoorBlock.GetOpen(data) > 0;
                return OpenCloseTrapdoor(cellFace.X, cellFace.Y, cellFace.Z, !open);
            }
            return true;
        }

        public override void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ) {
            int cellValue = SubsystemTerrain.Terrain.GetCellValue(x, y, z);
            int num = Terrain.ExtractContents(cellValue);
            Block obj = BlocksManager.Blocks[num];
            int data = Terrain.ExtractData(cellValue);
            if (obj is GVTrapdoorBlock) {
                int rotation = GVTrapdoorBlock.GetRotation(data);
                bool upsideDown = GVTrapdoorBlock.GetUpsideDown(data);
                bool flag = false;
                Point3 point = CellFace.FaceToPoint3(rotation);
                int cellValue2 = SubsystemTerrain.Terrain.GetCellValue(x - point.X, y - point.Y, z - point.Z);
                flag |= !BlocksManager.Blocks[Terrain.ExtractContents(cellValue2)].IsTransparent_(cellValue2);
                if (upsideDown) {
                    int cellValue3 = SubsystemTerrain.Terrain.GetCellValue(x, y + 1, z);
                    flag |= !BlocksManager.Blocks[Terrain.ExtractContents(cellValue3)].IsTransparent_(cellValue3);
                    int cellValue4 = SubsystemTerrain.Terrain.GetCellValue(x - point.X, y - point.Y + 1, z - point.Z);
                    flag |= !BlocksManager.Blocks[Terrain.ExtractContents(cellValue4)].IsTransparent_(cellValue4);
                }
                else {
                    int cellValue5 = SubsystemTerrain.Terrain.GetCellValue(x, y - 1, z);
                    flag |= !BlocksManager.Blocks[Terrain.ExtractContents(cellValue5)].IsTransparent_(cellValue5);
                    int cellValue6 = SubsystemTerrain.Terrain.GetCellValue(x - point.X, y - point.Y - 1, z - point.Z);
                    flag |= !BlocksManager.Blocks[Terrain.ExtractContents(cellValue6)].IsTransparent_(cellValue6);
                }
                if (!flag) {
                    SubsystemTerrain.DestroyCell(
                        0,
                        x,
                        y,
                        z,
                        0,
                        false,
                        false
                    );
                }
            }
        }

        public override void Load(ValuesDictionary valuesDictionary) {
            base.Load(valuesDictionary);
            m_subsystemElectricity = Project.FindSubsystem<SubsystemGVElectricity>(true);
        }
    }
}