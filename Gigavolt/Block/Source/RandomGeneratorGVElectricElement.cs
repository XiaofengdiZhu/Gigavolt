using Engine;

namespace Game {
    public class RandomGeneratorGVElectricElement : RotateableGVElectricElement {
        public bool m_clockAllowed = true;

        public uint m_voltage;

        public static readonly Random s_random = new();

        public RandomGeneratorGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) {
            uint? num = SubsystemGVElectricity.ReadPersistentVoltage(CellFaces[0].Point, SubterrainId);
            m_voltage = num.HasValue ? num.Value : GetRandomVoltage();
        }

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override bool Simulate() {
            uint voltage = m_voltage;
            bool flag = false;
            bool flag2 = false;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != GVElectricConnectorType.Input) {
                    if (IsSignalHigh(connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace))) {
                        if (m_clockAllowed) {
                            flag = true;
                            m_clockAllowed = false;
                        }
                    }
                    else {
                        m_clockAllowed = true;
                    }
                    flag2 = true;
                }
            }
            if (flag2) {
                if (flag) {
                    m_voltage = GetRandomVoltage();
                }
            }
            else {
                m_voltage = GetRandomVoltage();
                SubsystemGVElectricity.QueueGVElectricElementForSimulation(this, SubsystemGVElectricity.CircuitStep + MathUtils.Max((int)(s_random.Float(0.25f, 0.75f) / 0.01f), 1));
            }
            if (m_voltage != voltage) {
                SubsystemGVElectricity.WritePersistentVoltage(CellFaces[0].Point, m_voltage, SubterrainId);
                return true;
            }
            return false;
        }

        public static uint GetRandomVoltage() => s_random.UInt();
    }
}