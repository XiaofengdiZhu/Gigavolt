namespace Game {
    public class ButtonCabinetGVElectricElement : MountedGVElectricElement {
        public uint m_voltage;
        public bool m_wasPressed;
        public int m_duration;

        public ButtonCabinetGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, GVCellFace[] cellFaces, uint subterrainId, int duration = 10) : base(subsystemGVElectricity, cellFaces, subterrainId) => m_duration = duration;

        public override uint GetOutputVoltage(int face) => m_voltage;

        public override bool Simulate() {
            uint voltage = m_voltage;
            if (m_wasPressed) {
                m_wasPressed = false;
                m_voltage = uint.MaxValue;
                SubsystemGVElectricity.QueueGVElectricElementForSimulation(this, SubsystemGVElectricity.CircuitStep + m_duration);
            }
            else {
                m_voltage = 0u;
            }
            return m_voltage != voltage;
        }

        public override void OnRemoved() {
            GVCellFace cellFace = CellFaces[0];
            if (cellFace.Mask == 1) {
                for (int color = 1; color < 16; color++) {
                    SubsystemGVElectricity.RemoveGVElectricElement(
                        SubsystemGVElectricity.GetGVElectricElement(
                            cellFace.X,
                            cellFace.Y,
                            cellFace.Z,
                            cellFace.Face,
                            SubterrainId,
                            1 << color
                        )
                    );
                }
            }
        }
    }
}