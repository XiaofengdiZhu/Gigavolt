using System;
using Engine;

namespace Game {
    public class MoreTwoInTwoOutGVElectricElement : RotateableGVElectricElement {
        public int m_type;
        public uint m_output;
        public uint m_overflow;

        public MoreTwoInTwoOutGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, int value, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) => m_type = GVMoreTwoInTwoOutBlock.GetType(Terrain.ExtractData(value));

        public override uint GetOutputVoltage(int face) {
            GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, Rotation, face);
            if (connectorDirection.HasValue) {
                if (connectorDirection.Value == GVElectricConnectorDirection.Top
                    || connectorDirection.Value == GVElectricConnectorDirection.In) {
                    return m_output;
                }
                if (connectorDirection.Value == GVElectricConnectorDirection.Bottom) {
                    return m_overflow;
                }
            }
            return 0u;
        }

        public override bool Simulate() {
            try {
                uint output;
                uint overflow;
                uint leftInput = 0u;
                uint rightInput = 0u;
                int rotation = Rotation;
                foreach (GVElectricConnection connection in Connections) {
                    if (connection.ConnectorType != GVElectricConnectorType.Output
                        && connection.NeighborConnectorType != 0) {
                        GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
                        if (connectorDirection.HasValue) {
                            if (connectorDirection == GVElectricConnectorDirection.Right) {
                                rightInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                            }
                            else if (connectorDirection == GVElectricConnectorDirection.Left) {
                                leftInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                            }
                        }
                    }
                }
                switch (m_type) {
                    case 1: {
                        output = leftInput - rightInput;
                        overflow = leftInput < rightInput ? 1u : 0u;
                    }
                        break;
                    case 2: {
                        ulong result = leftInput * (ulong)rightInput;
                        output = (uint)result;
                        overflow = (uint)(result >> 32);
                    }
                        break;
                    case 3: {
                        output = rightInput == 0u ? 0u : leftInput / rightInput;
                        overflow = 0u;
                    }
                        break;
                    case 4: {
                        output = rightInput == 0u ? 0u : leftInput % rightInput;
                        overflow = 0u;
                    }
                        break;
                    case 5: {
                        output = leftInput == rightInput ? uint.MaxValue : 0u;
                        overflow = 0u;
                    }
                        break;
                    case 6: {
                        output = leftInput > rightInput ? uint.MaxValue : 0u;
                        overflow = 0u;
                    }
                        break;
                    case 7: {
                        output = leftInput >= rightInput ? uint.MaxValue : 0u;
                        overflow = 0u;
                    }
                        break;
                    case 8: {
                        output = leftInput < rightInput ? uint.MaxValue : 0u;
                        overflow = 0u;
                    }
                        break;
                    case 9: {
                        output = leftInput <= rightInput ? uint.MaxValue : 0u;
                        overflow = 0u;
                    }
                        break;
                    case 10: {
                        output = MathUint.Max(leftInput, rightInput);
                        overflow = 0u;
                    }
                        break;
                    case 11: {
                        output = MathUint.Min(leftInput, rightInput);
                        overflow = 0u;
                    }
                        break;
                    case 12: {
                        ulong result = (ulong)leftInput << (int)rightInput;
                        output = (uint)result;
                        overflow = (uint)(result >> 32);
                    }
                        break;
                    case 13: {
                        output = leftInput >> (int)rightInput;
                        overflow = (uint)((((ulong)leftInput << 32) >> (int)rightInput) & uint.MaxValue);
                    }
                        break;
                    case 14: {
                        double result = Math.Pow(leftInput, rightInput);
                        output = (uint)result;
                        overflow = (uint)((ulong)result >> 32);
                    }
                        break;
                    case 15: {
                        double result = Math.Log(leftInput, rightInput);
                        output = (uint)result;
                        overflow = 0u;
                    }
                        break;
                    default: {
                        ulong result = leftInput + (ulong)rightInput;
                        output = (uint)result;
                        overflow = (uint)(result >> 32);
                    }
                        break;
                }
                bool flag = false;
                if (output != m_output) {
                    m_output = output;
                    flag = true;
                }
                if (overflow != m_overflow) {
                    m_overflow = overflow;
                    flag = true;
                }
                return flag;
            }
            catch (Exception ex) {
                Log.Error(ex);
            }
            return false;
        }
    }
}