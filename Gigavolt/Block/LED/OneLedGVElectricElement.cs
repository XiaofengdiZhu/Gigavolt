using Engine;
using System;

namespace Game
{
    public class OneLedGVElectricElement : MountedGVElectricElement
    {
        public SubsystemGlow m_subsystemGlow;

        public uint m_voltage;

        public Color m_color;

        public GlowPoint m_glowPoint;

        public OneLedGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace)
            : base(subsystemGVElectricity, cellFace)
        {
            m_subsystemGlow = subsystemGVElectricity.Project.FindSubsystem<SubsystemGlow>(throwOnError: true);
        }

        public override void OnAdded()
        {
            CellFace cellFace = CellFaces[0];
            int data = Terrain.ExtractData(SubsystemGVElectricity.SubsystemTerrain.Terrain.GetCellValue(cellFace.X, cellFace.Y, cellFace.Z));
            int mountingFace = FourLedBlock.GetMountingFace(data);
            m_color = LedBlock.LedColors[FourLedBlock.GetColor(data)];
            var v = new Vector3(cellFace.X + 0.5f, cellFace.Y + 0.5f, cellFace.Z + 0.5f);
            Vector3 vector = CellFace.FaceToVector3(mountingFace);
            Vector3 vector2 = (mountingFace < 4) ? Vector3.UnitY : Vector3.UnitX;
            var right = Vector3.Cross(vector, vector2);
            m_glowPoint = m_subsystemGlow.AddGlowPoint();
            m_glowPoint.Position = v - 0.4375f * CellFace.FaceToVector3(mountingFace);
            m_glowPoint.Forward = vector;
            m_glowPoint.Up = vector2;
            m_glowPoint.Right = right;
            m_glowPoint.Color = Color.Transparent;
            m_glowPoint.Size = 0.52f;
            m_glowPoint.FarSize = 0.52f;
            m_glowPoint.FarDistance = 1f;
            m_glowPoint.Type = GlowPointType.Square;
        }

        public override void OnRemoved()
        {
            m_subsystemGlow.RemoveGlowPoint(m_glowPoint);
        }

        public override bool Simulate()
        {
            uint voltage = m_voltage;
            m_voltage = 0;
            foreach (GVElectricConnection connection in Connections)
            {
                if (connection.ConnectorType != GVElectricConnectorType.Output && connection.NeighborConnectorType != 0)
                {
                    m_voltage = MathUint.Max(m_voltage, connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace));
                }
            }
            if (m_voltage != voltage)
            {
                m_glowPoint.Color = new Color(m_voltage);
            }
            return false;
        }
    }
}
