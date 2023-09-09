using System;
using System.Collections.Generic;
using System.Linq;

namespace Game {
    public class MemoryBanksOperatorGVElectricElement : RotateableGVElectricElement {
        readonly Terrain m_terrain;
        uint m_rightInput;
        uint m_leftInput;
        uint m_topInput;
        uint m_bottomInput;
        uint m_inInput;

        public MemoryBanksOperatorGVElectricElement(SubsystemGVElectricity subsystemGVElectric, CellFace cellFace) : base(subsystemGVElectric, cellFace) => m_terrain = SubsystemGVElectricity.SubsystemTerrain.Terrain;

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
                && m_bottomInput < 8u
                && m_leftInput > 0u
                && m_topInput > 0u
                && GVStaticStorage.GVMBIDDataDictionary.TryGetValue(m_leftInput, out GVArrayData leftData)
                && GVStaticStorage.GVMBIDDataDictionary.TryGetValue(m_topInput, out GVArrayData topData)) {
                if (m_rightInput > 0u
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
                else if (m_bottomInput == 1u) {
                    if (leftData == null) {
                        return false;
                    }
                    GVListMemoryBankData arrayData = leftData as GVListMemoryBankData;
                    if (arrayData == null) {
                        topData.Image2Data(leftData.GetImage());
                    }
                    else {
                        uint[] uintArray = arrayData.GetUintArray();
                        topData.UintArray2Data(uintArray, (int)arrayData.m_width, (int)Math.Ceiling(uintArray.Length / (double)arrayData.m_width));
                    }
                }
            }
            return false;
        }
    }
}