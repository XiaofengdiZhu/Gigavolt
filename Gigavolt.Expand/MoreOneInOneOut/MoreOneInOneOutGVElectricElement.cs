using System;

namespace Game {
    public class MoreOneInOneOutGVElectricElement : RotateableGVElectricElement {
        public int m_type;
        public uint m_output;

        public MoreOneInOneOutGVElectricElement(SubsystemGVElectricity subsystemGVElectric, CellFace cellFace, int value) : base(subsystemGVElectric, cellFace) => m_type = GVMoreOneInOneOutBlock.GetType(Terrain.ExtractData(value));

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
                double midleOutput;
                switch (m_type) {
                    case 1:
                        midleOutput = Math.Cos(radius);
                        break;
                    case 2:
                        midleOutput = Math.Tan(radius);
                        break;
                    case 3:
                        midleOutput = 1 / Math.Tan(radius);
                        break;
                    case 4:
                        midleOutput = 1 / Math.Cos(radius);
                        break;
                    case 5:
                        midleOutput = 1 / Math.Sin(radius);
                        break;
                    case 6:
                        midleOutput = Math.Asin(radius);
                        break;
                    case 7:
                        midleOutput = Math.Acos(radius);
                        break;
                    case 8:
                        midleOutput = Math.Atan(radius);
                        break;
                    case 9:
                        midleOutput = Math.Sinh(radius);
                        break;
                    case 10:
                        midleOutput = Math.Cosh(radius);
                        break;
                    case 11:
                        midleOutput = Math.Tanh(radius);
                        break;
                    case 12:
                        midleOutput = radius * Math.PI / 180;
                        break;
                    case 13:
                        midleOutput = radius * 180 / Math.PI;
                        break;
                    default:
                        midleOutput = Math.Sin(radius);
                        break;
                }
                output = Double2Uint(midleOutput);
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

        public static uint Double2Uint(double num) => ((num < 0 ? 1u : 0u) << 31) | (((uint)Math.Truncate(num) & 0x7fff) << 16) | (uint)Math.Round(num % 1 * 0xffff);
    }
}