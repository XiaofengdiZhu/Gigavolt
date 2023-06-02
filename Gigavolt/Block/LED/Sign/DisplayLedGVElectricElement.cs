using Engine;

namespace Game {
    public class SignGVElectricElement : RotateableGVElectricElement {
        public SubsystemGVSignBlockBehavior m_subsystemGVSignBlockBehavior;
        public GVSignTextData m_glowPoint;
        public Vector3 m_originalPosition;
        public uint m_inputIn;
        public uint m_inputTop;
        public uint m_inputRight;
        public uint m_inputBottom;
        public uint m_inputLeft;

        public SignGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace) : base(subsystemGVElectricity, cellFace) => m_subsystemGVSignBlockBehavior = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVSignBlockBehavior>(true);

        public override void OnAdded() {
            CellFace cellFace = CellFaces[0];
            int data = Terrain.ExtractData(SubsystemGVElectricity.SubsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z));
            int mountingFace = cellFace.Face;
            if (m_subsystemGVSignBlockBehavior.m_textsByPoint.TryGetValue(new Point3(cellFace.X, cellFace.Y, cellFace.Z), out m_glowPoint)) {
                m_originalPosition = new Vector3(cellFace.X + 0.5f, cellFace.Y + 0.5f, cellFace.Z + 0.5f);
                m_glowPoint.FloatPosition = m_originalPosition;
                m_glowPoint.Color = Color.White;
                m_glowPoint.FloatSize = 0;
                m_glowPoint.FloatRotation = Vector3.Zero;
            }
        }

        public override bool Simulate() {
            int electricRotation = Rotation;
            uint inputIn = m_inputIn;
            m_inputIn = 0u;
            uint inputTop = m_inputTop;
            uint inputRight = m_inputRight;
            uint inputBottom = m_inputBottom;
            uint inputLeft = m_inputLeft;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != GVElectricConnectorType.Input) {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, electricRotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue) {
                        if (connectorDirection.Value == GVElectricConnectorDirection.In) {
                            m_inputIn = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                        }
                        else if (connectorDirection.Value == GVElectricConnectorDirection.Top) {
                            m_inputTop = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                            if (m_inputTop != inputTop) {
                                m_glowPoint.FloatSize = (m_inputTop & 0xFFFFu) / 8f;
                                m_glowPoint.FloatPosition = m_originalPosition;
                                m_glowPoint.FloatPosition.Y += ((m_inputTop >> 16) & 0x7FFFu) / (((m_inputTop >> 31) & 1u) == 1u ? -8f : 8f);
                            }
                        }
                        else if (connectorDirection.Value == GVElectricConnectorDirection.Right) {
                            m_inputRight = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                            if (m_inputRight != inputRight) {
                                m_glowPoint.FloatPosition = m_originalPosition;
                                m_glowPoint.FloatPosition.X += (m_inputRight & 0x7FFFu) / (((m_inputRight >> 15) & 1u) == 1u ? -8f : 8f);
                                m_glowPoint.FloatPosition.Z += ((m_inputRight >> 16) & 0x7FFFu) / (((m_inputRight >> 31) & 1u) == 1u ? -8f : 8f);
                            }
                        }
                        else if (connectorDirection.Value == GVElectricConnectorDirection.Bottom) {
                            m_inputBottom = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                            if (m_inputBottom != inputBottom) {
                                float yaw = (m_inputBottom & 0xFFu) * 0.017453292f * (((m_inputBottom >> 26) & 1u) == 1u ? -1f : 1f);
                                float pitch = ((m_inputBottom >> 8) & 0xFFu) * 0.017453292f * (((m_inputBottom >> 25) & 1u) == 1u ? -1f : 1f);
                                float roll = ((m_inputBottom >> 16) & 0xFFu) * 0.017453292f * (((m_inputBottom >> 24) & 1u) == 1u ? -1f : 1f);
                                m_glowPoint.FloatRotation = new Vector3(yaw, pitch, roll);
                                m_glowPoint.Light = (int)((m_inputBottom >> 28) & 0xFu);
                            }
                        }
                        else if (connectorDirection.Value == GVElectricConnectorDirection.Left) {
                            m_inputLeft = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                            if (m_inputLeft != inputLeft) {
                                m_glowPoint.Color = new Color(m_inputLeft);
                            }
                        }
                        else {
                            m_inputIn = MathUint.Max(connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace), m_inputIn);
                        }
                    }
                }
            }
            if (m_inputIn != inputIn) {
                //m_glowPoint.Value = m_inputIn;
            }
            return false;
        }
    }
}