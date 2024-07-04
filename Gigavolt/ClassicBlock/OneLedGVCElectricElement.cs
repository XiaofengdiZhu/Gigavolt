using System;
using Engine;

namespace Game {
    public class OneLedGVCElectricElement : MountedGVElectricElement {
        public readonly SubsystemGVGlow m_subsystemGlow;

        public uint m_voltage;

        public Color m_color;

        public GVGlowPoint m_glowPoint;

        public OneLedGVCElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) => m_subsystemGlow = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVGlow>(true);

        public override void OnAdded() {
            GVCellFace cellFace = CellFaces[0];
            int data = Terrain.ExtractData(SubsystemGVElectricity.SubsystemGVSubterrain.GetTerrain(SubterrainId).GetCellValue(cellFace.X, cellFace.Y, cellFace.Z));
            int mountingFace = FourLedBlock.GetMountingFace(data);
            m_color = LedBlock.LedColors[FourLedBlock.GetColor(data)];
            Vector3 v = new(cellFace.X + 0.5f, cellFace.Y + 0.5f, cellFace.Z + 0.5f);
            Vector3 vector = CellFace.FaceToVector3(mountingFace);
            Vector3 vector2 = mountingFace < 4 ? Vector3.UnitY : Vector3.UnitX;
            Vector3 right = Vector3.Cross(vector, vector2);
            m_glowPoint = m_subsystemGlow.AddGlowPoint(SubterrainId);
            m_glowPoint.Position = v - 0.43f * CellFace.FaceToVector3(mountingFace);
            m_glowPoint.Forward = vector;
            m_glowPoint.Up = vector2;
            m_glowPoint.Right = right;
            m_glowPoint.Color = Color.Transparent;
            m_glowPoint.Size = 0.52f;
            m_glowPoint.Type = GVGlowPointType.Square;
        }

        public override void OnRemoved() {
            m_subsystemGlow.RemoveGlowPoint(m_glowPoint, SubterrainId);
        }

        public override bool Simulate() {
            uint voltage = m_voltage;
            m_voltage = 0u;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    m_voltage = MathUint.Max(m_voltage, connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace));
                }
            }
            if (m_voltage != voltage) {
                int num = (int)MathUint.Clamp(m_voltage, 0, 15);
                m_glowPoint.Color = num >= 8 ? LedBlock.LedColors[Math.Clamp(num - 8, 0, 7)] : Color.Transparent;
            }
            return false;
        }
    }
}