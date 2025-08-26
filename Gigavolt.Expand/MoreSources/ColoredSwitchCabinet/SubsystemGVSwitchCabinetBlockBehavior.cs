using Engine;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVSwitchCabinetBlockBehavior : SubsystemBlockBehavior, IGVBlockBehavior {
        public SubsystemGVElectricity m_subsystemGVElectricity;
        public SubsystemAudio m_subsystemAudio;

        public override int[] HandledBlocks => [GVBlocksManager.GetBlockIndex<GVSwitchCabinetBlock>()];

        public override void Load(ValuesDictionary valuesDictionary) {
            base.Load(valuesDictionary);
            m_subsystemGVElectricity = Project.FindSubsystem<SubsystemGVElectricity>(true);
            m_subsystemAudio = Project.FindSubsystem<SubsystemAudio>(true);
        }

        public override void OnBlockAdded(int value, int oldValue, int x, int y, int z) => OnBlockAdded(value, oldValue, x, y, z, null);

        public void OnBlockAdded(int value, int oldValue, int x, int y, int z, GVSubterrainSystem system) {
            Terrain terrain = system == null ? SubsystemTerrain.Terrain : system.Terrain;
            int data = Terrain.ExtractData(terrain.GetCellValue(x, y, z));
            if (!GVSwitchCabinetBlock.GetIsTopPart(data)) {
                int face = GVSwitchCabinetBlock.GetFaceFromDataStatic(data);
                Point3 up = GVSwitchCabinetBlock.m_upPoint3[face] + new Point3(x, y, z);
                if (Terrain.ExtractContents(terrain.GetCellValue(up.X, up.Y, up.Z)) == 0) {
                    Point3 faceDirection = -CellFace.FaceToPoint3(face);
                    int faceValue = terrain.GetCellValue(up.X + faceDirection.X, up.Y + faceDirection.Y, up.Z + faceDirection.Z);
                    Block block = BlocksManager.Blocks[Terrain.ExtractContents(faceValue)];
                    if ((block.IsCollidable_(faceValue) && !block.IsFaceTransparent(SubsystemTerrain, face, faceValue))
                        || (face == 4 && block is FenceBlock)) {
                        if (system == null) {
                            SubsystemTerrain.ChangeCell(
                                up.X,
                                up.Y,
                                up.Z,
                                Terrain.MakeBlockValue(
                                    GVBlocksManager.GetBlockIndex<GVSwitchCabinetBlock>(),
                                    0,
                                    GVSwitchCabinetBlock.SetIsTopPart(data, true)
                                )
                            );
                        }
                        else {
                            system.ChangeCell(
                                up.X,
                                up.Y,
                                up.Z,
                                Terrain.MakeBlockValue(
                                    GVBlocksManager.GetBlockIndex<GVSwitchCabinetBlock>(),
                                    0,
                                    GVSwitchCabinetBlock.SetIsTopPart(data, true)
                                )
                            );
                        }
                        return;
                    }
                }
                if (system == null) {
                    SubsystemTerrain.DestroyCell(
                        int.MaxValue,
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
                        int.MaxValue,
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

        public override void OnBlockRemoved(int value, int newValue, int x, int y, int z) => OnBlockRemoved(value, newValue, x, y, z, null);

        public void OnBlockRemoved(int value, int newValue, int x, int y, int z, GVSubterrainSystem system) {
            int data = Terrain.ExtractData(value);
            int face = GVSwitchCabinetBlock.GetFaceFromDataStatic(data);
            Point3 upDirection = GVSwitchCabinetBlock.m_upPoint3[face];
            bool isUp = GVSwitchCabinetBlock.GetIsTopPart(data);
            Point3 origin = new(x, y, z);
            Point3 another = origin + upDirection * (isUp ? -1 : 1);
            int anotherData = Terrain.ExtractData(
                (system == null ? SubsystemTerrain.Terrain : system.Terrain).GetCellValue(another.X, another.Y, another.Z)
            );
            if (GVSwitchCabinetBlock.GetIsTopPart(anotherData) != isUp
                && GVSwitchCabinetBlock.GetFaceFromDataStatic(anotherData) == face) {
                if (system == null) {
                    SubsystemTerrain.ChangeCell(another.X, another.Y, another.Z, 0);
                }
                else {
                    system.ChangeCell(another.X, another.Y, another.Z, 0);
                }
            }
        }

        public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner) {
            int colorIndex = raycastResult.CollisionBoxIndex - 1;
            if (colorIndex < 0) {
                return true;
            }
            int color = GVSwitchCabinetBlock.ColorIndex2Color[colorIndex];
            int contents = Terrain.ExtractContents(raycastResult.Value);
            int data = Terrain.ExtractData(raycastResult.Value);
            int face = GVSwitchCabinetBlock.GetFaceFromDataStatic(data);
            Point3 upDirection = GVSwitchCabinetBlock.m_upPoint3[face];
            bool isUp = GVSwitchCabinetBlock.GetIsTopPart(data);
            Point3 origin = raycastResult.CellFace.Point;
            Point3 another = origin + upDirection * (isUp ? -1 : 1);
            int anotherData = Terrain.ExtractData(SubsystemTerrain.Terrain.GetCellValue(another.X, another.Y, another.Z));
            if (GVSwitchCabinetBlock.GetIsTopPart(anotherData) != isUp
                && GVSwitchCabinetBlock.GetFaceFromDataStatic(anotherData) == face) {
                if (m_subsystemGVElectricity.GetGVElectricElement(origin.X, origin.Y, origin.Z, face, 0, 1 << color) is SwitchCabinetGVElectricElement
                    element) {
                    bool newLeverState = !GVSwitchCabinetBlock.GetLeverState(data, color);
                    element.m_on = newLeverState;
                    SubsystemTerrain.ChangeCell(
                        origin.X,
                        origin.Y,
                        origin.Z,
                        Terrain.MakeBlockValue(contents, 0, GVSwitchCabinetBlock.SetLeverState(data, color, newLeverState))
                    );
                    SubsystemTerrain.ChangeCell(
                        another.X,
                        another.Y,
                        another.Z,
                        Terrain.MakeBlockValue(contents, 0, GVSwitchCabinetBlock.SetLeverState(anotherData, color, newLeverState))
                    );
                    SubsystemTerrain.Terrain.GetChunkAtCell(origin.X, origin.Z).GeneratedSliceContentsHashes[origin.Y / 16] = 0;
                    SubsystemTerrain.Terrain.GetChunkAtCell(another.X, another.Z).GeneratedSliceContentsHashes[another.Y / 16] = 0;
                    m_subsystemGVElectricity.QueueGVElectricElementForSimulation(element, m_subsystemGVElectricity.CircuitStep + 1);
                    m_subsystemAudio.PlaySound("Audio/Click", 1f, 0f, raycastResult.HitPoint(), 2f, true);
                }
            }
            return true;
        }
    }
}