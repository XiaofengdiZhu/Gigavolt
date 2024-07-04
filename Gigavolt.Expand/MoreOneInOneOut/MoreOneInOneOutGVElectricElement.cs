using System;

namespace Game {
    public class MoreOneInOneOutGVElectricElement : RotateableGVElectricElement {
        public readonly int m_type;
        public uint m_output;

        public MoreOneInOneOutGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, int value, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) => m_type = GVMoreOneInOneOutBlock.GetType(Terrain.ExtractData(value));

        public override uint GetOutputVoltage(int face) {
            GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, Rotation, face);
            if (connectorDirection.HasValue) {
                if (connectorDirection.Value == GVElectricConnectorDirection.Top
                    || connectorDirection.Value == GVElectricConnectorDirection.In) {
                    return m_output;
                }
            }
            return 0u;
        }

        public override bool Simulate() {
            uint output;
            uint input = 0u;
            int rotation = Rotation;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, rotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue) {
                        if (connectorDirection == GVElectricConnectorDirection.Bottom) {
                            input |= connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                        }
                        else if (connectorDirection == GVElectricConnectorDirection.In) {
                            input |= connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                        }
                    }
                }
            }
            if (m_type < 14) {
                double radius = (input >> 31 == 1 ? -1 : 1) * (((input >> 16) & 0x7fffu) + (double)(input & 0xffffu) / 0xffff);
                double middleOutput;
                switch (m_type) {
                    case 1:
                        middleOutput = Math.Cos(radius);
                        break;
                    case 2:
                        middleOutput = Math.Tan(radius);
                        break;
                    case 3:
                        middleOutput = 1 / Math.Tan(radius);
                        break;
                    case 4:
                        middleOutput = 1 / Math.Cos(radius);
                        break;
                    case 5:
                        middleOutput = 1 / Math.Sin(radius);
                        break;
                    case 6:
                        middleOutput = Math.Asin(radius);
                        break;
                    case 7:
                        middleOutput = Math.Acos(radius);
                        break;
                    case 8:
                        middleOutput = Math.Atan(radius);
                        break;
                    case 9:
                        middleOutput = Math.Sinh(radius);
                        break;
                    case 10:
                        middleOutput = Math.Cosh(radius);
                        break;
                    case 11:
                        middleOutput = Math.Tanh(radius);
                        break;
                    case 12:
                        middleOutput = radius * Math.PI / 180;
                        break;
                    case 13:
                        middleOutput = radius * 180 / Math.PI;
                        break;
                    default:
                        middleOutput = Math.Sin(radius);
                        break;
                }
                output = Double2Uint(middleOutput);
            }
            else if (m_type == 14) {
                output = input ^ (1u << 31);
            }
            else if (m_type == 15) {
                output = input >> 31 == 0u ? ~input + 1 : ~(input - 1);
            }
            else {
                output = input;
            }
            bool flag = false;
            if (output != m_output) {
                m_output = output;
                flag = true;
            }
            return flag;
        }

        public static uint Double2Uint(double num) => ((num < 0 ? 1u : 0u) << 31) | (((uint)Math.Truncate(Math.Abs(num)) & 0x7fff) << 16) | (uint)Math.Round(num % 1 * 0xffff);
    }
}