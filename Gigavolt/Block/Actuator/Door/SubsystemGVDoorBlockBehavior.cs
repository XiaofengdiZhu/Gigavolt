using Engine;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVDoorBlockBehavior : SubsystemBlockBehavior, IGVBlockBehavior {
        public SubsystemGVElectricity m_subsystemElectricity;
        public SubsystemAudio m_subsystemAudio;

        public static Random m_random = new();

        public override int[] HandledBlocks => [GVDoorBlock.Index];

        public bool OpenCloseDoor(int x, int y, int z, bool open) {
            int cellValue = SubsystemTerrain.Terrain.GetCellValue(x, y, z);
            int num = Terrain.ExtractContents(cellValue);
            if (BlocksManager.Blocks[num] is GVDoorBlock) {
                int data = GVDoorBlock.SetOpen(Terrain.ExtractData(cellValue), open ? 90 : 0);
                int value = Terrain.ReplaceData(cellValue, data);
                SubsystemTerrain.ChangeCell(x, y, z, value);
                string name = open ? "Audio/Doors/DoorOpen" : "Audio/Doors/DoorClose";
                m_subsystemAudio.PlaySound(
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

        public void OpenDoor(int x, int y, int z, uint subterrainId, int open) {
            if (subterrainId == 0) {
                int cellValue = SubsystemTerrain.Terrain.GetCellValue(x, y, z);
                if (BlocksManager.Blocks[Terrain.ExtractContents(cellValue)] is GVDoorBlock) {
                    SubsystemTerrain.ChangeCell(x, y, z, Terrain.ReplaceData(cellValue, GVDoorBlock.SetOpen(Terrain.ExtractData(cellValue), open)));
                }
            }
            else {
                GVSubterrainSystem subterrainSystem = GVStaticStorage.GVSubterrainSystemDictionary[subterrainId];
                int cellValue = subterrainSystem.Terrain.GetCellValue(x, y, z);
                if (BlocksManager.Blocks[Terrain.ExtractContents(cellValue)] is GVDoorBlock) {
                    subterrainSystem.ChangeCell(x, y, z, Terrain.ReplaceData(cellValue, GVDoorBlock.SetOpen(Terrain.ExtractData(cellValue), open)));
                }
            }
        }

        public bool IsDoorElectricallyConnected(int x, int y, int z, uint subterrainId) {
            int cellValue = SubsystemTerrain.Terrain.GetCellValue(x, y, z);
            int num = Terrain.ExtractContents(cellValue);
            int data = Terrain.ExtractData(cellValue);
            if (BlocksManager.Blocks[num] is GVDoorBlock) {
                int num2 = GVDoorBlock.IsBottomPart(SubsystemTerrain.Terrain, x, y, z) ? y : y - 1;
                for (int i = num2; i <= num2 + 1; i++) {
                    GVElectricElement electricElement = m_subsystemElectricity.GetGVElectricElement(
                        x,
                        i,
                        z,
                        GVDoorBlock.GetHingeFace(data),
                        subterrainId
                    );
                    if (electricElement != null
                        && electricElement.Connections.Count > 0) {
                        return true;
                    }
                }
            }
            return false;
        }

        public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner) {
            CellFace cellFace = raycastResult.CellFace;
            int cellValue = SubsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z);
            int data = Terrain.ExtractData(cellValue);
            if (GVDoorBlock.GetModel(data) == 0
                || !IsDoorElectricallyConnected(cellFace.X, cellFace.Y, cellFace.Z, 0)) {
                bool open = GVDoorBlock.GetOpen(data) > 0;
                return OpenCloseDoor(cellFace.X, cellFace.Y, cellFace.Z, !open);
            }
            return true;
        }

        public override void OnBlockAdded(int value, int oldValue, int x, int y, int z) {
            int cellValue = SubsystemTerrain.Terrain.GetCellValue(x, y - 1, z);
            int cellValue2 = SubsystemTerrain.Terrain.GetCellValue(x, y + 1, z);
            if (!BlocksManager.Blocks[Terrain.ExtractContents(cellValue)].IsTransparent_(cellValue)
                && Terrain.ExtractContents(cellValue2) == 0) {
                SubsystemTerrain.ChangeCell(x, y + 1, z, value);
            }
        }

        public override void OnBlockRemoved(int value, int newValue, int x, int y, int z) => OnBlockRemoved(
            value,
            newValue,
            x,
            y,
            z,
            null
        );

        public void OnBlockRemoved(int value, int newValue, int x, int y, int z, GVSubterrainSystem system) {
            int offset = 0;
            if (GVDoorBlock.IsTopPart(system == null ? SubsystemTerrain.Terrain : system.Terrain, x, y, z)) {
                offset = -1;
            }
            if (GVDoorBlock.IsBottomPart(system == null ? SubsystemTerrain.Terrain : system.Terrain, x, y, z)) {
                offset = 1;
            }
            if (system == null) {
                SubsystemTerrain.ChangeCell(x, y + offset, z, 0);
            }
            else {
                system.ChangeCell(x, y + offset, z, 0);
            }
        }

        public override void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ) => OnNeighborBlockChanged(
            x,
            y,
            z,
            neighborX,
            neighborY,
            neighborZ,
            null
        );

        public void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ, GVSubterrainSystem system) {
            Terrain terrain = system == null ? SubsystemTerrain.Terrain : system.Terrain;
            int cellValue = terrain.GetCellValue(x, y, z);
            int num = Terrain.ExtractContents(cellValue);
            Block obj = BlocksManager.Blocks[num];
            int data = Terrain.ExtractData(cellValue);
            if (obj is not GVDoorBlock) {
                return;
            }
            if (neighborX == x
                && neighborY == y
                && neighborZ == z) {
                if (GVDoorBlock.IsBottomPart(terrain, x, y, z)) {
                    int value = Terrain.ReplaceData(terrain.GetCellValue(x, y + 1, z), data);
                    SubsystemTerrain.ChangeCell(x, y + 1, z, value);
                }
                if (GVDoorBlock.IsTopPart(terrain, x, y, z)) {
                    int value2 = Terrain.ReplaceData(terrain.GetCellValue(x, y - 1, z), data);
                    SubsystemTerrain.ChangeCell(x, y - 1, z, value2);
                }
            }
            bool flag = false;
            if (GVDoorBlock.IsBottomPart(terrain, x, y, z)) {
                int cellValue2 = terrain.GetCellValue(x, y - 1, z);
                if (BlocksManager.Blocks[Terrain.ExtractContents(cellValue2)].IsTransparent_(cellValue2)) {
                    flag = true;
                }
            }
            if (!GVDoorBlock.IsBottomPart(terrain, x, y, z)
                && !GVDoorBlock.IsTopPart(terrain, x, y, z)) {
                flag = true;
            }
            if (flag) {
                if (system == null) {
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
                else {
                    system.DestroyCell(
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
            m_subsystemAudio = Project.FindSubsystem<SubsystemAudio>(true);
        }
    }
}