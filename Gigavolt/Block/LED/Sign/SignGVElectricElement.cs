using System;
using System.Collections.Generic;
using System.Text;
using Engine;

namespace Game {
    public class SignGVElectricElement : MountedGVElectricElement {
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
            Point3 point = CellFaces[0].Point;
            m_originalPosition = new Vector3(point.X + 0.5f, point.Y + 0.5f, point.Z + 0.5f);
            if (m_subsystemGVSignBlockBehavior.m_textsByPoint.TryGetValue(new Point3(point.X, point.Y, point.Z), out m_glowPoint)) {
                m_glowPoint.FloatPosition = m_originalPosition;
                m_glowPoint.FloatColor = Color.White;
                m_glowPoint.FloatSize = 0;
                m_glowPoint.FloatRotation = Vector3.Zero;
                m_glowPoint.FloatLight = 0;
            }
            else {
                m_glowPoint = new GVSignTextData {
                    Point = point,
                    Line = string.Empty,
                    Color = Color.White,
                    Url = string.Empty,
                    FloatPosition = m_originalPosition,
                    FloatColor = Color.White,
                    FloatSize = 0,
                    FloatRotation = Vector3.Zero,
                    FloatLight = 0
                };
                m_subsystemGVSignBlockBehavior.m_textsByPoint[point] = m_glowPoint;
            }
        }

        public override bool Simulate() {
            uint inputIn = m_inputIn;
            m_inputIn = 0u;
            uint inputTop = m_inputTop;
            uint inputRight = m_inputRight;
            uint inputBottom = m_inputBottom;
            uint inputLeft = m_inputLeft;
            float deltaY = 0f;
            float deltaX = 0f;
            float deltaZ = 0f;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != GVElectricConnectorType.Input) {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, 0, connection.ConnectorFace);
                    if (connectorDirection.HasValue) {
                        if (connectorDirection.Value == GVElectricConnectorDirection.In) {
                            m_inputIn = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                        }
                        else if (connectorDirection.Value == GVElectricConnectorDirection.Top) {
                            m_inputTop = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                            if (m_inputTop != inputTop) {
                                m_glowPoint.FloatSize = (m_inputTop & 0xFFFFu) / 8f;
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
                                m_glowPoint.FloatRotation = new Vector3(yaw, pitch, roll);
                                m_glowPoint.FloatLight = (int)((m_inputBottom >> 28) & 0xFu);
                            }
                        }
                        else if (connectorDirection.Value == GVElectricConnectorDirection.Left) {
                            m_inputLeft = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                            if (m_inputLeft != inputLeft) {
                                m_glowPoint.FloatColor = new Color(m_inputLeft);
                            }
                        }
                        else {
                            m_inputIn = MathUint.Max(connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace), m_inputIn);
                        }
                    }
                }
            }
            if (m_inputIn != inputIn
                && m_inputIn > 0) {
                if (GVStaticStorage.GVMBIDDataDictionary.TryGetValue(m_inputIn, out GVMemoryBankData data)
                    && data != null) {
                    string str;
                    try {
                        byte[] byteArray = GVMemoryBankData.Image2Bytes(data.Data, 0, 64);
                        List<byte> byteList = new List<byte>(64);
                        for (int i = 0; i < byteArray.Length; i++) {
                            byte b = byteArray[i];
                            if (b == 0) {
                                break;
                            }
                            byteList.Add(b);
                        }
                        str = new UTF8Encoding(true, true).GetString(byteList.ToArray());
                        m_glowPoint.Line = str;
                        m_glowPoint.TextureLocation = null;
                        m_subsystemGVSignBlockBehavior.m_lastUpdatePositions.Clear();
                    }
                    catch (Exception e) {
                        str = "错误！详见日志";
                        Log.Error(e);
                    }
                }
            }
            uint customBit = (m_inputBottom >> 27) & 1;
            if (customBit == 1
                && ((inputBottom >> 27) & 1) == 0) {
                foreach (ComponentPlayer componentPlayer in SubsystemGVElectricity.Project.FindSubsystem<SubsystemPlayers>(true).ComponentPlayers) {
                    Color color = m_glowPoint.Color == Color.Black ? Color.White : m_glowPoint.Color;
                    color *= 255f / MathUtils.Max(color.R, color.G, color.B);
                    componentPlayer.ComponentGui.DisplaySmallMessage(m_glowPoint.Line, color, true, true);
                }
            }
            m_glowPoint.FloatPosition = m_originalPosition + new Vector3(deltaX, deltaY, deltaZ);
            return false;
        }
    }
}