using Engine;

namespace Game {
    public class SwitchFurnitureGVElectricElement : FurnitureGVElectricElement {
        public uint m_voltage;

        public SwitchFurnitureGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, Point3 point, int value) : base(subsystemGVElectricity, point) {
            FurnitureDesign design = FurnitureBlock.GetDesign(subsystemGVElectricity.SubsystemTerrain.SubsystemFurnitureBlockBehavior, value);
            if (design != null
                && design.LinkedDesign != null) {
                m_voltage = design.Index >= design.LinkedDesign.Index ? uint.MaxValue : 0u;
            }
        }

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner) {
            GVCellFace cellFace = CellFaces[0];
            SubsystemGVElectricity.SubsystemTerrain.SubsystemFurnitureBlockBehavior.SwitchToNextState(cellFace.X, cellFace.Y, cellFace.Z, false);
            SubsystemGVElectricity.SubsystemAudio.PlaySound(
                "Audio/Click",
                1f,
                0f,
                new Vector3(cellFace.X, cellFace.Y, cellFace.Z),
                2f,
                true
            );
            return true;
        }
    }
}