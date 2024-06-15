using Engine;

namespace Game {
    public class FourLedGVElectricElement : MountedGVElectricElement {
        public SubsystemGVGlow m_subsystemGlow;

        public uint m_voltage;
        public Color m_color;
        public GVGlowPoint[] m_glowPoints = new GVGlowPoint[4];

        public FourLedGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) => m_subsystemGlow = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVGlow>(true);

        public override void OnAdded() {
            GVCellFace cellFace = CellFaces[0];
            int data = Terrain.ExtractData(SubsystemGVElectricity.SubsystemGVSubterrain.GetTerrain(SubterrainId).GetCellValue(cellFace.X, cellFace.Y, cellFace.Z));
            int mountingFace = GVFourLedBlock.GetMountingFace(data);
            m_color = GVLedBlock.LedColors[GVFourLedBlock.GetColor(data)];
            for (int i = 0; i < 4; i++) {
                int num = i % 2 == 0 ? 1 : -1;
                int num2 = i / 2 == 0 ? 1 : -1;
                Vector3 v = new(cellFace.X + 0.5f, cellFace.Y + 0.5f, cellFace.Z + 0.5f);
                Vector3 vector = CellFace.FaceToVector3(mountingFace);
                Vector3 vector2 = mountingFace < 4 ? Vector3.UnitY : Vector3.UnitX;
                Vector3 vector3 = Vector3.Cross(vector, vector2);
                GVGlowPoint glowPoint = m_subsystemGlow.AddGlowPoint(SubterrainId);
                glowPoint.Position = v - 0.43f * CellFace.FaceToVector3(mountingFace) + 0.25f * vector3 * num + 0.25f * vector2 * num2;
                glowPoint.Forward = vector;
                glowPoint.Up = vector2;
                glowPoint.Right = vector3;
                glowPoint.Color = Color.Transparent;
                glowPoint.Size = 0.26f;
                glowPoint.Type = GVGlowPointType.Square;
                m_glowPoints[i] = glowPoint;
            }
        }

        public override void OnRemoved() {
            foreach (GVGlowPoint glowPoint in m_glowPoints) {
                m_subsystemGlow.RemoveGlowPoint(glowPoint, SubterrainId);
            }
        }

        public override bool Simulate() {
            uint voltage = m_voltage;
            m_voltage = 0;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    m_voltage = MathUint.Max(m_voltage, connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace));
                }
            }
            if (m_voltage != voltage) {
                uint num = m_voltage;
                for (int i = 0; i < 4; i++) {
                    m_glowPoints[i].Color = (num & (1 << i)) != 0 ? m_color : Color.Transparent;
                }
            }
            return false;
        }
    }
}