using Engine;

namespace Game {
    public class MulticoloredLedGVElectricElement : MountedGVElectricElement {
        public SubsystemGlow m_subsystemGlow;

        public uint m_voltage;

        public GlowPoint m_glowPoint;

        public MulticoloredLedGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace) : base(subsystemGVElectricity, cellFace) => m_subsystemGlow = subsystemGVElectricity.Project.FindSubsystem<SubsystemGlow>(true);

        public override void OnAdded() {
            m_glowPoint = m_subsystemGlow.AddGlowPoint();
            CellFace cellFace = CellFaces[0];
            int mountingFace = GVMulticoloredLedBlock.GetMountingFace(Terrain.ExtractData(SubsystemGVElectricity.SubsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z)));
            Vector3 v = new Vector3(cellFace.X + 0.5f, cellFace.Y + 0.5f, cellFace.Z + 0.5f);
            m_glowPoint.Position = v - 0.4375f * CellFace.FaceToVector3(mountingFace);
            m_glowPoint.Forward = CellFace.FaceToVector3(mountingFace);
            m_glowPoint.Up = mountingFace < 4 ? Vector3.UnitY : Vector3.UnitX;
            m_glowPoint.Right = Vector3.Cross(m_glowPoint.Forward, m_glowPoint.Up);
            m_glowPoint.Color = Color.Transparent;
            m_glowPoint.Size = 0.0324f;
            m_glowPoint.FarSize = 0.0324f;
            m_glowPoint.FarDistance = 0f;
            m_glowPoint.Type = GlowPointType.Square;
        }

        public override void OnRemoved() {
            m_subsystemGlow.RemoveGlowPoint(m_glowPoint);
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
                m_glowPoint.Color = new Color(m_voltage);
            }
            return false;
        }
    }
}