using System;
using Engine;

namespace Game {
    public class TractorBeamGVElectricElement : RotateableGVElectricElement {
        public SubsystemGVTractorBeamBlockBehavior m_subsystemBlockBehavior;
        public GVSubterrainSystem m_subterrainSystem;
        public uint m_inputTop;
        public uint m_inputRight;
        public uint m_inputBottom;
        public uint m_inputLeft;
        public Vector3 m_targetOffset;
        public Point3 m_targetPoint;

        public TractorBeamGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) => m_subsystemBlockBehavior = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVTractorBeamBlockBehavior>(true);

        public override uint GetOutputVoltage(int face) => 0u;

        public override bool Simulate() {
            uint lastInputTop = m_inputTop;
            m_inputTop = 0u;
            uint lastInputRight = m_inputRight;
            m_inputRight = 0u;
            uint lastInputBottom = m_inputBottom;
            m_inputBottom = 0u;
            uint lastInputLeft = m_inputLeft;
            m_inputLeft = 0u;
            int rotation = Rotation;
            GVCellFace cellFace = CellFaces[0];
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != GVElectricConnectorType.Input) {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(cellFace.Face, rotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue) {
                        switch (connectorDirection.Value) {
                            case GVElectricConnectorDirection.Top:
                                m_inputTop = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                break;
                            case GVElectricConnectorDirection.Right:
                                m_inputRight = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                break;
                            case GVElectricConnectorDirection.Bottom:
                                m_inputBottom = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                break;
                            case GVElectricConnectorDirection.Left:
                                m_inputLeft = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                break;
                        }
                    }
                }
            }
            if (m_inputRight != lastInputRight) {
                m_targetOffset.X = (m_inputRight & 0x7FFFu) / (((m_inputRight >> 15) & 1u) == 1u ? -8f : 8f);
                m_targetOffset.Z = ((m_inputRight >> 16) & 0x7FFFu) / (((m_inputRight >> 31) & 1u) == 1u ? -8f : 8f);
            }
            uint rawY = m_inputTop >> 16;
            if (rawY != lastInputTop >> 16) {
                m_targetOffset.Y = (rawY & 0x7FFFu) / (rawY >> 15 == 1u ? -8f : 8f);
            }
            Point3 tractorBeamBlockPoint = cellFace.Point;
            if ((m_inputLeft & 2u) == 2u) {
                m_subsystemBlockBehavior.m_indicatorLine[cellFace] = m_targetOffset;
                if (m_subterrainSystem == null
                    && (m_inputLeft & 1u) == 0u) {
                    Point3 targetPoint = new((int)Math.Floor(tractorBeamBlockPoint.X + 0.5f + m_targetOffset.X), (int)Math.Floor(tractorBeamBlockPoint.Y + 0.5f + m_targetOffset.Y), (int)Math.Floor(tractorBeamBlockPoint.Z + 0.5f + m_targetOffset.Z));
                    if (targetPoint != m_targetPoint) {
                        m_targetPoint = targetPoint;
                        m_subsystemBlockBehavior.AddPreview(tractorBeamBlockPoint, targetPoint);
                    }
                }
                else {
                    m_targetPoint = default;
                    m_subsystemBlockBehavior.RemovePreview(tractorBeamBlockPoint);
                }
            }
            else {
                m_targetPoint = default;
                m_subsystemBlockBehavior.m_indicatorLine.Remove(cellFace);
                m_subsystemBlockBehavior.RemovePreview(tractorBeamBlockPoint);
            }
            if ((lastInputLeft & 1u) == 1u
                && (m_inputLeft & 1u) == 0u) {
                if (m_subterrainSystem != null
                    && m_subsystemBlockBehavior.RemoveSubterrain(tractorBeamBlockPoint)) {
                    m_subterrainSystem = null;
                }
            }
            else {
                float scale = m_inputTop & 0x7FFFu;
                if (((m_inputTop >> 15) & 1u) == 1u) {
                    scale = 1 / scale;
                }
                float yaw = (m_inputBottom & 0xFFu) * 0.017453292f * (((m_inputBottom >> 24) & 1u) == 1u ? -1f : 1f);
                float pitch = ((m_inputBottom >> 8) & 0xFFu) * 0.017453292f * (((m_inputBottom >> 25) & 1u) == 1u ? -1f : 1f);
                float roll = ((m_inputBottom >> 16) & 0xFFu) * 0.017453292f * (((m_inputBottom >> 26) & 1u) == 1u ? -1f : 1f);
                bool useParentLight = (m_inputBottom & 0x8000000u) == 0u;
                int light = (int)(m_inputBottom >> 28);
                if ((lastInputLeft & 1u) == 0u
                    && (m_inputLeft & 1u) == 1u) {
                    if (m_subterrainSystem == null) {
                        m_subterrainSystem = m_subsystemBlockBehavior.AddSubterrain(tractorBeamBlockPoint, new Vector3(tractorBeamBlockPoint.X + 0.5f + m_targetOffset.X, tractorBeamBlockPoint.Y + 0.5f + m_targetOffset.Y, tractorBeamBlockPoint.Z + 0.5f + m_targetOffset.Z), Matrix.CreateFromYawPitchRoll(yaw, pitch, roll) * Matrix.CreateScale(scale) * Matrix.CreateTranslation(m_targetOffset));
                        if (m_subterrainSystem != null) {
                            m_subterrainSystem.UseParentLight = useParentLight;
                            if (!useParentLight) {
                                m_subterrainSystem.Light = light;
                            }
                        }
                    }
                }
                else if (m_subterrainSystem != null) {
                    m_subterrainSystem.BaseTransform = Matrix.CreateFromYawPitchRoll(yaw, pitch, roll) * Matrix.CreateScale(scale) * Matrix.CreateTranslation(m_targetOffset);
                    m_subterrainSystem.UseParentLight = useParentLight;
                    if (!useParentLight) {
                        m_subterrainSystem.Light = light;
                    }
                }
            }
            return false;
        }
    }
}