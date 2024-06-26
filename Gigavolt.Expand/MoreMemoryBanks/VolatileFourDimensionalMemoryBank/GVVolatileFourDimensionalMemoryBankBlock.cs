﻿using Engine.Graphics;

namespace Game {
    public class GVVolatileFourDimensionalMemoryBankBlock : GVFourDimensionalMemoryBankBlock {
        public new const int Index = 892;

        public override void Initialize() {
            RotateableMountedGVElectricElementBlockInitialize();
            m_texture = ContentManager.Get<Texture2D>("Textures/GVVolatileFourDimensionalMemoryBankBlock");
        }

        public override GVElectricElement CreateGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, int value, int x, int y, int z, uint subterrainId) => new VolatileFourDimensionalMemoryBankGVElectricElement(subsystemGVElectricity, new GVCellFace(x, y, z, GetFace(value)), subterrainId);
    }
}