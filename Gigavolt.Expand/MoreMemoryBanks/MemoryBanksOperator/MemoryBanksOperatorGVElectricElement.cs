using System;
using System.Collections.Generic;
using System.Linq;
using Engine;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;

namespace Game {
    public class MemoryBanksOperatorGVElectricElement : RotateableGVElectricElement {
        uint m_rightInput;
        uint m_leftInput;
        uint m_topInput;
        uint m_bottomInput;
        uint m_inInput;

        public MemoryBanksOperatorGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) { }

        public override uint GetOutputVoltage(int face) => 0u;

        public override bool Simulate() {
            m_rightInput = 0u;
            m_leftInput = 0u;
            m_topInput = 0u;
            m_inInput = 0u;
            uint bottomInput = m_bottomInput;
            m_bottomInput = 0u;
            int rotation = Rotation;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue) {
                        switch (connectorDirection) {
                            case GVElectricConnectorDirection.Right:
                                m_rightInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                break;
                            case GVElectricConnectorDirection.Left:
                                m_leftInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                break;
                            case GVElectricConnectorDirection.Top:
                                m_topInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                break;
                            case GVElectricConnectorDirection.Bottom:
                                m_bottomInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                break;
                            case GVElectricConnectorDirection.In:
                                m_inInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                break;
                        }
                    }
                }
            }
            if (m_bottomInput != bottomInput
                && m_bottomInput > 0u
                && m_leftInput > 0u
                && m_topInput > 0u
                && GVStaticStorage.GVMBIDDataDictionary.TryGetValue(m_leftInput, out GVArrayData leftData)
                && GVStaticStorage.GVMBIDDataDictionary.TryGetValue(m_topInput, out GVArrayData topData)) {
                if (m_bottomInput == 1u) {
                    //合并
                    if (GVStaticStorage.GVMBIDDataDictionary.TryGetValue(m_rightInput, out GVArrayData rightData)) {
                        uint[] leftArray = leftData.GetUintArray();
                        uint[] rightArray = rightData.GetUintArray();
                        if (leftArray == null
                            || rightArray == null) {
                            return false;
                        }
                        uint[] newData = leftArray.Concat(rightArray).ToArray();
                        topData.UintArray2Data(newData, newData.Length, 1);
                    }
                    //复制
                    else {
                        uint oldTargetId = topData.ID;
                        if ((leftData is GVMemoryBankData && topData is GVMemoryBankData)
                            || (leftData is GVListMemoryBankData && topData is GVListMemoryBankData)
                            || (leftData is GVFourDimensionalMemoryBankData && topData is GVFourDimensionalMemoryBankData)) {
                            leftData.Copy(oldTargetId);
                            topData.Dispose();
                        }
                    }
                }
                //批量复制
                else if (m_bottomInput == 0x100u) {
                    Point3 rangeAbs = new((int)(m_rightInput & 0xFFu), (int)((m_rightInput >> 8) & 0xFFu), (int)((m_rightInput >> 16) & 0xFFu));
                    if (rangeAbs.X == 0
                        || rangeAbs.Y == 0
                        || rangeAbs.Z == 0) {
                        return false;
                    }
                    Point3 startPosition = CellFaces[0].Point + new Point3((int)(m_inInput & 0xFFu) * (m_inInput >> 24 == 1u ? -1 : 1), (int)((m_inInput >> 8) & 0xFFu) * (m_inInput >> 25 == 1u ? -1 : 1), (int)((m_inInput >> 16) & 0xFFu) * (m_inInput >> 26 == 1u ? -1 : 1));
                    bool xDirection = ((m_inInput >> 24) & 1u) == 1u;
                    bool yDirection = ((m_inInput >> 25) & 1u) == 1u;
                    bool zDirection = ((m_inInput >> 26) & 1u) == 1u;
                    Point3 range = new(rangeAbs.X * (xDirection ? -1 : 1), rangeAbs.Y * (yDirection ? -1 : 1), rangeAbs.Z * (zDirection ? -1 : 1));
                    Point3 endPosition = startPosition + range;
                    bool storeIds = ((m_inInput >> 27) & 1u) == 0u;
                    bool overwrite = ((m_inInput >> 28) & 1u) == 1u;
                    bool xFixed = false;
                    bool yFixed = false;
                    bool zFixed = false;
                    Dictionary<int, uint> storeIdsDictionary = new();
                    if (storeIds) {
                        switch (topData) {
                            case GVListMemoryBankData: {
                                int count = 0;
                                if (range.X is 1 or -1) {
                                    xFixed = true;
                                    count++;
                                }
                                if (range.Y is 1 or -1) {
                                    yFixed = true;
                                    count++;
                                }
                                if (range.Z is 1 or -1) {
                                    zFixed = true;
                                    count++;
                                }
                                if (count < 2) {
                                    storeIds = false;
                                }
                                break;
                            }
                            case GVMemoryBankData: {
                                if (rangeAbs.X == 1) {
                                    xFixed = true;
                                }
                                if (rangeAbs.Y == 1) {
                                    yFixed = true;
                                }
                                if (rangeAbs.Z == 1) {
                                    zFixed = true;
                                }
                                if (!(xFixed || yFixed || zFixed)) {
                                    storeIds = false;
                                }
                                break;
                            }
                        }
                    }
                    int sourceContents = leftData switch {
                        GVVolatileMemoryBankData => GVBlocksManager.GetBlockIndex<GVVolatileMemoryBankBlock>(),
                        GVMemoryBankData => GVBlocksManager.GetBlockIndex<GVMemoryBankBlock>(),
                        GVVolatileListMemoryBankData => GVBlocksManager.GetBlockIndex<GVVolatileListMemoryBankBlock>(),
                        GVListMemoryBankData => GVBlocksManager.GetBlockIndex<GVListMemoryBankBlock>(),
                        GVVolatileFourDimensionalMemoryBankData => GVBlocksManager.GetBlockIndex<GVVolatileFourDimensionalMemoryBankBlock>(),
                        GVFourDimensionalMemoryBankData => GVBlocksManager.GetBlockIndex<GVFourDimensionalMemoryBankBlock>(),
                        _ => -1
                    };
                    GVSubterrainSystem subterrain = SubterrainId == 0 ? null : GVStaticStorage.GVSubterrainSystemDictionary[SubterrainId];
                    Terrain terrain = subterrain == null ? SubsystemGVElectricity.SubsystemTerrain.Terrain : subterrain.Terrain;
                    for (int x = startPosition.X; x != endPosition.X; x += xDirection ? -1 : 1) {
                        for (int y = Math.Max(startPosition.Y, 0); y != Math.Min(endPosition.Y, 255); y += yDirection ? -1 : 1) {
                            for (int z = startPosition.Z; z != endPosition.Z; z += zDirection ? -1 : 1) {
                                int value = terrain.GetCellValueFast(x, y, z);
                                int contents = Terrain.ExtractContents(value);
                                if (contents != sourceContents) {
                                    continue;
                                }
                                uint newId;
                                int newValue;
                                switch (BlocksManager.Blocks[sourceContents]) {
                                    case GVVolatileMemoryBankBlock: {
                                        SubsystemGVVolatileMemoryBankBlockBehavior subsystem = SubsystemGVElectricity.Project.FindSubsystem<SubsystemGVVolatileMemoryBankBlockBehavior>(true);
                                        if (!overwrite
                                            && subsystem.GetIdFromValue(value) > 0) {
                                            continue;
                                        }
                                        int oldBlockId = subsystem.GetIdFromValue(value);
                                        if (oldBlockId > 0) {
                                            newId = subsystem.GetItemData(oldBlockId).ID;
                                            if (newId == m_leftInput
                                                || newId == m_topInput) {
                                                continue;
                                            }
                                            leftData.Copy(newId);
                                            newValue = value;
                                        }
                                        else {
                                            GVVolatileMemoryBankData newData = (GVVolatileMemoryBankData)leftData.Copy();
                                            newId = newData.ID;
                                            newValue = subsystem.SetIdToValue(value, subsystem.StoreItemDataAtUniqueId(newData));
                                        }
                                        break;
                                    }
                                    case GVMemoryBankBlock: {
                                        SubsystemGVMemoryBankBlockBehavior subsystem = SubsystemGVElectricity.Project.FindSubsystem<SubsystemGVMemoryBankBlockBehavior>(true);
                                        if (!overwrite
                                            && subsystem.GetIdFromValue(value) > 0) {
                                            continue;
                                        }
                                        int oldBlockId = subsystem.GetIdFromValue(value);
                                        if (oldBlockId > 0) {
                                            newId = subsystem.GetItemData(oldBlockId).ID;
                                            if (newId == m_leftInput
                                                || newId == m_topInput) {
                                                continue;
                                            }
                                            leftData.Copy(newId);
                                            newValue = value;
                                        }
                                        else {
                                            GVMemoryBankData newData = (GVMemoryBankData)leftData.Copy();
                                            newId = newData.ID;
                                            newValue = subsystem.SetIdToValue(value, subsystem.StoreItemDataAtUniqueId(newData));
                                        }
                                        break;
                                    }
                                    case GVVolatileListMemoryBankBlock: {
                                        SubsystemGVVolatileListMemoryBankBlockBehavior subsystem = SubsystemGVElectricity.Project.FindSubsystem<SubsystemGVVolatileListMemoryBankBlockBehavior>(true);
                                        if (!overwrite
                                            && subsystem.GetIdFromValue(value) > 0) {
                                            continue;
                                        }
                                        int oldBlockId = subsystem.GetIdFromValue(value);
                                        if (oldBlockId > 0) {
                                            newId = subsystem.GetItemData(oldBlockId).ID;
                                            if (newId == m_leftInput
                                                || newId == m_topInput) {
                                                continue;
                                            }
                                            leftData.Copy(newId);
                                            newValue = value;
                                        }
                                        else {
                                            GVVolatileListMemoryBankData newData = (GVVolatileListMemoryBankData)leftData.Copy();
                                            newId = newData.ID;
                                            newValue = subsystem.SetIdToValue(value, subsystem.StoreItemDataAtUniqueId(newData));
                                        }
                                        break;
                                    }
                                    case GVListMemoryBankBlock: {
                                        SubsystemGVListMemoryBankBlockBehavior subsystem = SubsystemGVElectricity.Project.FindSubsystem<SubsystemGVListMemoryBankBlockBehavior>(true);
                                        if (!overwrite
                                            && subsystem.GetIdFromValue(value) > 0) {
                                            continue;
                                        }
                                        int oldBlockId = subsystem.GetIdFromValue(value);
                                        if (oldBlockId > 0) {
                                            newId = subsystem.GetItemData(oldBlockId).ID;
                                            if (newId == m_leftInput
                                                || newId == m_topInput) {
                                                continue;
                                            }
                                            leftData.Copy(newId);
                                            newValue = value;
                                        }
                                        else {
                                            GVListMemoryBankData newData = (GVListMemoryBankData)leftData.Copy();
                                            newId = newData.ID;
                                            newValue = subsystem.SetIdToValue(value, subsystem.StoreItemDataAtUniqueId(newData));
                                        }
                                        break;
                                    }
                                    case GVVolatileFourDimensionalMemoryBankBlock: {
                                        SubsystemGVVolatileFourDimensionalMemoryBankBlockBehavior subsystem = SubsystemGVElectricity.Project.FindSubsystem<SubsystemGVVolatileFourDimensionalMemoryBankBlockBehavior>(true);
                                        if (!overwrite
                                            && subsystem.GetIdFromValue(value) > 0) {
                                            continue;
                                        }
                                        int oldBlockId = subsystem.GetIdFromValue(value);
                                        if (oldBlockId > 0) {
                                            newId = subsystem.GetItemData(oldBlockId).ID;
                                            if (newId == m_leftInput
                                                || newId == m_topInput) {
                                                continue;
                                            }
                                            leftData.Copy(newId);
                                            newValue = value;
                                        }
                                        else {
                                            GVVolatileFourDimensionalMemoryBankData newData = (GVVolatileFourDimensionalMemoryBankData)leftData.Copy();
                                            newId = newData.ID;
                                            newValue = subsystem.SetIdToValue(value, subsystem.StoreItemDataAtUniqueId(newData));
                                        }
                                        break;
                                    }
                                    case GVFourDimensionalMemoryBankBlock: {
                                        SubsystemGVFourDimensionalMemoryBankBlockBehavior subsystem = SubsystemGVElectricity.Project.FindSubsystem<SubsystemGVFourDimensionalMemoryBankBlockBehavior>(true);
                                        if (!overwrite
                                            && subsystem.GetIdFromValue(value) > 0) {
                                            continue;
                                        }
                                        int oldBlockId = subsystem.GetIdFromValue(value);
                                        if (oldBlockId > 0) {
                                            newId = subsystem.GetItemData(oldBlockId).ID;
                                            if (newId == m_leftInput
                                                || newId == m_topInput) {
                                                continue;
                                            }
                                            leftData.Copy(newId);
                                            newValue = value;
                                        }
                                        else {
                                            GVFourDimensionalMemoryBankData newData = (GVFourDimensionalMemoryBankData)leftData.Copy();
                                            newId = newData.ID;
                                            newValue = subsystem.SetIdToValue(value, subsystem.StoreItemDataAtUniqueId(newData));
                                        }
                                        break;
                                    }
                                    default: continue;
                                }
                                if (subterrain == null) {
                                    SubsystemGVElectricity.SubsystemTerrain.ChangeCell(x, y, z, newValue);
                                }
                                else {
                                    subterrain.ChangeCell(x, y, z, newValue);
                                }
                                if (storeIds) {
                                    int xy = rangeAbs.X * rangeAbs.Y;
                                    switch (topData) {
                                        case GVFourDimensionalMemoryBankData: {
                                            storeIdsDictionary.Add(Math.Abs(x - startPosition.X) + Math.Abs(y - startPosition.Y) * rangeAbs.X + Math.Abs(z - startPosition.Z) * xy, newId);
                                            break;
                                        }
                                        case GVMemoryBankData: {
                                            if (xFixed) {
                                                storeIdsDictionary.Add(Math.Abs(y - startPosition.Y) + Math.Abs(z - startPosition.Z) * rangeAbs.Y, newId);
                                            }
                                            else if (yFixed) {
                                                storeIdsDictionary.Add(Math.Abs(x - startPosition.X) + Math.Abs(z - startPosition.Z) * rangeAbs.X, newId);
                                            }
                                            else if (zFixed) {
                                                storeIdsDictionary.Add(Math.Abs(x - startPosition.X) + Math.Abs(y - startPosition.Y) * rangeAbs.X, newId);
                                            }
                                            break;
                                        }
                                        case GVListMemoryBankData: {
                                            if (xFixed && yFixed) {
                                                storeIdsDictionary.Add(Math.Abs(z - startPosition.Z), newId);
                                            }
                                            else if (xFixed && zFixed) {
                                                storeIdsDictionary.Add(Math.Abs(y - startPosition.Y), newId);
                                            }
                                            else if (yFixed && zFixed) {
                                                storeIdsDictionary.Add(Math.Abs(x - startPosition.X), newId);
                                            }
                                            break;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (storeIds) {
                        switch (topData) {
                            case GVFourDimensionalMemoryBankData fd: {
                                fd.m_xLength = rangeAbs.X;
                                fd.m_yLength = rangeAbs.Y;
                                fd.m_xyProduct = fd.m_xLength * fd.m_yLength;
                                fd.m_zLength = rangeAbs.Z;
                                fd.m_xyzProduct = fd.m_xyProduct * fd.m_zLength;
                                fd.m_wLength = 1;
                                fd.m_totalLength = fd.m_xyProduct;
                                Image<Rgba32> image = new(GVFourDimensionalMemoryBankData.DefaultImageConfiguration, rangeAbs.X, rangeAbs.Y);
                                image.Metadata.GetWebpMetadata().FileFormat = WebpFileFormatType.Lossless;
                                while (image.Frames.Count < fd.m_zLength) {
                                    image.Frames.AddFrame(image.Frames.RootFrame);
                                }
                                foreach ((int index, uint id) in storeIdsDictionary) {
                                    int z = index / fd.m_xyProduct;
                                    int zRemainder = index % fd.m_xyProduct;
                                    int y = zRemainder / fd.m_xLength;
                                    int x = zRemainder % fd.m_xLength;
                                    image.Frames[z][x, y] = new Rgba32(id);
                                }
                                fd.Data = new Dictionary<int, Image<Rgba32>> { { 0, image } };
                                break;
                            }
                            case GVMemoryBankData md: {
                                if (xFixed) {
                                    md.m_width = (uint)rangeAbs.Y;
                                    md.m_height = (uint)rangeAbs.Z;
                                }
                                else if (yFixed) {
                                    md.m_width = (uint)rangeAbs.X;
                                    md.m_height = (uint)rangeAbs.Z;
                                }
                                else if (zFixed) {
                                    md.m_width = (uint)rangeAbs.X;
                                    md.m_height = (uint)rangeAbs.Y;
                                }
                                uint[] image = new uint[rangeAbs.X * rangeAbs.Y * rangeAbs.Z];
                                foreach ((int index, uint id) in storeIdsDictionary) {
                                    image[index] = id;
                                }
                                md.Data = image;
                                break;
                            }
                            case GVListMemoryBankData ld: {
                                ld.m_width = 0u;
                                ld.m_height = 0u;
                                ld.m_offset = 0u;
                                List<uint> list = new(rangeAbs.X * rangeAbs.Y * rangeAbs.Z);
                                foreach ((int index, uint id) in storeIdsDictionary) {
                                    if (index < list.Count) {
                                        list[index] = id;
                                    }
                                    else {
                                        list.AddRange(Enumerable.Repeat(0u, index - list.Count));
                                        list.Add(id);
                                    }
                                }
                                ld.Data = list;
                                break;
                            }
                        }
                    }
                }
                else if (m_rightInput > 0u
                    && GVStaticStorage.GVMBIDDataDictionary.TryGetValue(m_rightInput, out GVArrayData rightData)) {
                    uint[] leftArray = leftData.GetUintArray();
                    uint[] rightArray = rightData.GetUintArray();
                    if (leftArray == null
                        || rightArray == null) {
                        return false;
                    }
                    switch (m_bottomInput) {
                        case 1: {
                            uint[] newData = leftArray.Concat(rightArray).ToArray();
                            topData.UintArray2Data(newData, newData.Length, 1);
                            break;
                        }
                        case 2: {
                            List<uint> leftList = leftArray.ToList();
                            int m_inInput_int = (int)m_inInput;
                            if (m_inInput_int < leftList.Count) {
                                leftList.InsertRange((int)m_inInput, rightArray);
                            }
                            else {
                                leftList.Capacity = m_inInput_int + rightArray.Length;
                                leftList.AddRange(Enumerable.Repeat(0u, m_inInput_int - leftArray.Length));
                                leftList.AddRange(rightArray);
                            }
                            topData.UintArray2Data(leftList.ToArray(), leftList.Count, 1);
                            break;
                        }
                        case 3: {
                            int m_inInput_int = (int)m_inInput;
                            if (m_inInput_int + rightArray.Length < leftArray.Length) {
                                for (int i = 0; i < rightArray.Length; i++) {
                                    leftArray[m_inInput_int + i] = rightArray[i];
                                }
                                topData.UintArray2Data(leftArray, leftArray.Length, 1);
                            }
                            else if (m_inInput_int < leftArray.Length) {
                                List<uint> leftList = leftArray.ToList();
                                leftList.RemoveRange(m_inInput_int, leftArray.Length - m_inInput_int);
                                leftList.AddRange(rightArray);
                                topData.UintArray2Data(leftList.ToArray(), leftList.Count, 1);
                            }
                            else {
                                List<uint> leftList = leftArray.ToList();
                                leftList.Capacity = m_inInput_int + rightArray.Length;
                                leftList.AddRange(Enumerable.Repeat(0u, m_inInput_int - leftArray.Length));
                                leftList.AddRange(rightArray);
                                topData.UintArray2Data(leftList.ToArray(), leftList.Count, 1);
                            }
                            break;
                        }
                        case 4: {
                            List<uint> leftList = leftArray.ToList();
                            topData.UintArray2Data(leftList.Where(item => rightArray.Contains(item)).ToArray());
                            break;
                        }
                        case 5: {
                            List<uint> leftList = leftArray.ToList();
                            topData.UintArray2Data(leftList.Where(item => rightArray.Contains(item)).Distinct().ToArray());
                            break;
                        }
                        case 6: {
                            List<uint> leftList = leftArray.ToList();
                            topData.UintArray2Data(leftList.Where(item => !rightArray.Contains(item)).ToArray());
                            break;
                        }
                        case 7: {
                            List<uint> leftList = leftArray.ToList();
                            topData.UintArray2Data(leftList.Where(item => !rightArray.Contains(item)).Distinct().ToArray());
                            break;
                        }
                    }
                }
            }
            return false;
        }
    }
}