using Engine;

// ReSharper disable RedundantExplicitArraySize

namespace Game {
    public class SevenSegmentDisplayGVElectricElement : MountedGVElectricElement {
        public readonly SubsystemGVGlow m_subsystemGlow;

        public uint m_voltage;

        public readonly GVGlowPoint[] m_glowPoints = new GVGlowPoint[7];

        public Color m_color;

        public readonly Vector2[] m_centers = [
            new(0f, 6f),
            new(-4f, 3f),
            new(-4f, -3f),
            new(0f, -6f),
            new(4f, -3f),
            new(4f, 3f),
            new(0f, 0f)
        ];

        public readonly Vector2[] m_sizes = [
            new(3.2f, 1f),
            new(1f, 2.3f),
            new(1f, 2.3f),
            new(3.2f, 1f),
            new(1f, 2.3f),
            new(1f, 2.3f),
            new(3.2f, 1f)
        ];

        public readonly int[] m_patterns = [
            0,
            6,
            91,
            79,
            102,
            109,
            125,
            7,
            127,
            111,
            119,
            124,
            57,
            94,
            121,
            113
        ];

        public SevenSegmentDisplayGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) => m_subsystemGlow = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVGlow>(true);

        public override void OnAdded() {
            GVCellFace cellFace = CellFaces[0];
            int data = Terrain.ExtractData(SubsystemGVElectricity.SubsystemGVSubterrain.GetTerrain(SubterrainId).GetCellValue(cellFace.X, cellFace.Y, cellFace.Z));
            int mountingFace = GVSevenSegmentDisplayBlock.GetMountingFace(data);
            m_color = GVLedBlock.LedColors[GVSevenSegmentDisplayBlock.GetColor(data)];
            for (int i = 0; i < 7; i++) {
                Vector3 v = new(cellFace.X + 0.5f, cellFace.Y + 0.5f, cellFace.Z + 0.5f);
                Vector3 vector = CellFace.FaceToVector3(mountingFace);
                Vector3 vector2 = mountingFace < 4 ? Vector3.UnitY : Vector3.UnitX;
                Vector3 v2 = Vector3.Cross(vector, vector2);
                GVGlowPoint glowPoint = m_subsystemGlow.AddGlowPoint(SubterrainId);
                glowPoint.Position = v - 0.43f * CellFace.FaceToVector3(mountingFace) + m_centers[i].X * 0.0625f * v2 + m_centers[i].Y * 0.0625f * vector2;
                glowPoint.Forward = vector;
                Vector2 size = m_sizes[i];
                glowPoint.Right = v2 * size.X * 0.0625f;
                glowPoint.Up = vector2 * size.Y * 0.0625f;
                glowPoint.Color = Color.Transparent;
                glowPoint.Size = 1.35f;
                glowPoint.Type = m_sizes[i].X > m_sizes[i].Y ? GVGlowPointType.HorizontalRectangle : GVGlowPointType.VerticalRectangle;
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
                uint num = m_voltage & 0xfu;
                for (int i = 0; i < 7; i++) {
                    m_glowPoints[i].Color = (m_patterns[num] & (1 << i)) != 0 ? m_color : Color.Transparent;
                }
            }
            return false;
        }
    }
}