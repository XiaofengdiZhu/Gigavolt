using Engine;

namespace Game {
    public class MultistateFurnitureGVElectricElement : FurnitureGVElectricElement {
        public bool m_isActionAllowed;

        public double? m_lastActionTime;

        public MultistateFurnitureGVElectricElement(SubsystemGVElectricity subsystemGVElectricity, Point3 point, uint subterrainId) : base(subsystemGVElectricity, point, subterrainId) { }

        public override bool Simulate() {
            if (CalculateHighInputsCount() > 0) {
                if (m_isActionAllowed && (!m_lastActionTime.HasValue || SubsystemGVElectricity.SubsystemTime.GameTime - m_lastActionTime > 0.1)) {
                    m_isActionAllowed = false;
                    m_lastActionTime = SubsystemGVElectricity.SubsystemTime.GameTime;
                    SubsystemGVElectricity.Project.FindSubsystem<SubsystemFurnitureBlockBehavior>(true).SwitchToNextState(CellFaces[0].X, CellFaces[0].Y, CellFaces[0].Z, false);
                }
            }
            else {
                m_isActionAllowed = true;
            }
            return false;
        }
    }
}