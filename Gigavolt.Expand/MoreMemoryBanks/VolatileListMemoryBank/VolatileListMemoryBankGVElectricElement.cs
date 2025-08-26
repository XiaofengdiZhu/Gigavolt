using System;
using System.Collections.Generic;
using System.Linq;

namespace Game {
    public class VolatileListMemoryBankGVElectricElement : RotateableGVElectricElement {
        public readonly SubsystemGVVolatileListMemoryBankBlockBehavior m_SubsystemGVMemoryBankBlockBehavior;

        public readonly GVVolatileListMemoryBankData m_data;
        public uint m_voltage;
        public uint m_lastBottomInput;

        public VolatileListMemoryBankGVElectricElement(SubsystemGVElectricity subsystemGVElectricity,
            GVCellFace cellFace,
            int value,
            uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) {
            m_SubsystemGVMemoryBankBlockBehavior = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVVolatileListMemoryBankBlockBehavior>(true);
            m_data = m_SubsystemGVMemoryBankBlockBehavior.GetItemData(m_SubsystemGVMemoryBankBlockBehavior.GetIdFromValue(value));
        }

        public override void OnAdded() { }

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override bool Simulate() {
            uint voltage = m_voltage;
            bool bottomConnected = false;
            uint inInput = 0u;
            uint rightInput = 0u;
            uint leftInput = 0u;
            uint bottomInput = 0u;
            bool hasInput = false;
            int rotation = Rotation;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    GVElectricConnectorDirection? connectorDirection =
                        SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue) {
                        switch (connectorDirection) {
                            case GVElectricConnectorDirection.Right:
                                rightInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                hasInput = true;
                                break;
                            case GVElectricConnectorDirection.Left:
                                leftInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                hasInput = true;
                                break;
                            case GVElectricConnectorDirection.Bottom: {
                                bottomInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                bottomConnected = true;
                                hasInput = true;
                                break;
                            }
                            case GVElectricConnectorDirection.In:
                                inInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                hasInput = true;
                                break;
                        }
                    }
                }
            }
            if (bottomConnected) {
                if (bottomInput != m_lastBottomInput) {
                    m_lastBottomInput = bottomInput;
                    if (bottomInput > 0u) {
                        m_voltage = 0u;
                        uint smallIndex = MathUint.Min(leftInput, rightInput);
                        uint bigIndex = MathUint.Max(leftInput, rightInput);
                        if (leftInput == 0u) {
                            smallIndex = bigIndex;
                        }
                        int smallIndexInt = MathUint.ToIntWithClamp(smallIndex);
                        int bigIndexInt = MathUint.ToIntWithClamp(bigIndex);
                        int leftInputInt = MathUint.ToIntWithClamp(leftInput);
                        int rightInputInt = MathUint.ToIntWithClamp(rightInput);
                        int inInputInt = MathUint.ToIntWithClamp(inInput);
                        List<uint> data = m_data.Data;
                        switch (bottomInput) {
                            case 1u: m_voltage = m_data.Read(rightInput); break;
                            case 2u:
                                for (uint i = bigIndex; i >= smallIndex; i--) {
                                    m_data.Write(i, inInput);
                                }
                                break;
                            case 3u:
                                if (smallIndexInt < m_data.Data.Count) {
                                    data.InsertRange(smallIndexInt, Enumerable.Repeat(inInput, bigIndexInt - smallIndexInt + 1));
                                }
                                else {
                                    data.Capacity = bigIndexInt + 1;
                                    data.AddRange(Enumerable.Repeat(0u, smallIndexInt - data.Count));
                                    data.AddRange(Enumerable.Repeat(inInput, bigIndexInt - smallIndexInt + 1));
                                }
                                m_data.m_updateTime = DateTime.Now;
                                m_data.m_dataChanged = true;
                                break;
                            case 4u:
                                m_voltage = m_data.Read(rightInput);
                                if (rightInputInt < data.Count) {
                                    data.RemoveAt(rightInputInt);
                                    m_data.m_updateTime = DateTime.Now;
                                    m_data.m_dataChanged = true;
                                }
                                break;
                            case 5u:
                                if (smallIndexInt < data.Count) {
                                    data.RemoveRange(smallIndexInt, Math.Min(bigIndexInt - smallIndexInt + 1, data.Count - smallIndexInt));
                                    m_data.m_updateTime = DateTime.Now;
                                    m_data.m_dataChanged = true;
                                }
                                break;
                            case 6u:
                                if (smallIndexInt < data.Count) {
                                    m_voltage = (uint)data.IndexOf(
                                        inInput,
                                        smallIndexInt,
                                        Math.Min(bigIndexInt - smallIndexInt + 1, data.Count - smallIndexInt)
                                    );
                                }
                                break;
                            case 7u:
                                if (smallIndexInt < data.Count) {
                                    m_voltage = (uint)data.LastIndexOf(
                                        inInput,
                                        smallIndexInt,
                                        Math.Min(bigIndexInt - smallIndexInt + 1, data.Count - smallIndexInt)
                                    );
                                }
                                break;
                            case 8u:
                                int oldCount = data.Count;
                                if (smallIndexInt < data.Count) {
                                    m_data.Data = data.Where((u, i) => i >= smallIndex && i <= bigIndexInt && u == inInput).ToList();
                                    m_voltage = (uint)(m_data.Data.Count - oldCount);
                                }
                                break;
                            case 9u: m_voltage = (uint)data.Where((u, i) => i >= smallIndex && i <= bigIndexInt && u == inInput).Count(); break;
                            case 10u: m_data.Write(leftInput, m_data.Read(rightInput)); break;
                            case 11u: {
                                uint temp = m_data.Read(rightInput);
                                if (leftInputInt < m_data.Data.Count) {
                                    data.Insert(leftInputInt, temp);
                                }
                                else {
                                    data.Capacity = leftInputInt + 1;
                                    data.AddRange(Enumerable.Repeat(0u, leftInputInt - data.Count));
                                    data.Add(temp);
                                }
                                m_data.m_updateTime = DateTime.Now;
                                m_data.m_dataChanged = true;
                                break;
                            }
                            case 12u:
                                if (smallIndexInt < data.Count) {
                                    data.Reverse(smallIndexInt, Math.Min(bigIndexInt - smallIndexInt + 1, data.Count - smallIndexInt));
                                    m_data.m_updateTime = DateTime.Now;
                                    m_data.m_dataChanged = true;
                                }
                                break;
                            case 13u:
                                if (smallIndexInt < data.Count) {
                                    data.Sort(
                                        smallIndexInt,
                                        Math.Min(bigIndexInt - smallIndexInt + 1, data.Count - smallIndexInt),
                                        new UintComparer()
                                    );
                                }
                                break;
                            case 14u:
                                if (smallIndexInt < data.Count) {
                                    data.Sort(
                                        smallIndexInt,
                                        Math.Min(bigIndexInt - smallIndexInt + 1, data.Count - smallIndexInt),
                                        new UintDecComparer()
                                    );
                                }
                                break;
                            case 15u: m_voltage = (uint)data.Count; break;
                            case 16u:
                            case 32u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, (uint)(m_data.Read(i) + (ulong)inInput));
                                }
                                break;
                            case 17u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, m_data.Read(i) - inInput);
                                }
                                break;
                            case 33u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, inInput - m_data.Read(i));
                                }
                                break;
                            case 18u:
                            case 34u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, (uint)(m_data.Read(i) * (ulong)inInput));
                                }
                                break;
                            case 19u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, inInput == 0u ? 0u : m_data.Read(i) / inInput);
                                }
                                break;
                            case 35u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    uint temp = m_data.Read(i);
                                    m_data.Write(i, temp == 0u ? 0u : inInput / temp);
                                }
                                break;
                            case 20u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, inInput == 0u ? 0u : m_data.Read(i) % inInput);
                                }
                                break;
                            case 36u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    uint temp = m_data.Read(i);
                                    m_data.Write(i, temp == 0u ? 0u : inInput % temp);
                                }
                                break;
                            case 21u:
                            case 37u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, m_data.Read(i) == inInput ? uint.MaxValue : 0u);
                                }
                                break;
                            case 22u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, m_data.Read(i) > inInput ? uint.MaxValue : 0u);
                                }
                                break;
                            case 38u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, inInput > m_data.Read(i) ? uint.MaxValue : 0u);
                                }
                                break;
                            case 23u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, m_data.Read(i) >= inInput ? uint.MaxValue : 0u);
                                }
                                break;
                            case 39u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, inInput >= m_data.Read(i) ? uint.MaxValue : 0u);
                                }
                                break;
                            case 24u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, m_data.Read(i) < inInput ? uint.MaxValue : 0u);
                                }
                                break;
                            case 40u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, inInput < m_data.Read(i) ? uint.MaxValue : 0u);
                                }
                                break;
                            case 25u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, m_data.Read(i) <= inInput ? uint.MaxValue : 0u);
                                }
                                break;
                            case 41u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, inInput <= m_data.Read(i) ? uint.MaxValue : 0u);
                                }
                                break;
                            case 26u:
                            case 42u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, MathUint.Max(m_data.Read(i), inInput));
                                }
                                break;
                            case 27u:
                            case 43u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, MathUint.Min(m_data.Read(i), inInput));
                                }
                                break;
                            case 28u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, m_data.Read(i) << inInputInt);
                                }
                                break;
                            case 44u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, inInput << MathUint.ToIntWithClamp(m_data.Read(i)));
                                }
                                break;
                            case 29u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, m_data.Read(i) >> inInputInt);
                                }
                                break;
                            case 45u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, inInput >> MathUint.ToIntWithClamp(m_data.Read(i)));
                                }
                                break;
                            case 30u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, (uint)Math.Pow(m_data.Read(i), inInput));
                                }
                                break;
                            case 46u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, (uint)Math.Pow(inInput, m_data.Read(i)));
                                }
                                break;
                            case 31u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, (uint)Math.Log(m_data.Read(i), inInput));
                                }
                                break;
                            case 47u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, (uint)Math.Log(inInput, m_data.Read(i)));
                                }
                                break;
                            case 48u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, MoreOneInOneOutGVElectricElement.Double2Uint(Math.Sin(Uint2Double(m_data.Read(i)))));
                                }
                                break;
                            case 49u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, MoreOneInOneOutGVElectricElement.Double2Uint(Math.Cos(Uint2Double(m_data.Read(i)))));
                                }
                                break;
                            case 50u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, MoreOneInOneOutGVElectricElement.Double2Uint(Math.Tan(Uint2Double(m_data.Read(i)))));
                                }
                                break;
                            case 51u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, MoreOneInOneOutGVElectricElement.Double2Uint(1 / Math.Tan(Uint2Double(m_data.Read(i)))));
                                }
                                break;
                            case 52u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, MoreOneInOneOutGVElectricElement.Double2Uint(1 / Math.Cos(Uint2Double(m_data.Read(i)))));
                                }
                                break;
                            case 53u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, MoreOneInOneOutGVElectricElement.Double2Uint(1 / Math.Sin(Uint2Double(m_data.Read(i)))));
                                }
                                break;
                            case 54u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, MoreOneInOneOutGVElectricElement.Double2Uint(Math.Asin(Uint2Double(m_data.Read(i)))));
                                }
                                break;
                            case 55u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, MoreOneInOneOutGVElectricElement.Double2Uint(Math.Acos(Uint2Double(m_data.Read(i)))));
                                }
                                break;
                            case 56u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, MoreOneInOneOutGVElectricElement.Double2Uint(Math.Atan(Uint2Double(m_data.Read(i)))));
                                }
                                break;
                            case 57u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, MoreOneInOneOutGVElectricElement.Double2Uint(Math.Sinh(Uint2Double(m_data.Read(i)))));
                                }
                                break;
                            case 58u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, MoreOneInOneOutGVElectricElement.Double2Uint(Math.Cosh(Uint2Double(m_data.Read(i)))));
                                }
                                break;
                            case 59u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, MoreOneInOneOutGVElectricElement.Double2Uint(Math.Tanh(Uint2Double(m_data.Read(i)))));
                                }
                                break;
                            case 60u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, MoreOneInOneOutGVElectricElement.Double2Uint(Uint2Double(m_data.Read(i)) * Math.PI / 180));
                                }
                                break;
                            case 61u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, MoreOneInOneOutGVElectricElement.Double2Uint(Uint2Double(m_data.Read(i)) * 180 / Math.PI));
                                }
                                break;
                            case 62u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, m_data.Read(i) ^ (1u << 31));
                                }
                                break;
                            case 63u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    uint input = m_data.Read(i);
                                    m_data.Write(i, input >> 31 == 0u ? ~input + 1 : ~(input - 1));
                                }
                                break;
                            case 64u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, ~m_data.Read(i));
                                }
                                break;
                            case 65u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, m_data.Read(i) + 1);
                                }
                                break;
                            case 66u:
                                for (uint i = smallIndex; i < bigIndex; i++) {
                                    m_data.Write(i, m_data.Read(i) - 1);
                                }
                                break;
                            case 256u:
                                m_data.m_width = inInput;
                                m_data.m_updateTime = DateTime.Now;
                                break;
                            case 257u:
                                m_data.m_height = inInput;
                                m_data.m_updateTime = DateTime.Now;
                                break;
                            case 258u:
                                m_data.m_offset = inInput;
                                m_data.m_updateTime = DateTime.Now;
                                break;
                            case 272u: m_voltage = m_data.m_width; break;
                            case 273u: m_voltage = m_data.m_height; break;
                            case 274u: m_voltage = m_data.m_offset; break;
                        }
                    }
                }
            }
            else {
                m_voltage = m_data.Read(rightInput);
            }
            if (!hasInput) {
                m_voltage = m_data.ID;
            }
            return m_voltage != voltage;
        }

        public static double Uint2Double(uint num) => (num >> 31 == 1 ? -1 : 1) * (((num >> 16) & 0x7fffu) + (double)(num & 0xffffu) / 0xffff);
    }
}