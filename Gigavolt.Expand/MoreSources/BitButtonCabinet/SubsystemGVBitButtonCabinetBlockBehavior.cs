using System;
using Engine;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVBitButtonCabinetBlockBehavior : SubsystemBlockBehavior, IGVBlockBehavior {
        public SubsystemGVElectricity m_subsystemGVElectricity;
        public SubsystemAudio m_subsystemAudio;

        public override int[] HandledBlocks => [GVBlocksManager.GetBlockIndex<GVBitButtonCabinetBlock>()];

        public override void Load(ValuesDictionary valuesDictionary) {
            base.Load(valuesDictionary);
            m_subsystemGVElectricity = Project.FindSubsystem<SubsystemGVElectricity>(true);
            m_subsystemAudio = Project.FindSubsystem<SubsystemAudio>(true);
        }

        public override void OnBlockAdded(int value, int oldValue, int x, int y, int z) => OnBlockAdded(
            value,
            oldValue,
            x,
            y,
            z,
            null
        );

        public void OnBlockAdded(int value, int oldValue, int x, int y, int z, GVSubterrainSystem system) {
            Terrain terrain = system == null ? SubsystemTerrain.Terrain : system.Terrain;
            int data = Terrain.ExtractData(terrain.GetCellValue(x, y, z));
            if (!GVBitButtonCabinetBlock.GetIsTopPart(data)) {
                int face = GVBitButtonCabinetBlock.GetFaceFromDataStatic(data);
                Point3 up = GVBitButtonCabinetBlock.m_upPoint3[face] + new Point3(x, y, z);
                if (Terrain.ExtractContents(terrain.GetCellValue(up.X, up.Y, up.Z)) == 0) {
                    Point3 faceDirection = -CellFace.FaceToPoint3(face);
                    int faceValue = terrain.GetCellValue(up.X + faceDirection.X, up.Y + faceDirection.Y, up.Z + faceDirection.Z);
                    Block block = BlocksManager.Blocks[Terrain.ExtractContents(faceValue)];
                    if ((block.IsCollidable_(faceValue) && !block.IsFaceTransparent(SubsystemTerrain, face, faceValue))
                        || (face == 4 && block is FenceBlock)) {
                        if (system == null) {
                            SubsystemTerrain.ChangeCell(up.X, up.Y, up.Z, Terrain.MakeBlockValue(GVBlocksManager.GetBlockIndex<GVBitButtonCabinetBlock>(), 0, GVBitButtonCabinetBlock.SetIsTopPart(data, true)));
                        }
                        else {
                            system.ChangeCell(up.X, up.Y, up.Z, Terrain.MakeBlockValue(GVBlocksManager.GetBlockIndex<GVBitButtonCabinetBlock>(), 0, GVBitButtonCabinetBlock.SetIsTopPart(data, true)));
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

        public override void OnBlockRemoved(int value, int newValue, int x, int y, int z) => OnBlockRemoved(
            value,
            newValue,
            x,
            y,
            z,
            null
        );

        public void OnBlockRemoved(int value, int newValue, int x, int y, int z, GVSubterrainSystem system) {
            int data = Terrain.ExtractData(value);
            int face = GVBitButtonCabinetBlock.GetFaceFromDataStatic(data);
            Point3 upDirection = GVBitButtonCabinetBlock.m_upPoint3[face];
            bool isUp = GVBitButtonCabinetBlock.GetIsTopPart(data);
            Point3 origin = new(x, y, z);
            Point3 another = origin + upDirection * (isUp ? -1 : 1);
            int anotherData = Terrain.ExtractData((system == null ? SubsystemTerrain.Terrain : system.Terrain).GetCellValue(another.X, another.Y, another.Z));
            if (GVBitButtonCabinetBlock.GetIsTopPart(anotherData) != isUp
                && GVBitButtonCabinetBlock.GetFaceFromDataStatic(anotherData) == face) {
                if (system == null) {
                    SubsystemTerrain.ChangeCell(another.X, another.Y, another.Z, 0);
                }
                else {
                    system.ChangeCell(another.X, another.Y, another.Z, 0);
                }
            }
        }

        public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner) {
            int bitIndex = raycastResult.CollisionBoxIndex - 1;
            Console.WriteLine(bitIndex);
            if (bitIndex < 0) {
                return true;
            }
            int data = Terrain.ExtractData(raycastResult.Value);
            int face = GVBitButtonCabinetBlock.GetFaceFromDataStatic(data);
            Point3 upDirection = GVBitButtonCabinetBlock.m_upPoint3[face];
            bool isUp = GVBitButtonCabinetBlock.GetIsTopPart(data);
            Point3 origin = raycastResult.CellFace.Point;
            Point3 another = origin + upDirection * (isUp ? -1 : 1);
            int anotherData = Terrain.ExtractData(SubsystemTerrain.Terrain.GetCellValue(another.X, another.Y, another.Z));
            if (GVBitButtonCabinetBlock.GetIsTopPart(anotherData) != isUp
                && GVBitButtonCabinetBlock.GetFaceFromDataStatic(anotherData) == face) {
                if (m_subsystemGVElectricity.GetGVElectricElement(
                        origin.X,
                        origin.Y,
                        origin.Z,
                        face,
                        0
                    ) is BitButtonCabinetGVElectricElement element) {
                    element.m_pressedMask = bitIndex;
                    m_subsystemGVElectricity.QueueGVElectricElementForSimulation(element, m_subsystemGVElectricity.CircuitStep + 1);
                    m_subsystemAudio.PlaySound(
                        "Audio/Click",
                        1f,
                        0f,
                        raycastResult.HitPoint(),
                        2f,
                        true
                    );
                }
            }
            return true;
        }

        public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer) {
            int data = Terrain.ExtractData(value);
            DialogsManager.ShowDialog(
                componentPlayer.GuiWidget,
                new EditGVButtonCabinetDialog(
                    GVBitButtonCabinetBlock.GetDuration(data),
                    delegate(int newDuration) {
                        int newData = GVBitButtonCabinetBlock.SetDuration(data, newDuration);
                        if (newData != data) {
                            int face = GVBitButtonCabinetBlock.GetFaceFromDataStatic(data);
                            Point3 upDirection = GVBitButtonCabinetBlock.m_upPoint3[face];
                            bool isUp = GVBitButtonCabinetBlock.GetIsTopPart(data);
                            Point3 another = new Point3(x, y, z) + upDirection * (isUp ? -1 : 1);
                            SubsystemTerrain.ChangeCell(x, y, z, Terrain.ReplaceData(value, newData));
                            SubsystemTerrain.ChangeCell(another.X, another.Y, another.Z, Terrain.ReplaceData(value, GVBitButtonCabinetBlock.SetIsTopPart(newData, !isUp)));
                        }
                    }
                )
            );
            return true;
        }

        public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer) {
            int value = inventory.GetSlotValue(slotIndex);
            int count = inventory.GetSlotCount(slotIndex);
            int data = Terrain.ExtractData(value);
            DialogsManager.ShowDialog(
                componentPlayer.GuiWidget,
                new EditGVButtonCabinetDialog(
                    GVBitButtonCabinetBlock.GetDuration(data),
                    delegate(int newDuration) {
                        int newData = GVBitButtonCabinetBlock.SetDuration(data, newDuration);
                        if (newData != data) {
                            inventory.RemoveSlotItems(slotIndex, count);
                            inventory.AddSlotItems(slotIndex, Terrain.ReplaceData(value, newData), 1);
                        }
                    }
                )
            );
            return true;
        }
    }
}