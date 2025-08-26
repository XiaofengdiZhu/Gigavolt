using Engine;

namespace Game {
    public class PhotodiodeGVElectricElement : MountedGVElectricElement {
        public uint m_voltage;

        public PhotodiodeGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, uint subterrainId) : base(
            subsystemGVElectricity,
            cellFace,
            subterrainId
        ) => m_voltage = CalculateVoltage();

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override bool Simulate() {
            float voltage = m_voltage;
            m_voltage = CalculateVoltage();
            SubsystemGVElectricity.QueueGVElectricElementForSimulation(this, SubsystemGVElectricity.CircuitStep + MathUtils.Max(50, 1));
            return m_voltage != voltage;
        }

        public uint CalculateVoltage() {
            GVCellFace cellFace = CellFaces[0];
            if (SubterrainId == 0) {
                Point3 point = CellFace.FaceToPoint3(cellFace.Face);
                int cellLight = SubsystemGVElectricity.SubsystemTerrain.Terrain.GetCellLight(cellFace.X, cellFace.Y, cellFace.Z);
                int cellLight2 = SubsystemGVElectricity.SubsystemTerrain.Terrain.GetCellLight(
                    cellFace.X + point.X,
                    cellFace.Y + point.Y,
                    cellFace.Z + point.Z
                );
                return (uint)MathUtils.Max(cellLight, cellLight2);
            }
            Point3 position = Terrain.ToCell(
                Vector3.Transform(
                    new Vector3(cellFace.X + 0.5f, cellFace.Y + 0.5f, cellFace.Z + 0.5f),
                    GVStaticStorage.GVSubterrainSystemDictionary[SubterrainId].GlobalTransform
                )
            );
            return (uint)SubsystemGVElectricity.SubsystemTerrain.Terrain.GetCellLight(position.X, position.Y, position.Z);
        }
    }
}