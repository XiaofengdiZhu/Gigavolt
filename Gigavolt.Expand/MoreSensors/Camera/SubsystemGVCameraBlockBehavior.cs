using System.Collections.Generic;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVCameraBlockBehavior : Subsystem {
        public SubsystemGameWidgets m_subsystemGameWidgets;
        public readonly HashSet<GameWidget> m_gameWidgets = new();

        public override void Load(ValuesDictionary valuesDictionary) {
            base.Load(valuesDictionary);
            m_subsystemGameWidgets = Project.FindSubsystem<SubsystemGameWidgets>(true);
            int index = Project.m_subsystems.IndexOf(m_subsystemGameWidgets);
            Project.m_subsystems.Remove(this);
            Project.m_subsystems.Insert(index - 1, this);
        }

        public override void Dispose() {
            foreach (GameWidget widget in m_gameWidgets) {
                m_subsystemGameWidgets.m_gameWidgets.Remove(widget);
            }
        }
    }
}