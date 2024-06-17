using System.Collections.Generic;
using System.Linq;
using Engine;

namespace Game {
    public class DisplayLedGVElectricElement : RotateableGVElectricElement {
        public SubsystemGVDisplayLedGlow m_subsystemGVDisplayLedGlow;
        public HashSet<GVDisplayPoint> m_glowPoints;
        public Vector3 m_originalPosition;
        public int m_type;
        public bool m_complex;
        public uint m_inputIn;
        public uint m_inputTop;
        public uint m_inputRight;
        public uint m_inputBottom;
        public uint m_inputLeft;

        public static readonly Vector3[] m_upVector3 = [
            Vector3.UnitY,
            Vector3.UnitX,
            -Vector3.UnitY,
            -Vector3.UnitX,
            Vector3.UnitY,
            -Vector3.UnitZ,
            -Vector3.UnitY,
            Vector3.UnitZ,
            Vector3.UnitY,
            -Vector3.UnitX,
            -Vector3.UnitY,
            Vector3.UnitX,
            Vector3.UnitY,
            Vector3.UnitZ,
            -Vector3.UnitY,
            -Vector3.UnitZ,
            -Vector3.UnitZ,
            Vector3.UnitX,
            Vector3.UnitZ,
            -Vector3.UnitX,
            Vector3.UnitZ,
            Vector3.UnitX,
            -Vector3.UnitZ,
            -Vector3.UnitX
        ];

        public DisplayLedGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) => m_subsystemGVDisplayLedGlow = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVDisplayLedGlow>(true);

        public override void OnAdded() {
            GVCellFace cellFace = CellFaces[0];
            int data = Terrain.ExtractData(SubsystemGVElectricity.SubsystemGVSubterrain.GetTerrain(SubterrainId).GetCellValue(cellFace.X, cellFace.Y, cellFace.Z));
            int mountingFace = cellFace.Face;
            int rotation = RotateableMountedGVElectricElementBlock.GetRotation(data);
            m_complex = GVDisplayLedBlock.GetComplex(data);
            m_type = GVDisplayLedBlock.GetType(data);
            m_glowPoints = m_subsystemGVDisplayLedGlow.AddGlowPoints(SubterrainId);
            m_originalPosition = new Vector3(cellFace.X + 0.5f, cellFace.Y + 0.5f, cellFace.Z + 0.5f);
            if (!m_complex) {
                GVDisplayPoint point = new() { Type = m_type, Position = m_originalPosition, Color = Color.White, Complex = false };
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
                point.Rotation = matrix.ToYawPitchRoll();
                m_glowPoints.Add(point);
            }
        }

        public override void OnRemoved() {
            m_subsystemGVDisplayLedGlow.RemoveGlowPoints(m_glowPoints, SubterrainId);
            m_glowPoints.Clear();
            m_glowPoints = null;
        }

        public override bool Simulate() {
            int electricRotation = Rotation;
            m_inputIn = 0u;
            m_inputTop = 0u;
            m_inputRight = 0u;
            m_inputBottom = 0u;
            m_inputLeft = 0u;
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
                            }
                            else if (connectorDirection.Value == GVElectricConnectorDirection.Right) {
                                m_inputRight = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                            }
                            else if (connectorDirection.Value == GVElectricConnectorDirection.Bottom) {
                                m_inputBottom = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                            }
                            else if (connectorDirection.Value == GVElectricConnectorDirection.Left) {
                                m_inputLeft = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                            }
                        }
                        else {
                            m_inputIn = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace) | m_inputIn;
                        }
                    }
                }
            }
            if (m_complex) {
                if (((m_inputBottom >> 28) & 1u) == 0u) {
                    m_glowPoints.Clear();
                }
                GVDisplayPoint glowPoint = new() {
                    Complex = true,
                    Type = m_type,
                    Rotation = Vector3.Zero,
                    Value = m_inputIn,
                    Size = (m_inputTop & 0xFFFFu) / 8f,
                    CustomBit = ((m_inputBottom >> 27) & 1u) == 1u,
                    Color = new Color(m_inputLeft),
                    Position = m_originalPosition + new Vector3((m_inputRight & 0x7FFFu) / (((m_inputRight >> 15) & 1u) == 1u ? -8f : 8f), ((m_inputTop >> 16) & 0x7FFFu) / (((m_inputTop >> 31) & 1u) == 1u ? -8f : 8f), ((m_inputRight >> 16) & 0x7FFFu) / (((m_inputRight >> 31) & 1u) == 1u ? -8f : 8f))
                };
                float yaw = (m_inputBottom & 0xFFu) * 0.017453292f * (((m_inputBottom >> 24) & 1u) == 1u ? -1f : 1f);
                float pitch = ((m_inputBottom >> 8) & 0xFFu) * 0.017453292f * (((m_inputBottom >> 25) & 1u) == 1u ? -1f : 1f);
                float roll = ((m_inputBottom >> 16) & 0xFFu) * 0.017453292f * (((m_inputBottom >> 26) & 1u) == 1u ? -1f : 1f);
                glowPoint.Rotation = new Vector3(yaw, pitch, roll);
                if (glowPoint.isValid()) {
                    m_glowPoints.Add(glowPoint);
                }
            }
            else {
                m_glowPoints.First().Value = m_inputIn;
            }
            return false;
        }
    }
}