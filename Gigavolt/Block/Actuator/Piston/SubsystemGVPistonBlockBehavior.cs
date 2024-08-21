using System;
using System.Collections.Generic;
using Engine;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVPistonBlockBehavior : SubsystemGVEditableItemBehavior<GVPistonData>, IUpdateable {
        public class QueuedAction {
            public int StoppedFrame;
            public bool Stop;
            public int? Move;
            public GVPistonData ComplexData;
        }

        public SubsystemTime m_subsystemTime;
        public SubsystemAudio m_subsystemAudio;
        public SubsystemMovingBlocks m_subsystemMovingBlocks;
        public SubsystemGVSubterrain m_subsystemGVSubterrain;

        public Terrain m_movingTerrain;
        public int m_allocatedX = 1;
        public int m_allocatedZ = 1;
        public bool m_allowPistonHeadRemove;

        public readonly Dictionary<GVPoint3, QueuedAction> m_actions = new();
        public readonly List<KeyValuePair<GVPoint3, QueuedAction>> m_tmpActions = [];
        public readonly DynamicArray<MovingBlock> m_movingBlocks = [];
        public readonly Dictionary<GVPoint3, GVPistonData> m_complexPistonData = new();

        public const string IdString = "GVPiston";

        public UpdateOrder UpdateOrder => m_subsystemMovingBlocks.UpdateOrder + 1;

        public override int[] HandledBlocks => [GVPistonBlock.Index, GVPistonHeadBlock.Index];
        public SubsystemGVPistonBlockBehavior() : base(GVPistonBlock.Index) { }

        public void AdjustPiston(GVPoint3 position, int length, GVPistonData pistonData) {
            if (m_actions.TryGetValue(position, out QueuedAction value)) {
                value.Move = length;
                value.ComplexData = pistonData;
            }
            else {
                m_actions.Add(position, new QueuedAction { Move = length, ComplexData = pistonData });
            }
        }

        public void Update(float dt) {
            if (m_subsystemTime.PeriodicGameTimeEvent(0.125, 0.0)) {
                ProcessQueuedActions();
            }
            UpdateMovableBlocks();
        }

        public override int GetIdFromValue(int value) => (Terrain.ExtractData(value) >> 6) & 2047;
        public override int SetIdToValue(int value, int id) => Terrain.ReplaceData(value, (Terrain.ExtractData(value) & -131009) | ((id & 2047) << 6));

        public override bool OnEditInventoryItem(IInventory inventory, int slotIndex, ComponentPlayer componentPlayer) {
            if (componentPlayer.DragHostWidget.IsDragInProgress) {
                return false;
            }
            int value = inventory.GetSlotValue(slotIndex);
            int data = Terrain.ExtractData(value);
            GVPistonMode mode = GVPistonBlock.GetMode(data);
            if (mode == GVPistonMode.Complex) {
                return false;
            }
            int count = inventory.GetSlotCount(slotIndex);
            int id = GetIdFromValue(value);
            GVPistonData blockData = GetItemData(id, true);
            DialogsManager.ShowDialog(
                componentPlayer.GuiWidget,
                new EditGVPistonDialog(
                    GVPistonBlock.GetMode(data),
                    blockData,
                    delegate {
                        inventory.RemoveSlotItems(slotIndex, count);
                        inventory.AddSlotItems(slotIndex, Terrain.MakeBlockValue(GVPistonBlock.Index, 0, Terrain.ExtractData(SetIdToValue(value, StoreItemDataAtUniqueId(blockData, id)))), count);
                    }
                )
            );
            return true;
        }

        public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer) {
            int data = Terrain.ExtractData(value);
            GVPistonMode mode = GVPistonBlock.GetMode(data);
            if (mode == GVPistonMode.Complex) {
                return false;
            }
            int id = GetIdFromValue(value);
            GVPistonData blockData = GetItemData(id, true);
            DialogsManager.ShowDialog(
                componentPlayer.GuiWidget,
                new EditGVPistonDialog(
                    GVPistonBlock.GetMode(data),
                    blockData,
                    delegate {
                        SubsystemTerrain.ChangeCell(x, y, z, Terrain.MakeBlockValue(GVPistonBlock.Index, 0, Terrain.ExtractData(SetIdToValue(value, StoreItemDataAtUniqueId(blockData, id)))));
                        SubsystemGVElectricity subsystemGVElectricity = SubsystemTerrain.Project.FindSubsystem<SubsystemGVElectricity>(true);
                        GVElectricElement electricElement = subsystemGVElectricity.GetGVElectricElement(
                            x,
                            y,
                            z,
                            0,
                            0
                        );
                        if (electricElement != null) {
                            subsystemGVElectricity.QueueGVElectricElementForSimulation(electricElement, subsystemGVElectricity.CircuitStep + 1);
                        }
                    }
                )
            );
            return true;
        }

        public override void OnBlockRemoved(int value, int newValue, int x, int y, int z) => OnBlockRemoved(
            value,
            newValue,
            x,
            y,
            z,
            null
        );

        public override void OnBlockRemoved(int value, int newValue, int x, int y, int z, GVSubterrainSystem system) {
            base.OnBlockRemoved(
                value,
                newValue,
                x,
                y,
                z,
                system
            );
            bool inSubterrain = system != null;
            uint subterrainId = system?.ID ?? 0u;
            int data = Terrain.ExtractData(value);
            GVPoint3 point3 = new(x, y, z, subterrainId);
            switch (Terrain.ExtractContents(value)) {
                case GVPistonBlock.Index: {
                    StopPiston(point3);
                    int face2 = GVPistonBlock.GetFace(data);
                    Point3 point2 = CellFace.FaceToPoint3(face2);
                    int cellValue3 = inSubterrain ? system.Terrain.GetCellValue(x + point2.X, y + point2.Y, z + point2.Z) : m_subsystemTerrain.Terrain.GetCellValue(x + point2.X, y + point2.Y, z + point2.Z);
                    int num4 = Terrain.ExtractContents(cellValue3);
                    int data4 = Terrain.ExtractData(cellValue3);
                    if (num4 == GVPistonHeadBlock.Index
                        && GVPistonHeadBlock.GetFace(data4) == face2) {
                        if (inSubterrain) {
                            system.DestroyCell(
                                0,
                                x + point2.X,
                                y + point2.Y,
                                z + point2.Z,
                                0,
                                false,
                                false
                            );
                        }
                        else {
                            m_subsystemTerrain.DestroyCell(
                                0,
                                x + point2.X,
                                y + point2.Y,
                                z + point2.Z,
                                0,
                                false,
                                false
                            );
                        }
                    }
                    break;
                }
                case GVPistonHeadBlock.Index:
                    if (!m_allowPistonHeadRemove) {
                        int face = GVPistonHeadBlock.GetFace(data);
                        Point3 point = CellFace.FaceToPoint3(face);
                        int cellValue = inSubterrain ? system.Terrain.GetCellValue(x + point.X, y + point.Y, z + point.Z) : m_subsystemTerrain.Terrain.GetCellValue(x + point.X, y + point.Y, z + point.Z);
                        int cellValue2 = inSubterrain ? system.Terrain.GetCellValue(x - point.X, y - point.Y, z - point.Z) : m_subsystemTerrain.Terrain.GetCellValue(x - point.X, y - point.Y, z - point.Z);
                        int num2 = Terrain.ExtractContents(cellValue);
                        int num3 = Terrain.ExtractContents(cellValue2);
                        int data2 = Terrain.ExtractData(cellValue);
                        int data3 = Terrain.ExtractData(cellValue2);
                        if (num2 == GVPistonHeadBlock.Index
                            && GVPistonHeadBlock.GetFace(data2) == face) {
                            if (inSubterrain) {
                                system.DestroyCell(
                                    0,
                                    x + point.X,
                                    y + point.Y,
                                    z + point.Z,
                                    0,
                                    false,
                                    false
                                );
                            }
                            else {
                                m_subsystemTerrain.DestroyCell(
                                    0,
                                    x + point.X,
                                    y + point.Y,
                                    z + point.Z,
                                    0,
                                    false,
                                    false
                                );
                            }
                        }
                        if ((num3 == GVPistonBlock.Index && GVPistonBlock.GetFace(data3) == face)
                            || (num3 == GVPistonHeadBlock.Index && GVPistonHeadBlock.GetFace(data3) == face)) {
                            if (inSubterrain) {
                                system.DestroyCell(
                                    0,
                                    x - point.X,
                                    y - point.Y,
                                    z - point.Z,
                                    0,
                                    false,
                                    false
                                );
                            }
                            else {
                                m_subsystemTerrain.DestroyCell(
                                    0,
                                    x - point.X,
                                    y - point.Y,
                                    z - point.Z,
                                    0,
                                    false,
                                    false
                                );
                            }
                        }
                    }
                    break;
            }
            /*HashSet<GVPoint3> toRemove = [];
            foreach (GVPoint3 action in m_actions.Keys) {
                if (action == point3) {
                    toRemove.Add(action);
                }
            }
            foreach (GVPoint3 p in toRemove) {
                m_actions.Remove(p);
            }*/
        }

        public override void OnChunkDiscarding(TerrainChunk chunk) {
            BoundingBox boundingBox = new(chunk.BoundingBox.Min - new Vector3(16f), chunk.BoundingBox.Max + new Vector3(16f));
            DynamicArray<IMovingBlockSet> dynamicArray = new();
            m_subsystemMovingBlocks.FindMovingBlocks(boundingBox, false, dynamicArray);
            foreach (IMovingBlockSet item in dynamicArray) {
                if (item.Id == IdString) {
                    StopPiston((GVPoint3)item.Tag);
                }
            }
        }

        public override void Load(ValuesDictionary valuesDictionary) {
            base.Load(valuesDictionary);
            m_subsystemTime = Project.FindSubsystem<SubsystemTime>(true);
            m_subsystemAudio = Project.FindSubsystem<SubsystemAudio>(true);
            m_subsystemMovingBlocks = Project.FindSubsystem<SubsystemMovingBlocks>(true);
            m_subsystemGVSubterrain = Project.FindSubsystem<SubsystemGVSubterrain>(true);
            m_subsystemMovingBlocks.Stopped += MovingBlocksStopped;
            m_subsystemMovingBlocks.CollidedWithTerrain += MovingBlocksCollidedWithTerrain;
            if (m_subsystemMovingBlocks.m_blockGeometryGenerator == null) {
                int x = 2;
                x = (int)MathUtils.NextPowerOf2((uint)x);
                m_subsystemMovingBlocks.m_blockGeometryGenerator = new BlockGeometryGenerator(
                    new Terrain(),
                    m_subsystemTerrain,
                    null,
                    Project.FindSubsystem<SubsystemFurnitureBlockBehavior>(true),
                    null,
                    Project.FindSubsystem<SubsystemPalette>(true)
                );
                for (int i = 0; i < x; i++) {
                    for (int j = 0; j < x; j++) {
                        m_subsystemMovingBlocks.m_blockGeometryGenerator.Terrain.AllocateChunk(i, j);
                    }
                }
            }
            m_movingTerrain = m_subsystemMovingBlocks.m_blockGeometryGenerator.Terrain;
        }

        public void ProcessQueuedActions() {
            m_tmpActions.Clear();
            m_tmpActions.AddRange(m_actions);
            foreach ((GVPoint3 key, QueuedAction value) in m_tmpActions) {
                Terrain terrain = m_subsystemGVSubterrain.GetTerrain(key.SubterrainId);
                if (Terrain.ExtractContents(terrain.GetCellValue(key.X, key.Y, key.Z)) != GVPistonBlock.Index) {
                    StopPiston(key);
                    value.Move = null;
                    value.Stop = false;
                }
                else if (value.Stop) {
                    StopPiston(key);
                    value.Stop = false;
                    value.StoppedFrame = Time.FrameIndex;
                }
            }
            foreach ((GVPoint3 key2, QueuedAction value2) in m_tmpActions) {
                if (value2.Move.HasValue
                    && !value2.Stop
                    && Time.FrameIndex != value2.StoppedFrame
                    && m_subsystemMovingBlocks.FindMovingBlocks(IdString, key2) == null) {
                    Terrain terrain = m_subsystemGVSubterrain.GetTerrain(key2.SubterrainId);
                    bool flag = true;
                    for (int i = -1; i <= 1; i++) {
                        for (int j = -1; j <= 1; j++) {
                            TerrainChunk chunkAtCell = terrain.GetChunkAtCell(key2.X + i * 16, key2.Z + j * 16);
                            if (chunkAtCell == null
                                || chunkAtCell.State <= TerrainChunkState.InvalidContents4) {
                                flag = false;
                            }
                        }
                    }
                    if (flag && MovePiston(key2, value2.Move.Value, value2.ComplexData)) {
                        value2.Move = null;
                    }
                }
            }
            foreach (KeyValuePair<GVPoint3, QueuedAction> tmpAction3 in m_tmpActions) {
                QueuedAction value3 = tmpAction3.Value;
                if (!value3.Move.HasValue
                    && !value3.Stop) {
                    m_actions.Remove(tmpAction3.Key);
                }
            }
        }

        public void UpdateMovableBlocks() {
            foreach (IMovingBlockSet movingBlockSet in m_subsystemMovingBlocks.MovingBlockSets) {
                if (movingBlockSet.Id == IdString) {
                    if (movingBlockSet.Tag is not GVPoint3 tag) {
                        if (movingBlockSet.Tag is Point3 point3) {
                            tag = new GVPoint3(point3.X, point3.Y, point3.Z);
                        }
                        else {
                            continue;
                        }
                    }
                    bool inSubterrain = tag.SubterrainId != 0u;
                    GVSubterrainSystem subterrainSystem = inSubterrain ? GVStaticStorage.GVSubterrainSystemDictionary[tag.SubterrainId] : null;
                    int cellValue = (inSubterrain ? subterrainSystem.Terrain : m_subsystemTerrain.Terrain).GetCellValue(tag.X, tag.Y, tag.Z);
                    if (Terrain.ExtractContents(cellValue) == GVPistonBlock.Index) {
                        int data = Terrain.ExtractData(cellValue);
                        int face = GVPistonBlock.GetFace(data);
                        GVPistonMode mode = GVPistonBlock.GetMode(data);
                        bool transparent = false;
                        if (mode == GVPistonMode.Complex) {
                            if (m_complexPistonData.TryGetValue(tag, out GVPistonData pistonData)) {
                                transparent = pistonData.Transparent;
                            }
                        }
                        else {
                            transparent = GetItemData(GetIdFromValue(cellValue), true).Transparent;
                        }
                        Point3 faceDirection = CellFace.FaceToPoint3(face);
                        int min = int.MaxValue;
                        foreach (MovingBlock block in movingBlockSet.Blocks) {
                            min = MathUtils.Min(min, block.Offset.X * faceDirection.X + block.Offset.Y * faceDirection.Y + block.Offset.Z * faceDirection.Z);
                        }
                        Vector3 movingPosition = movingBlockSet.Position;
                        if (inSubterrain) {
                            movingPosition = Vector3.Transform(movingPosition, subterrainSystem.InvertedGlobalTransform);
                        }
                        float num2 = movingPosition.X * faceDirection.X + movingPosition.Y * faceDirection.Y + movingPosition.Z * faceDirection.Z;
                        float num3 = tag.X * faceDirection.X + tag.Y * faceDirection.Y + tag.Z * faceDirection.Z;
                        if (num2 > num3) {
                            if (min + num2 - num3 > 1f) {
                                movingBlockSet.SetBlock(faceDirection * (min - 1), Terrain.MakeBlockValue(GVPistonHeadBlock.Index, 0, GVPistonHeadBlock.SetTransparent(GVPistonHeadBlock.SetFace(GVPistonHeadBlock.SetIsShaft(GVPistonHeadBlock.SetMode(0, mode), true), face), transparent)));
                            }
                        }
                        else if (num2 < num3
                            && min + num2 - num3 <= 0f) {
                            movingBlockSet.SetBlock(faceDirection * min, 0);
                        }
                    }
                }
            }
        }

        public static void GetSpeedAndSmoothness(int pistonSpeed, out float speed, out Vector2 smoothness) {
            switch (pistonSpeed) {
                case 0:
                    speed = 3.5f;
                    smoothness = new Vector2(1.2f, 1.2f);
                    break;
                case 1:
                    speed = 4f;
                    smoothness = new Vector2(0.9f, 0.9f);
                    break;
                case 2:
                    speed = 4.5f;
                    smoothness = new Vector2(0.6f, 0.6f);
                    break;
                case 3:
                    speed = 5f;
                    smoothness = new Vector2(0f, 0.5f);
                    break;
                case 4:
                    speed = 10f;
                    smoothness = new Vector2(0f, 0f);
                    break;
                case 5:
                    speed = 15f;
                    smoothness = new Vector2(0f, 0f);
                    break;
                case 6:
                    speed = 20f;
                    smoothness = new Vector2(0f, 0f);
                    break;
                default:
                    speed = 5f;
                    smoothness = new Vector2(0f, 0.5f);
                    break;
            }
        }

        public bool MovePiston(GVPoint3 position, int length, GVPistonData pistonData = null) {
            Terrain terrain = m_subsystemGVSubterrain.GetTerrain(position.SubterrainId);
            bool inSubterrain = position.SubterrainId != 0;
            GVSubterrainSystem subterrainSystem = inSubterrain ? GVStaticStorage.GVSubterrainSystemDictionary[position.SubterrainId] : null;
            int value = terrain.GetCellValue(position.X, position.Y, position.Z);
            int data = Terrain.ExtractData(value);
            int face = GVPistonBlock.GetFace(data);
            GVPistonMode mode = GVPistonBlock.GetMode(data);
            float trueSpeed;
            Vector2 smoothness;
            bool pulling, strict;
            if (pistonData == null) {
                pistonData = GetItemData(GetIdFromValue(value), true);
                GetSpeedAndSmoothness(pistonData.Speed, out trueSpeed, out smoothness);
                pulling = mode is GVPistonMode.Pulling or GVPistonMode.StrictPulling;
                strict = mode == GVPistonMode.StrictPulling;
            }
            else {
                trueSpeed = pistonData.Speed;
                if (trueSpeed == 0) {
                    return true;
                }
                smoothness = Vector2.Zero;
                pulling = pistonData.Pulling;
                strict = pistonData.Strict;
            }
            if (inSubterrain) {
                Matrix transform = subterrainSystem.GlobalTransform;
                float scale = MathF.Sqrt(transform.M11 * transform.M11 + transform.M12 * transform.M12 + transform.M13 * transform.M13);
                trueSpeed *= scale;
                smoothness *= scale;
            }
            bool transparent = pistonData.Transparent;
            int pullCount = pistonData.PullCount;
            Point3 faceDirection = CellFace.FaceToPoint3(face);
            length = Math.Clamp(length, 0, pistonData.MaxExtension + 1);
            int num = 0;
            m_movingBlocks.Clear();
            int num5 = 0;
            Point3 offset = faceDirection;
            MovingBlock item;
            while (true) {
                int cellValue = terrain.GetCellValue(position.X + offset.X, position.Y + offset.Y, position.Z + offset.Z);
                if (Terrain.ExtractContents(cellValue) != GVPistonHeadBlock.Index
                    || GVPistonHeadBlock.GetFace(Terrain.ExtractData(cellValue)) != face) {
                    break;
                }
                DynamicArray<MovingBlock> movingBlocks = m_movingBlocks;
                item = new MovingBlock { Offset = offset, Value = cellValue };
                movingBlocks.Add(item);
                offset += faceDirection;
                num5++;
                num++;
            }
            Vector3 positionVector3 = new(position.X, position.Y, position.Z);
            Vector3 positionVector3Transformed = inSubterrain ? Vector3.Transform(positionVector3, subterrainSystem.GlobalTransform) : positionVector3;
            //伸长
            if (length > num) {
                DynamicArray<MovingBlock> movingBlocks2 = m_movingBlocks;
                item = new MovingBlock { Offset = Point3.Zero, Value = Terrain.MakeBlockValue(GVPistonHeadBlock.Index, 0, GVPistonHeadBlock.SetTransparent(GVPistonHeadBlock.SetFace(GVPistonHeadBlock.SetMode(GVPistonHeadBlock.SetIsShaft(0, num > 0), mode), face), transparent)) };
                movingBlocks2.Add(item);
                int num3 = 0;
                while (num3 < pullCount + 1) {
                    int cellValue2 = terrain.GetCellValue(position.X + offset.X, position.Y + offset.Y, position.Z + offset.Z);
                    if (!IsBlockMovable(cellValue2, face, position.Y + offset.Y, out bool isEnd)) {
                        break;
                    }
                    DynamicArray<MovingBlock> movingBlocks3 = m_movingBlocks;
                    item = new MovingBlock { Offset = offset, Value = cellValue2 };
                    movingBlocks3.Add(item);
                    num3++;
                    offset += faceDirection;
                    num5++;
                    if (isEnd) {
                        break;
                    }
                }
                if (num3 > 0
                    && strict
                    && num3 < pullCount + 1) {
                    return false;
                }
                if (!IsBlockBlocking(terrain.GetCellValue(position.X + offset.X, position.Y + offset.Y, position.Z + offset.Z), position.Y + offset.Y)) {
                    Point3 p = position.Point + (length - num) * faceDirection;
                    if (num5 > 30) {
                        int count = (num5 + 1) / 16;
                        for (int i = 2; i <= count; i++) {
                            if (Math.Abs(faceDirection.X) > 0) {
                                if (i > m_allocatedX) {
                                    m_movingTerrain.AllocateChunk(i, 0);
                                    m_allocatedX = i;
                                }
                            }
                            else if (Math.Abs(faceDirection.Z) > 0) {
                                if (i > m_allocatedZ) {
                                    m_movingTerrain.AllocateChunk(0, i);
                                    m_allocatedZ = i;
                                }
                            }
                        }
                    }
                    Vector3 startPosition = positionVector3 + 0.01f * new Vector3(faceDirection);
                    Vector3 targetPosition = new(p);
                    if (inSubterrain) {
                        startPosition = Vector3.Transform(startPosition, subterrainSystem.GlobalTransform);
                        targetPosition = Vector3.Transform(targetPosition, subterrainSystem.GlobalTransform);
                    }
                    if (m_subsystemMovingBlocks.AddMovingBlockSet(
                            startPosition,
                            targetPosition,
                            trueSpeed,
                            0f,
                            0f,
                            smoothness,
                            m_movingBlocks,
                            IdString,
                            position,
                            !inSubterrain
                        )
                        != null) {
                        m_allowPistonHeadRemove = true;
                        try {
                            foreach (MovingBlock movingBlock in m_movingBlocks) {
                                if (movingBlock.Offset != Point3.Zero) {
                                    if (inSubterrain) {
                                        subterrainSystem.ChangeCell(position.X + movingBlock.Offset.X, position.Y + movingBlock.Offset.Y, position.Z + movingBlock.Offset.Z, 0);
                                    }
                                    else {
                                        m_subsystemTerrain.ChangeCell(position.X + movingBlock.Offset.X, position.Y + movingBlock.Offset.Y, position.Z + movingBlock.Offset.Z, 0);
                                    }
                                }
                            }
                        }
                        finally {
                            m_allowPistonHeadRemove = false;
                        }
                        if (inSubterrain) {
                            subterrainSystem.ChangeCell(position.X, position.Y, position.Z, Terrain.MakeBlockValue(GVPistonBlock.Index, 0, GVPistonBlock.SetIsExtended(data, true)));
                        }
                        else {
                            m_subsystemTerrain.ChangeCell(position.X, position.Y, position.Z, Terrain.MakeBlockValue(GVPistonBlock.Index, 0, GVPistonBlock.SetIsExtended(data, true)));
                        }
                        m_subsystemAudio.PlaySound(
                            "Audio/Piston",
                            1f,
                            0f,
                            positionVector3Transformed,
                            2f,
                            true
                        );
                    }
                }
                return false;
            }
            //缩回
            if (length < num) {
                if (pulling) {
                    int num4 = 0;
                    for (int i = 0; i < pullCount + 1; i++) {
                        int cellValue3 = terrain.GetCellValue(position.X + offset.X, position.Y + offset.Y, position.Z + offset.Z);
                        if (!IsBlockMovable(cellValue3, face, position.Y + offset.Y, out bool isEnd2)) {
                            break;
                        }
                        DynamicArray<MovingBlock> movingBlocks4 = m_movingBlocks;
                        item = new MovingBlock { Offset = offset, Value = cellValue3 };
                        movingBlocks4.Add(item);
                        offset += faceDirection;
                        num5++;
                        num4++;
                        if (isEnd2) {
                            break;
                        }
                    }
                    if (strict && num4 < pullCount + 1) {
                        return false;
                    }
                }
                float s = length == 0 ? 0.01f : 0f;
                Vector3 targetPosition = positionVector3 + (length - num + s) * new Vector3(faceDirection);
                if (num5 > 30) {
                    int count = (num5 + 1) / 16;
                    for (int i = 2; i <= count; i++) {
                        if (Math.Abs(faceDirection.X) > 0) {
                            if (i > m_allocatedX) {
                                m_movingTerrain.AllocateChunk(i, 0);
                                m_allocatedX = i;
                            }
                        }
                        else if (Math.Abs(faceDirection.Z) > 0) {
                            if (i > m_allocatedZ) {
                                m_movingTerrain.AllocateChunk(0, i);
                                m_allocatedZ = i;
                            }
                        }
                    }
                }
                if (m_subsystemMovingBlocks.AddMovingBlockSet(
                        positionVector3Transformed,
                        inSubterrain ? Vector3.Transform(targetPosition, subterrainSystem.GlobalTransform) : targetPosition,
                        trueSpeed,
                        0f,
                        0f,
                        smoothness,
                        m_movingBlocks,
                        IdString,
                        position,
                        !inSubterrain
                    )
                    != null) {
                    m_allowPistonHeadRemove = true;
                    try {
                        if (inSubterrain) {
                            foreach (MovingBlock movingBlock2 in m_movingBlocks) {
                                subterrainSystem.ChangeCell(position.X + movingBlock2.Offset.X, position.Y + movingBlock2.Offset.Y, position.Z + movingBlock2.Offset.Z, 0);
                            }
                        }
                        else {
                            foreach (MovingBlock movingBlock2 in m_movingBlocks) {
                                m_subsystemTerrain.ChangeCell(position.X + movingBlock2.Offset.X, position.Y + movingBlock2.Offset.Y, position.Z + movingBlock2.Offset.Z, 0);
                            }
                        }
                    }
                    finally {
                        m_allowPistonHeadRemove = false;
                    }
                    m_subsystemAudio.PlaySound(
                        "Audio/Piston",
                        1f,
                        0f,
                        positionVector3Transformed,
                        2f,
                        true
                    );
                }
                return false;
            }
            return true;
        }

        public void StopPiston(GVPoint3 position) {
            IMovingBlockSet movingBlockSet = m_subsystemMovingBlocks.FindMovingBlocks(IdString, position);
            if (movingBlockSet != null) {
                uint subterrainId = position.SubterrainId;
                bool inSubterrain = subterrainId != 0;
                GVSubterrainSystem subterrainSystem = inSubterrain ? GVStaticStorage.GVSubterrainSystemDictionary[subterrainId] : null;
                Terrain terrain = inSubterrain ? subterrainSystem.Terrain : m_subsystemTerrain.Terrain;
                int cellValue = terrain.GetCellValue(position.X, position.Y, position.Z);
                int data = Terrain.ExtractData(cellValue);
                bool flag = Terrain.ExtractContents(cellValue) == GVPistonBlock.Index;
                bool isExtended = false;
                m_subsystemMovingBlocks.RemoveMovingBlockSet(movingBlockSet);
                foreach (MovingBlock block in movingBlockSet.Blocks) {
                    Vector3 movingPosition = movingBlockSet.Position;
                    if (inSubterrain) {
                        movingPosition = Vector3.Transform(movingPosition, subterrainSystem.InvertedGlobalTransform);
                    }
                    int x = Terrain.ToCell(MathF.Round(movingPosition.X)) + block.Offset.X;
                    int y = Terrain.ToCell(MathF.Round(movingPosition.Y)) + block.Offset.Y;
                    int z = Terrain.ToCell(MathF.Round(movingPosition.Z)) + block.Offset.Z;
                    if (!(new Point3(x, y, z) == position.Point)) {
                        int num2 = Terrain.ExtractContents(block.Value);
                        if (flag || num2 != GVPistonHeadBlock.Index) {
                            if (inSubterrain) {
                                subterrainSystem.DestroyCell(
                                    0,
                                    x,
                                    y,
                                    z,
                                    block.Value,
                                    false,
                                    false
                                );
                            }
                            else {
                                m_subsystemTerrain.DestroyCell(
                                    0,
                                    x,
                                    y,
                                    z,
                                    block.Value,
                                    false,
                                    false
                                );
                            }
                            if (num2 == GVPistonHeadBlock.Index) {
                                isExtended = true;
                            }
                        }
                    }
                }
                if (flag) {
                    if (inSubterrain) {
                        subterrainSystem.ChangeCell(position.X, position.Y, position.Z, Terrain.MakeBlockValue(GVPistonBlock.Index, 0, GVPistonBlock.SetIsExtended(data, isExtended)));
                    }
                    else {
                        m_subsystemTerrain.ChangeCell(position.X, position.Y, position.Z, Terrain.MakeBlockValue(GVPistonBlock.Index, 0, GVPistonBlock.SetIsExtended(data, isExtended)));
                    }
                }
            }
        }

        public void MovingBlocksCollidedWithTerrain(IMovingBlockSet movingBlockSet, Point3 p) {
            if (movingBlockSet.Id != IdString) {
                return;
            }
            if (movingBlockSet.Tag is not GVPoint3 point) {
                return;
            }
            if (point.SubterrainId != 0) {
                return;
            }
            int cellValue = m_subsystemTerrain.Terrain.GetCellValue(point.X, point.Y, point.Z);
            if (Terrain.ExtractContents(cellValue) != GVPistonBlock.Index) {
                return;
            }
            Point3 point2 = CellFace.FaceToPoint3(GVPistonBlock.GetFace(Terrain.ExtractData(cellValue)));
            int num = p.X * point2.X + p.Y * point2.Y + p.Z * point2.Z;
            int num2 = point.X * point2.X + point.Y * point2.Y + point.Z * point2.Z;
            if (num > num2) {
                if (IsBlockBlocking(SubsystemTerrain.Terrain.GetCellValue(p.X, p.Y, p.Z), p.Y)) {
                    movingBlockSet.Stop();
                }
                else {
                    SubsystemTerrain.DestroyCell(
                        0,
                        p.X,
                        p.Y,
                        p.Z,
                        0,
                        false,
                        false
                    );
                }
            }
        }

        public void MovingBlocksStopped(IMovingBlockSet movingBlockSet) {
            if (movingBlockSet.Id != IdString
                || movingBlockSet.Tag is not GVPoint3 key) {
                return;
            }
            if (Terrain.ExtractContents(m_subsystemGVSubterrain.GetTerrain(key.SubterrainId).GetCellValue(key.X, key.Y, key.Z)) == GVPistonBlock.Index) {
                if (!m_actions.TryGetValue(key, out QueuedAction value)) {
                    value = new QueuedAction();
                    m_actions.Add(key, value);
                }
                value.Stop = true;
            }
        }

        public static bool IsBlockMovable(int value, int pistonFace, int y, out bool isEnd) {
            isEnd = false;
            if (y is <= 0 or >= 255) {
                return false;
            }
            int num = Terrain.ExtractContents(value);
            int data = Terrain.ExtractData(value);
            switch (num) {
                case CraftingTableBlock.Index:
                case ChestBlock.Index:
                case FurnaceBlock.Index:
                case LitFurnaceBlock.Index:
                case RottenPumpkinBlock.Index:
                case CactusBlock.Index:
                case DiamondBlock.Index:
                case PumpkinBlock.Index:
                case JackOLanternBlock.Index:
                case DispenserBlock.Index: return false;
                case FurnitureBlock.Index: return true;
                case PistonBlock.Index: return !PistonBlock.GetIsExtended(data);
                case GVPistonBlock.Index: return !GVPistonBlock.GetIsExtended(data);
                case PistonHeadBlock.Index:
                case GVPistonHeadBlock.Index:
                case BedrockBlock.Index: return false;
                default: {
                    Block block = BlocksManager.Blocks[num];
                    switch (block) {
                        case DoorBlock _:
                        case TrapdoorBlock _:
                        case BottomSuckerBlock _: return false;
                        case MountedElectricElementBlock elementBlock:
                            isEnd = true;
                            return elementBlock.GetFace(value) == pistonFace;
                        case MountedGVElectricElementBlock GVElementBlock:
                            isEnd = true;
                            return GVElementBlock.GetFace(value) == pistonFace;
                        case LadderBlock _:
                            isEnd = true;
                            return pistonFace == LadderBlock.GetFace(data);
                        case AttachedSignBlock _:
                            isEnd = true;
                            return pistonFace == AttachedSignBlock.GetFace(data);
                        case GVAttachedSignBlock _:
                            isEnd = true;
                            return pistonFace == GVAttachedSignBlock.GetFace(data);
                    }
                    return !block.IsNonDuplicable_(value) && (block.IsCollidable_(value) || (!block.IsDiggingTransparent && block.DestructionDebrisScale == 0f));
                }
            }
        }

        public static bool IsBlockBlocking(int value, int y) {
            if (y is <= 0 or >= 255) {
                return true;
            }
            Block block = BlocksManager.Blocks[Terrain.ExtractContents(value)];
            return block.IsCollidable_(value) || (!block.IsDiggingTransparent && block.DestructionDebrisScale == 0f);
        }
    }
}