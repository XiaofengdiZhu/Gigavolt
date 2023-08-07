using System;
using System.Collections.Generic;
using System.Linq;

namespace Game {
    public class VolatileListMemoryBankGVElectricElement : RotateableGVElectricElement {
        public SubsystemGVVolatileListMemoryBankBlockBehavior m_SubsystemGVMemoryBankBlockBehavior;
        public SubsystemGameInfo m_subsystemGameInfo;

        public uint m_voltage;
        public uint m_lastBottomInput;

        public VolatileListMemoryBankGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace) : base(subsystemGVElectricity, cellFace) {
            m_SubsystemGVMemoryBankBlockBehavior = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVVolatileListMemoryBankBlockBehavior>(true);
            m_subsystemGameInfo = subsystemGVElectricity.Project.FindSubsystem<SubsystemGameInfo>(true);
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
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
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
            GVVolatileListMemoryBankData memoryBankData = m_SubsystemGVMemoryBankBlockBehavior.GetBlockData(CellFaces[0].Point);
            if (memoryBankData == null) {
                memoryBankData = new GVVolatileListMemoryBankData(GVStaticStorage.GetUniqueGVMBID(), new List<uint>(256));
                m_SubsystemGVMemoryBankBlockBehavior.SetBlockData(CellFaces[0].Point, memoryBankData);
            }
            if (bottomConnected) {
                if (bottomInput == 0u
                    || bottomInput > 2u) {
                    m_lastBottomInput = bottomInput;
                    m_voltage = 0u;
                }
                if (bottomInput != m_lastBottomInput) {
                    m_lastBottomInput = bottomInput;
                    uint smallIndex = MathUint.Min(leftInput, rightInput);
                    uint bigIndex = MathUint.Max(leftInput, rightInput);
                    if (leftInput == 0u) {
                        smallIndex = bigIndex;
                    }
                    int smallIndexInt = MathUint.ToInt(smallIndex);
                    int bigIndexInt = MathUint.ToInt(bigIndex);
                    int leftInputInt = MathUint.ToInt(leftInput);
                    int rightInputInt = MathUint.ToInt(rightInput);
                    int inInputInt = MathUint.ToInt(inInput);
                    List<uint> data = memoryBankData.Data;
                    switch (bottomInput) {
                        case 1u:
                            m_voltage = memoryBankData.Read(rightInput);
                            break;
                        case 2u:
                            for (uint i = bigIndex; i >= smallIndex; i--) {
                                memoryBankData.Write(i, inInput);
                            }
                            break;
                        case 3u:
                            if (smallIndexInt < memoryBankData.Data.Count) {
                                data.InsertRange(smallIndexInt, Enumerable.Repeat(inInput, bigIndexInt - smallIndexInt + 1));
                            }
                            else {
                                data.Capacity = bigIndexInt + 1;
                                data.AddRange(Enumerable.Repeat(0u, smallIndexInt - data.Count));
                                data.AddRange(Enumerable.Repeat(inInput, bigIndexInt - smallIndexInt + 1));
                            }
                            memoryBankData.m_updateTime = DateTime.Now;
                            memoryBankData.m_dataChanged = true;
                            break;
                        case 4u:
                            m_voltage = memoryBankData.Read(rightInput);
                            if (rightInputInt < data.Count) {
                                data.RemoveAt(rightInputInt);
                                memoryBankData.m_updateTime = DateTime.Now;
                                memoryBankData.m_dataChanged = true;
                            }
                            break;
                        case 5u:
                            if (smallIndexInt < data.Count) {
                                data.RemoveRange(smallIndexInt, Math.Min(bigIndexInt - smallIndexInt + 1, data.Count - smallIndexInt));
                                memoryBankData.m_updateTime = DateTime.Now;
                                memoryBankData.m_dataChanged = true;
                            }
                            break;
                        case 6u:
                            if (smallIndexInt < data.Count) {
                                m_voltage = (uint)data.IndexOf(inInput, smallIndexInt, Math.Min(bigIndexInt - smallIndexInt + 1, data.Count - smallIndexInt));
                            }
                            break;
                        case 7u:
                            if (smallIndexInt < data.Count) {
                                m_voltage = (uint)data.LastIndexOf(inInput, smallIndexInt, Math.Min(bigIndexInt - smallIndexInt + 1, data.Count - smallIndexInt));
                            }
                            break;
                        case 8u:
                            int oldCount = data.Count;
                            if (smallIndexInt < data.Count) {
                                memoryBankData.Data = data.Where((u, i) => i >= smallIndex && i <= bigIndexInt && u == inInput).ToList();
                                m_voltage = (uint)(memoryBankData.Data.Count - oldCount);
                            }
                            break;
                        case 9u:
                            m_voltage = (uint)data.Where((u, i) => i >= smallIndex && i <= bigIndexInt && u == inInput).Count();
                            break;
                        case 10u:
                            memoryBankData.Write(leftInput, memoryBankData.Read(rightInput));
                            break;
                        case 11u: {
                            uint temp = memoryBankData.Read(rightInput);
                            if (leftInputInt < memoryBankData.Data.Count) {
                                data.Insert(leftInputInt, temp);
                            }
                            else {
                                data.Capacity = leftInputInt + 1;
                                data.AddRange(Enumerable.Repeat(0u, leftInputInt - data.Count));
                                data.Add(temp);
                            }
                            memoryBankData.m_updateTime = DateTime.Now;
                            memoryBankData.m_dataChanged = true;
                            break;
                        }
                        case 12u:
                            if (smallIndexInt < data.Count) {
                                data.Reverse(smallIndexInt, Math.Min(bigIndexInt - smallIndexInt + 1, data.Count - smallIndexInt));
                                memoryBankData.m_updateTime = DateTime.Now;
                                memoryBankData.m_dataChanged = true;
                            }
                            break;
                        case 13u:
                            if (smallIndexInt < data.Count) {
                                data.Sort(smallIndexInt, Math.Min(bigIndexInt - smallIndexInt + 1, data.Count - smallIndexInt), new UintComparer());
                            }
                            break;
                        case 14u:
                            if (smallIndexInt < data.Count) {
                                data.Sort(smallIndexInt, Math.Min(bigIndexInt - smallIndexInt + 1, data.Count - smallIndexInt), new UintDecComparer());
                            }
                            break;
                        case 15u:
                            m_voltage = (uint)data.Count;
                            break;
                        case 16u:
                        case 32u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, (uint)(memoryBankData.Read(i) + (ulong)inInput));
                            }
                            break;
                        case 17u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, memoryBankData.Read(i) - inInput);
                            }
                            break;
                        case 33u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, inInput - memoryBankData.Read(i));
                            }
                            break;
                        case 18u:
                        case 34u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, (uint)(memoryBankData.Read(i) * (ulong)inInput));
                            }
                            break;
                        case 19u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, inInput == 0u ? 0u : memoryBankData.Read(i) / inInput);
                            }
                            break;
                        case 35u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                uint temp = memoryBankData.Read(i);
                                memoryBankData.Write(i, temp == 0u ? 0u : inInput / temp);
                            }
                            break;
                        case 20u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, inInput == 0u ? 0u : memoryBankData.Read(i) % inInput);
                            }
                            break;
                        case 36u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                uint temp = memoryBankData.Read(i);
                                memoryBankData.Write(i, temp == 0u ? 0u : inInput % temp);
                            }
                            break;
                        case 21u:
                        case 37u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, memoryBankData.Read(i) == inInput ? uint.MaxValue : 0u);
                            }
                            break;
                        case 22u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, memoryBankData.Read(i) > inInput ? uint.MaxValue : 0u);
                            }
                            break;
                        case 38u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, inInput > memoryBankData.Read(i) ? uint.MaxValue : 0u);
                            }
                            break;
                        case 23u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, memoryBankData.Read(i) >= inInput ? uint.MaxValue : 0u);
                            }
                            break;
                        case 39u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, inInput >= memoryBankData.Read(i) ? uint.MaxValue : 0u);
                            }
                            break;
                        case 24u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, memoryBankData.Read(i) < inInput ? uint.MaxValue : 0u);
                            }
                            break;
                        case 40u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, inInput < memoryBankData.Read(i) ? uint.MaxValue : 0u);
                            }
                            break;
                        case 25u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, memoryBankData.Read(i) <= inInput ? uint.MaxValue : 0u);
                            }
                            break;
                        case 41u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, inInput <= memoryBankData.Read(i) ? uint.MaxValue : 0u);
                            }
                            break;
                        case 26u:
                        case 42u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, MathUint.Max(memoryBankData.Read(i), inInput));
                            }
                            break;
                        case 27u:
                        case 43u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, MathUint.Min(memoryBankData.Read(i), inInput));
                            }
                            break;
                        case 28u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, memoryBankData.Read(i) << inInputInt);
                            }
                            break;
                        case 44u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, inInput << MathUint.ToInt(memoryBankData.Read(i)));
                            }
                            break;
                        case 29u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, memoryBankData.Read(i) >> inInputInt);
                            }
                            break;
                        case 45u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, inInput >> MathUint.ToInt(memoryBankData.Read(i)));
                            }
                            break;
                        case 30u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, (uint)Math.Pow(memoryBankData.Read(i), inInput));
                            }
                            break;
                        case 46u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, (uint)Math.Pow(inInput, memoryBankData.Read(i)));
                            }
                            break;
                        case 31u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, (uint)Math.Log(memoryBankData.Read(i), inInput));
                            }
                            break;
                        case 47u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, (uint)Math.Log(inInput, memoryBankData.Read(i)));
                            }
                            break;
                        case 48u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, MoreOneInOneOutGVElectricElement.Double2Uint(Math.Sin(Uint2Double(memoryBankData.Read(i)))));
                            }
                            break;
                        case 49u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, MoreOneInOneOutGVElectricElement.Double2Uint(Math.Cos(Uint2Double(memoryBankData.Read(i)))));
                            }
                            break;
                        case 50u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, MoreOneInOneOutGVElectricElement.Double2Uint(Math.Tan(Uint2Double(memoryBankData.Read(i)))));
                            }
                            break;
                        case 51u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, MoreOneInOneOutGVElectricElement.Double2Uint(1 / Math.Tan(Uint2Double(memoryBankData.Read(i)))));
                            }
                            break;
                        case 52u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, MoreOneInOneOutGVElectricElement.Double2Uint(1 / Math.Cos(Uint2Double(memoryBankData.Read(i)))));
                            }
                            break;
                        case 53u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, MoreOneInOneOutGVElectricElement.Double2Uint(1 / Math.Sin(Uint2Double(memoryBankData.Read(i)))));
                            }
                            break;
                        case 54u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, MoreOneInOneOutGVElectricElement.Double2Uint(Math.Asin(Uint2Double(memoryBankData.Read(i)))));
                            }
                            break;
                        case 55u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, MoreOneInOneOutGVElectricElement.Double2Uint(Math.Acos(Uint2Double(memoryBankData.Read(i)))));
                            }
                            break;
                        case 56u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, MoreOneInOneOutGVElectricElement.Double2Uint(Math.Atan(Uint2Double(memoryBankData.Read(i)))));
                            }
                            break;
                        case 57u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, MoreOneInOneOutGVElectricElement.Double2Uint(Math.Sinh(Uint2Double(memoryBankData.Read(i)))));
                            }
                            break;
                        case 58u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, MoreOneInOneOutGVElectricElement.Double2Uint(Math.Cosh(Uint2Double(memoryBankData.Read(i)))));
                            }
                            break;
                        case 59u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, MoreOneInOneOutGVElectricElement.Double2Uint(Math.Tanh(Uint2Double(memoryBankData.Read(i)))));
                            }
                            break;
                        case 60u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, MoreOneInOneOutGVElectricElement.Double2Uint(Uint2Double(memoryBankData.Read(i)) * Math.PI / 180));
                            }
                            break;
                        case 61u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, MoreOneInOneOutGVElectricElement.Double2Uint(Uint2Double(memoryBankData.Read(i)) * 180 / Math.PI));
                            }
                            break;
                        case 62u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                memoryBankData.Write(i, memoryBankData.Read(i) ^ (1u << 31));
                            }
                            break;
                        case 63u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                uint input = memoryBankData.Read(i);
                                memoryBankData.Write(i, input >> 31 == 0u ? ~input + 1 : ~(input - 1));
                            }
                            break;
                        case 64u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                uint input = memoryBankData.Read(i);
                                memoryBankData.Write(i, ~memoryBankData.Read(i));
                            }
                            break;
                        case 65u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                uint input = memoryBankData.Read(i);
                                memoryBankData.Write(i, memoryBankData.Read(i) + 1);
                            }
                            break;
                        case 66u:
                            for (uint i = smallIndex; i < bigIndex; i++) {
                                uint input = memoryBankData.Read(i);
                                memoryBankData.Write(i, memoryBankData.Read(i) - 1);
                            }
                            break;
                        case 256u:
                            memoryBankData.m_width = inInput;
                            break;
                        case 257u:
                            memoryBankData.m_height = inInput;
                            break;
                        case 258u:
                            memoryBankData.m_offset = inInput;
                            break;
                    }
                }
            }
            else {
                m_voltage = memoryBankData.Read(leftInput);
            }
            if (!hasInput) {
                m_voltage = memoryBankData.m_ID;
            }
            return m_voltage != voltage;
        }

        public static double Uint2Double(uint num) => (num >> 31 == 1 ? -1 : 1) * (((num >> 16) & 0x7fffu) + (double)(num & 0xffffu) / 0xffff);
    }
}