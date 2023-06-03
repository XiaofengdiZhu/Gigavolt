using Engine;

namespace Game {
    public class DisplayLedGVElectricElement : RotateableGVElectricElement {
        public SubsystemGVDisplayLedGlow m_subsystemGVDisplayLedGlow;
        public GVDisplayPoint m_glowPoint;
        public Vector3 m_originalPosition;
        public int m_type;
        public bool m_complex;
        public uint m_inputIn;
        public uint m_inputTop;
        public uint m_inputRight;
        public uint m_inputBottom;
        public uint m_inputLeft;
        public static readonly Vector3[] m_upVector3 = { Vector3.UnitY, Vector3.UnitX, -Vector3.UnitY, -Vector3.UnitX, Vector3.UnitY, -Vector3.UnitZ, -Vector3.UnitY, Vector3.UnitZ, Vector3.UnitY, -Vector3.UnitX, -Vector3.UnitY, Vector3.UnitX, Vector3.UnitY, Vector3.UnitZ, -Vector3.UnitY, -Vector3.UnitZ, -Vector3.UnitZ, Vector3.UnitX, Vector3.UnitZ, -Vector3.UnitX, Vector3.UnitZ, Vector3.UnitX, -Vector3.UnitZ, -Vector3.UnitX };

        public DisplayLedGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace) : base(subsystemGVElectricity, cellFace) => m_subsystemGVDisplayLedGlow = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVDisplayLedGlow>(true);

        public override void OnAdded() {
            CellFace cellFace = CellFaces[0];
            int data = Terrain.ExtractData(SubsystemGVElectricity.SubsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z));
            int mountingFace = cellFace.Face;
            int rotation = RotateableMountedGVElectricElementBlock.GetRotation(data);
            m_complex = GVDisplayLedBlock.GetComplex(data);
            m_type = GVDisplayLedBlock.GetType(data);
            m_glowPoint = m_subsystemGVDisplayLedGlow.AddGlowPoint();
            m_originalPosition = new Vector3(cellFace.X + 0.5f, cellFace.Y + 0.5f, cellFace.Z + 0.5f);
            m_glowPoint.Position = m_originalPosition;
            m_glowPoint.Color = Color.White;
            m_glowPoint.Complex = m_complex;
            if (m_complex) {
                m_glowPoint.Type = m_type;
                m_glowPoint.Size = 0;
                m_glowPoint.Rotation = Vector3.Zero;
            }
            else {
                m_glowPoint.Type = m_type;
                Vector3 forward = -CellFace.FaceToVector3(mountingFace);
                if (forward.Y != 0) {
                    forward.Z += 0.0001f;
                }
                Vector3 up = m_upVector3[mountingFace * 4 + rotation];
                Vector3 right = Vector3.Cross(forward, up);
                Matrix matrix = Matrix.Zero;
                matrix.Forward = forward;
                matrix.Up = up;
                matrix.Right = right;
                m_glowPoint.Rotation = matrix.ToYawPitchRoll();
                if (mountingFace > 3) {
                    Log.Information(up + " " + right + " " + m_glowPoint.Rotation);
                }
                m_glowPoint.Color = Color.White;
            }
        }

        public override void OnRemoved() {
            m_subsystemGVDisplayLedGlow.RemoveGlowPoint(m_glowPoint);
        }

        public override bool Simulate() {
            int electricRotation = Rotation;
            uint inputIn = m_inputIn;
            m_inputIn = 0u;
            uint inputTop = m_inputTop;
            uint inputRight = m_inputRight;
            uint inputBottom = m_inputBottom;
            uint inputLeft = m_inputLeft;
            float deltaX = 0f;
            float deltaY = 0f;
            float deltaZ = 0f;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != GVElectricConnectorType.Input) {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, electricRotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue) {
                        if (m_complex) {
                            if (connectorDirection.Value == GVElectricConnectorDirection.In) {
                                m_inputIn = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                            }
                            else if (connectorDirection.Value == GVElectricConnectorDirection.Top) {
                                m_inputTop = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                if (m_inputTop != inputTop) {
                                    m_glowPoint.Size = (m_inputTop & 0xFFFFu) / 8f;
                                    deltaY = ((m_inputTop >> 16) & 0x7FFFu) / (((m_inputTop >> 31) & 1u) == 1u ? -8f : 8f);
                                }
                            }
                            else if (connectorDirection.Value == GVElectricConnectorDirection.Right) {
                                m_inputRight = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                if (m_inputRight != inputRight) {
                                    deltaX = (m_inputRight & 0x7FFFu) / (((m_inputRight >> 15) & 1u) == 1u ? -8f : 8f);
                                    deltaZ = ((m_inputRight >> 16) & 0x7FFFu) / (((m_inputRight >> 31) & 1u) == 1u ? -8f : 8f);
                                }
                            }
                            else if (connectorDirection.Value == GVElectricConnectorDirection.Bottom) {
                                m_inputBottom = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                if (m_inputBottom != inputBottom) {
                                    float yaw = (m_inputBottom & 0xFFu) * 0.017453292f * (((m_inputBottom >> 26) & 1u) == 1u ? -1f : 1f);
                                    float pitch = ((m_inputBottom >> 8) & 0xFFu) * 0.017453292f * (((m_inputBottom >> 25) & 1u) == 1u ? -1f : 1f);
                                    float roll = ((m_inputBottom >> 16) & 0xFFu) * 0.017453292f * (((m_inputBottom >> 24) & 1u) == 1u ? -1f : 1f);
                                    m_glowPoint.Rotation = new Vector3(yaw, pitch, roll);
                                    m_glowPoint.Light = (int)((m_inputBottom >> 28) & 0xFu);
                                    m_glowPoint.CustomBit = m_inputBottom >> 27 == 1u;
                                }
                            }
                            else if (connectorDirection.Value == GVElectricConnectorDirection.Left) {
                                m_inputLeft = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                if (m_inputLeft != inputLeft) {
                                    m_glowPoint.Color = new Color(m_inputLeft);
                                }
                            }
                        }
                        else {
                            m_inputIn = MathUint.Max(connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace), m_inputIn);
                        }
                    }
                }
            }
            if (m_inputIn != inputIn) {
                m_glowPoint.Value = m_inputIn;
            }
            m_glowPoint.Position = m_originalPosition + new Vector3(deltaX, deltaY, deltaZ);
            return false;
        }
    }
}