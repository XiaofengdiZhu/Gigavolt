using Engine;

namespace Game {
    public class LedGVElectricElement : MountedGVElectricElement {
        public SubsystemGlow m_subsystemGlow;

        public uint m_voltage;

        public GlowPoint m_glowPoint;

        public Color m_color;

        public LedGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) => m_subsystemGlow = subsystemGVElectricity.Project.FindSubsystem<SubsystemGlow>(true);

        public override void OnAdded() {
            m_glowPoint = m_subsystemGlow.AddGlowPoint();
            GVCellFace cellFace = CellFaces[0];
            int data = Terrain.ExtractData(SubsystemGVElectricity.SubsystemGVSubterrain.GetTerrain(SubterrainId).GetCellValue(cellFace.X, cellFace.Y, cellFace.Z));
            int mountingFace = GVLedBlock.GetMountingFace(data);
            m_color = GVLedBlock.LedColors[GVLedBlock.GetColor(data)];
            Vector3 v = new(cellFace.X + 0.5f, cellFace.Y + 0.5f, cellFace.Z + 0.5f);
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
            m_voltage = CalculateVoltage();
            if (IsSignalHigh(m_voltage) != IsSignalHigh(voltage)) {
                m_glowPoint.Color = IsSignalHigh(m_voltage) ? m_color : Color.Transparent;
            }
            return false;
        }

        public uint CalculateVoltage() => CalculateHighInputsCount() > 0 ? 15u : 0u;
    }
}