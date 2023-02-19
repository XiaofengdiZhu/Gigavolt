namespace Game
{
    public class DetonatorGVElectricElement : MountedGVElectricElement
    {
        public DetonatorGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace)
            : base(subsystemGVElectricity, cellFace)
        {
        }

        public void Detonate(uint presure)
        {
            SubsystemExplosions m_subsystemExplosions = SubsystemGVElectricity.Project.FindSubsystem<SubsystemExplosions>(throwOnError: true);
            CellFace cellFace = CellFaces[0]; 
            if (presure == 0)
            {
                int value = Terrain.MakeBlockValue(847);
                m_subsystemExplosions.TryExplodeBlock(cellFace.X, cellFace.Y, cellFace.Z, value);
            }
            else
            {
                SubsystemGVElectricity.Project.FindSubsystem<SubsystemTerrain>(throwOnError: true).ChangeCell(cellFace.X, cellFace.Y, cellFace.Z, 0);
                m_subsystemExplosions.AddExplosion(cellFace.X,cellFace.Y,cellFace.Z,(float)presure,false,false);
            }
        }

        public override bool Simulate()
        {
            uint num = 0u;
            foreach (GVElectricConnection connection in Connections)
            {
                if (connection.ConnectorType != GVElectricConnectorType.Output && connection.NeighborConnectorType != 0)
                {
                    num = MathUint.Max(num, connection.NeighborGVElectricElement.GetOutputVoltage(connection.NeighborConnectorFace));
                }
            }
            if (num > 0u)
            {
                Detonate(num);
            }
            return false;
        }

        public override void OnHitByProjectile(CellFace cellFace, WorldItem worldItem)
        {
            Detonate(0u);
        }
    }
}
