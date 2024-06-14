using Engine;

namespace Game {
    public class OscilloscopeGVElectricElement : MountedGVElectricElement {
        public SubsystemGVOscilloscopeBlockBehavior m_subsystemGlow;
        public GVOscilloscopeData m_data;
        public uint m_lastInInput;
        public int m_lastCircuitStep;

        public OscilloscopeGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) => m_subsystemGlow = subsystemGVElectricity.Project.FindSubsystem<SubsystemGVOscilloscopeBlockBehavior>(true);

        public override void OnAdded() {
            GVCellFace cellFace = CellFaces[0];
            int data = Terrain.ExtractData(SubsystemGVElectricity.SubsystemGVSubterrain.GetTerrain(SubterrainId).GetCellValue(cellFace.X, cellFace.Y, cellFace.Z));
            int mountingFace = FourLedBlock.GetMountingFace(data);
            Vector3 v = new(cellFace.X + 0.5f, cellFace.Y + 0.5f, cellFace.Z + 0.5f);
            Vector3 vector = CellFace.FaceToVector3(mountingFace);
            Vector3 vector2 = mountingFace < 4 ? Vector3.UnitY :
                mountingFace == 4 ? -Vector3.UnitZ : Vector3.UnitZ;
            Vector3 right = Vector3.Cross(vector, vector2);
            m_data = m_subsystemGlow.GetData(cellFace.Point);
            m_data.Position = v - 0.43f * CellFace.FaceToVector3(mountingFace);
            m_data.Forward = vector;
            m_data.Up = vector2;
            m_data.Right = right;
        }

        public override void OnRemoved() {
            m_subsystemGlow.RemoveData(CellFaces[0].Point);
        }

        public override bool Simulate() {
            uint topInput = 0u;
            uint rightInput = 0u;
            uint bottomInput = 0u;
            uint leftInput = 0u;
            uint inInput = 0u;
            bool inConected = false;
            int face = CellFaces[0].Face;
            m_data.ConnectionState = new bool[4];
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != GVElectricConnectorType.Input) {
                    GVElectricConnectorDirection? connectorDirection = SubsystemGVElectricity.GetConnectorDirection(face, 0, connection.ConnectorFace);
                    if (connectorDirection.HasValue) {
                        switch (connectorDirection) {
                            case GVElectricConnectorDirection.Top:
                                topInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                m_data.ConnectionState[0] = true;
                                break;
                            case GVElectricConnectorDirection.Right:
                                rightInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                m_data.ConnectionState[1] = true;
                                break;
                            case GVElectricConnectorDirection.Bottom:
                                bottomInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                m_data.ConnectionState[2] = true;
                                break;
                            case GVElectricConnectorDirection.Left:
                                leftInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                m_data.ConnectionState[3] = true;
                                break;
                            case GVElectricConnectorDirection.In:
                                inInput = connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace);
                                inConected = true;
                                break;
                        }
                    }
                }
            }
            if (inConected) {
                if (m_lastInInput != inInput) {
                    if (m_lastInInput == 0u) {
                        m_data.AddRecord([topInput, rightInput, bottomInput, leftInput]);
                    }
                    m_lastInInput = inInput;
                }
            }
            else if (SubsystemGVElectricity.CircuitStep != m_lastCircuitStep) {
                m_lastCircuitStep = SubsystemGVElectricity.CircuitStep;
                m_data.AddRecord([topInput, rightInput, bottomInput, leftInput]);
            }
            return false;
        }
    }
}