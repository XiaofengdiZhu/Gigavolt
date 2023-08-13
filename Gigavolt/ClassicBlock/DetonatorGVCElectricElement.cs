namespace Game {
    public class DetonatorGVCElectricElement : MountedGVElectricElement {
        public DetonatorGVCElectricElement(SubsystemGVElectricity subsystemGVElectricity, CellFace cellFace) : base(subsystemGVElectricity, cellFace) { }

        public void Detonate() {
            CellFace cellFace = CellFaces[0];
            int value = Terrain.MakeBlockValue(GVDetonatorCBlock.Index);
            SubsystemGVElectricity.Project.FindSubsystem<SubsystemExplosions>(true).TryExplodeBlock(cellFace.X, cellFace.Y, cellFace.Z, value);
        }

        public override bool Simulate() {
            if (CalculateHighInputsCount() > 0) {
                Detonate();
            }
            return false;
        }

        public override void OnHitByProjectile(CellFace cellFace, WorldItem worldItem) {
            Detonate();
        }
    }
}