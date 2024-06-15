using Engine;

namespace Game {
    public class DetonatorGVCElectricElement : MountedGVElectricElement {
        public DetonatorGVCElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace cellFace, uint subterrainId) : base(subsystemGVElectricity, cellFace, subterrainId) { }

        public void Detonate() {
            Point3 position = CellFaces[0].Point;
            if (SubterrainId != 0) {
                position = Terrain.ToCell(Vector3.Transform(new Vector3(position.X + 0.5f, position.Y + 0.5f, position.Z + 0.5f), GVStaticStorage.GVSubterrainSystemDictionary[SubterrainId].GlobalTransform));
            }
            Block block = BlocksManager.Blocks[GVDetonatorCBlock.Index];
            SubsystemGVElectricity.Project.FindSubsystem<SubsystemExplosions>(true)
            .AddExplosion(
                position.X,
                position.Y,
                position.Z,
                block.GetExplosionPressure(GVDetonatorCBlock.Index),
                block.GetExplosionIncendiary(GVDetonatorCBlock.Index),
                false
            );
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