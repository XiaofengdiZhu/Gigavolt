using Engine;

namespace Game {
    public class SwitchFurnitureGVElectricElement : FurnitureGVElectricElement {
        public readonly uint m_voltage;

        public SwitchFurnitureGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, Point3 point, int value, uint subterrainId) : base(subsystemGVElectricity, point, subterrainId) {
            FurnitureDesign design = FurnitureBlock.GetDesign(subsystemGVElectricity.SubsystemTerrain.SubsystemFurnitureBlockBehavior, value);
            if (design?.LinkedDesign != null) {
                m_voltage = design.Index >= design.LinkedDesign.Index ? uint.MaxValue : 0u;
            }
        }

        public override uint GetOutputVoltage(int face) => m_voltage;
    }
}