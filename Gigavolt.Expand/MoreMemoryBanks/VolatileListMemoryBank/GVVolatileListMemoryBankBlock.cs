using Engine.Graphics;

namespace Game {
    public class GVVolatileListMemoryBankBlock : GVListMemoryBankBlock {
        public new const int Index = 873;

        public override void Initialize() {
            RotateableMountedGVElectricElementBlockInitialize();
            m_texture = ContentManager.Get<Texture2D>("Textures/GVVolatileListMemoryBankBlock");
        }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z, uint subterrainId) => new VolatileListMemoryBankGVElectricElement(subsystemGVElectricity, new GVCellFace(x, y, z, GetFace(value)), subterrainId);
    }
}