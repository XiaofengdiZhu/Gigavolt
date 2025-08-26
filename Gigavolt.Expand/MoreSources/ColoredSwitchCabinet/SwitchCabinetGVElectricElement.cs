namespace Game {
    public class SwitchCabinetGVElectricElement : MountedGVElectricElement {
        public bool m_on;

        public SwitchCabinetGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace[] cellFaces, uint subterrainId, bool on) :
            base(subsystemGVElectricity, cellFaces, subterrainId) => m_on = on;

        public override uint GetOutputVoltage(int face) => m_on ? uint.MaxValue : 0u;

        public override void OnRemoved() {
            GVCellFace cellFace = CellFaces[0];
            if (cellFace.Mask == 1) {
                for (int color = 1; color < 16; color++) {
                    SubsystemGVElectricity.RemoveGVElectricElement(
                        SubsystemGVElectricity.GetGVElectricElement(cellFace.X, cellFace.Y, cellFace.Z, cellFace.Face, SubterrainId, 1 << color)
                    );
                }
            }
        }
    }
}