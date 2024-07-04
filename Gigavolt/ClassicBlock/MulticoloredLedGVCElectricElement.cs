using System;
using Engine;

namespace Game {
    public class MulticoloredLedGVCElectricElement : MountedGVElectricElement {
        public readonly SubsystemGVGlow m_subsystemGlow;

        public uint m_voltage;

        public GVGlowPoint m_glowPoint;

        public MulticoloredLedGVCElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) => m_subsystemGlow = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVGlow>(true);

        public override void OnAdded() {
            m_glowPoint = m_subsystemGlow.AddGlowPoint(SubterrainId);
            GVCellFace cellFace = CellFaces[0];
            int mountingFace = MulticoloredLedBlock.GetMountingFace(Terrain.ExtractData(SubsystemGVElectricity.SubsystemGVSubterrain.GetTerrain(SubterrainId).GetCellValue(cellFace.X, cellFace.Y, cellFace.Z)));
            Vector3 v = new(cellFace.X + 0.5f, cellFace.Y + 0.5f, cellFace.Z + 0.5f);
            m_glowPoint.Position = v - 0.43f * CellFace.FaceToVector3(mountingFace);
            m_glowPoint.Forward = CellFace.FaceToVector3(mountingFace);
            m_glowPoint.Up = mountingFace < 4 ? Vector3.UnitY : Vector3.UnitX;
            m_glowPoint.Right = Vector3.Cross(m_glowPoint.Forward, m_glowPoint.Up);
            m_glowPoint.Color = Color.Transparent;
            m_glowPoint.Size = 0.0324f;
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