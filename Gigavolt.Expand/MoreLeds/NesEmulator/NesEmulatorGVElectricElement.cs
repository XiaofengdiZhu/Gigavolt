using Engine;

namespace Game {
    public class NesEmulatorGVElectricElement : RotateableGVElectricElement {
        public SubsystemNesEmulatorBlockBehavior m_subsystemNesEmulatorBlockBehavior;
        public GVNesEmulatorGlowPoint m_glowPoint;

        public NesEmulatorGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) => m_subsystemNesEmulatorBlockBehavior = subsystemGVElectricity.Project.FindSubsystem<SubsystemNesEmulatorBlockBehavior>(true);

        public override void OnAdded() {
            GVCellFace cellFace = CellFaces[0];
            int data = Terrain.ExtractData(SubsystemGVElectricity.SubsystemGVSubterrain.GetTerrain(SubterrainId).GetCellValue(cellFace.X, cellFace.Y, cellFace.Z));
            int mountingFace = GVNesEmulatorBlock.GetMountingFace(data);
            Vector3 v = new(cellFace.X + 0.5f, cellFace.Y + 0.5f, cellFace.Z + 0.5f);
            Vector3 vector = CellFace.FaceToVector3(mountingFace);
            Vector3 vector2 = mountingFace < 4 ? Vector3.UnitY : Vector3.UnitX;
            Vector3 right = Vector3.Cross(vector, vector2);
            m_glowPoint = m_subsystemNesEmulatorBlockBehavior.AddGlowPoint();
            m_glowPoint.Position = v - 0.43f * vector;
            m_glowPoint.Forward = vector;
            m_glowPoint.Up = vector2;
            m_glowPoint.Right = right;
        }

        public override void OnRemoved() {
            m_subsystemNesEmulatorBlockBehavior.RemoveGlowPoint(m_glowPoint);
        }

        public override bool Simulate() {
            uint voltage = 0;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    voltage |= connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                    break;
                }
            }
            m_glowPoint.Voltage = voltage;
            return false;
        }
    }
}