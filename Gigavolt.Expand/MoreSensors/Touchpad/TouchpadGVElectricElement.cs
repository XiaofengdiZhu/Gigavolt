using System;
using System.Collections.Generic;
using System.Linq;
using Engine;

namespace Game {
    public class TouchpadGVElectricElement : RotateableGVElectricElement {
        public readonly SubsystemGVDisplayLedGlow m_subsystemGVDisplayLedGlow;
        public HashSet<GVDisplayPoint> m_glowPoints;
        public bool m_interactStop;
        public Vector2 m_interactPosition;
        public DateTime? m_interactStartTime;
        public Vector2 m_sightPosition;
        public DateTime? m_sightStartTime;
        public uint m_rightOutput;
        public uint m_leftOutput;
        public uint m_topOutput;

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

        public TouchpadGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) => m_subsystemGVDisplayLedGlow = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVDisplayLedGlow>(true);

        public override void OnAdded() {
            GVCellFace cellFace = CellFaces[0];
            int data = Terrain.ExtractData(SubsystemGVElectricity.SubsystemGVSubterrain.GetTerrain(SubterrainId).GetCellValue(cellFace.X, cellFace.Y, cellFace.Z));
            int mountingFace = cellFace.Face;
            int rotation = RotateableMountedGVElectricElementBlock.GetRotation(data);
            m_glowPoints = m_subsystemGVDisplayLedGlow.AddGlowPoints(SubterrainId);
            GVDisplayPoint point = new() { Type = 1, Position = new Vector3(cellFace.X + 0.5f, cellFace.Y + 0.5f, cellFace.Z + 0.5f), Color = Color.White, Complex = false };
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

        public override void OnRemoved() {
            m_subsystemGVDisplayLedGlow.RemoveGlowPoints(m_glowPoints, SubterrainId);
            m_glowPoints.Clear();
            m_glowPoints = null;
        }

        public override uint GetOutputVoltage(int face) {
            GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, Rotation, face);
            if (connectorDirection.HasValue) {
                switch (connectorDirection.Value) {
                    case GVElectricConnectorDirection.Right: return m_rightOutput;
                    case GVElectricConnectorDirection.Top: return m_topOutput;
                    case GVElectricConnectorDirection.Left: return m_leftOutput;
                }
            }
            return 0u;
        }

        public override bool Simulate() {
            DateTime now = DateTime.Now;
            int electricRotation = Rotation;
            uint inputIn = 0u;
            uint inputBottom = 0u;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != GVElectricConnectorType.Input) {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(CellFaces[0].Face, electricRotation, connection.ConnectorFace);
                    if (connectorDirection.HasValue) {
                        switch (connectorDirection.Value) {
                            case GVElectricConnectorDirection.In:
                                inputIn = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                break;
                            case GVElectricConnectorDirection.Bottom:
                                inputBottom = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                break;
                        }
                    }
                }
            }
            m_glowPoints.First().Value = inputIn;
            bool sightMode = (inputBottom & 1u) == 1u;
            bool notKeep = (inputBottom & 2u) == 0u;
            m_interactStop = !sightMode && (inputBottom & 4u) == 4u;
            if (sightMode) {
                m_interactPosition = default;
                m_interactStartTime = null;
                if (m_sightStartTime.HasValue) {
                    m_topOutput = (uint)Math.Floor((now - m_sightStartTime.Value).TotalMilliseconds);
                    m_rightOutput = (uint)(m_sightPosition.X * uint.MaxValue);
                    m_leftOutput = (uint)(m_sightPosition.Y * uint.MaxValue);
                    return true;
                }
            }
            else {
                m_sightPosition = default;
                m_sightStartTime = null;
                if (m_interactStop) {
                    if (m_interactStartTime.HasValue) {
                        m_topOutput = (uint)Math.Floor((now - m_interactStartTime.Value).TotalMilliseconds);
                        m_rightOutput = (uint)(m_interactPosition.X * uint.MaxValue);
                        m_leftOutput = (uint)(m_interactPosition.Y * uint.MaxValue);
                        return true;
                    }
                }
                else if (m_interactPosition != default) {
                    m_topOutput = 0u;
                    m_rightOutput = (uint)(m_interactPosition.X * uint.MaxValue);
                    m_leftOutput = (uint)(m_interactPosition.Y * uint.MaxValue);
                    m_interactPosition = default;
                    if (notKeep) {
                        SubsystemGVElectricity.QueueGVElectricElementForSimulation(this, SubsystemGVElectricity.CircuitStep + 2);
                    }
                    return true;
                }
            }
            if (notKeep && (!m_interactStop || m_topOutput != 0u)) {
                m_topOutput = 0u;
                m_rightOutput = 0u;
                m_leftOutput = 0u;
                return true;
            }
            return false;
        }

        public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner) => false;
    }
}