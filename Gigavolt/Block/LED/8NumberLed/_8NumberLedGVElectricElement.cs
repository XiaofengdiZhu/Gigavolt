using Engine;

namespace Game {
    public class _8NumberLedGVElectricElement : MountedGVElectricElement {
        public SubsystemGV8NumberLedGlow m_subsystemGV8NumberLedGlow;

        public uint m_voltage;

        public GV8NumberGlowPoint m_glowPoint;

        public _8NumberLedGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) => m_subsystemGV8NumberLedGlow = subsystemGVElectricity.Project.FindSubsystem<SubsystemGV8NumberLedGlow>(true);

        public override void OnAdded() {
            GVCellFace cellFace = CellFaces[0];
            int data = Terrain.ExtractData(SubsystemGVElectricity.SubsystemGVSubterrain.GetTerrain(SubterrainId).GetCellValue(cellFace.X, cellFace.Y, cellFace.Z));
            int mountingFace = GV8NumberLedBlock.GetMountingFace(data);
            Vector3 v = new(cellFace.X + 0.5f, cellFace.Y + 0.5f, cellFace.Z + 0.5f);
            Vector3 vector = CellFace.FaceToVector3(mountingFace);
            Vector3 vector2 = mountingFace < 4 ? Vector3.UnitY : Vector3.UnitX;
            Vector3 right = Vector3.Cross(vector, vector2);
            m_glowPoint = m_subsystemGV8NumberLedGlow.AddGlowPoint();
            m_glowPoint.Position = v - 0.43f * CellFace.FaceToVector3(mountingFace);
            m_glowPoint.Forward = vector;
            m_glowPoint.Up = vector2;
            m_glowPoint.Right = right;
            m_glowPoint.Size = 0.5f;
            m_glowPoint.FarSize = 0.5f;
            m_glowPoint.FarDistance = 1f;
        }

        public override void OnRemoved() {
            m_subsystemGV8NumberLedGlow.RemoveGlowPoint(m_glowPoint);
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
                m_glowPoint.Voltage = m_voltage;
            }
            return false;
        }
    }
}