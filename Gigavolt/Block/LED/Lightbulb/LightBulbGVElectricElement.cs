using System;

namespace Game {
    public class LightBulbGVElectricElement : MountedGVElectricElement {
        public int m_intensity;

        public int m_lastChangeCircuitStep;

        public LightBulbGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, int value, uint subterrainId) : base(
            subsystemGVElectricity,
            cellFace,
            subterrainId
        ) {
            m_lastChangeCircuitStep = SubsystemGVElectricity.CircuitStep;
            int data = Terrain.ExtractData(value);
            m_intensity = GVLightbulbBlock.GetLightIntensity(data);
        }

        public override bool Simulate() {
            int num = SubsystemGVElectricity.CircuitStep - m_lastChangeCircuitStep;
            uint num2 = 0u;
            foreach (GVElectricConnection connection in Connections) {
                if (connection.ConnectorType != GVElectricConnectorType.Output
                    && connection.NeighborConnectorType != 0) {
                    num2 = MathUint.Max(num2, connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace));
                }
            }
            int intensity = m_intensity;
            m_intensity = Math.Clamp((int)(num2 & 0xfu) * 2 - 15, 0, 15);
            if (m_intensity != intensity) {
                m_lastChangeCircuitStep = SubsystemGVElectricity.CircuitStep;
            }
            if (num >= 10) {
                GVCellFace cellFace = CellFaces[0];
                int cellValue = SubsystemGVElectricity.SubsystemGVSubterrain.GetTerrain(SubterrainId)
                    .GetCellValue(cellFace.X, cellFace.Y, cellFace.Z);
                int data = GVLightbulbBlock.SetLightIntensity(Terrain.ExtractData(cellValue), m_intensity);
                int value = Terrain.ReplaceData(cellValue, data);
                SubsystemGVElectricity.SubsystemGVSubterrain.ChangeCell(cellFace.X, cellFace.Y, cellFace.Z, SubterrainId, value);
            }
            else {
                SubsystemGVElectricity.QueueGVElectricElementForSimulation(this, SubsystemGVElectricity.CircuitStep + 10 - num);
            }
            return false;
        }
    }
}