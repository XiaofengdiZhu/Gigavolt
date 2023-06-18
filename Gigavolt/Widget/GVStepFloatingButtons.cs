using System;
using System.Xml.Linq;
using Engine;

namespace Game {
    public class GVStepFloatingButtons : CanvasWidget {
        public ButtonWidget m_stopButton;
        public ButtonWidget m_stepButton;
        public ButtonWidget m_jumpButton;
        public LabelWidget m_count;
        public LabelWidget m_time;
        public SubsystemGVElectricity m_subsystem;

        public GVStepFloatingButtons(SubsystemGVElectricity subsystem) {
            XElement node = ContentManager.Get<XElement>("Widgets/GVStepFloatingButtons");
            LoadContents(this, node);
            m_stopButton = Children.Find<ButtonWidget>("GVStepFloatingButtons.Stop");
            m_stepButton = Children.Find<ButtonWidget>("GVStepFloatingButtons.Step");
            m_jumpButton = Children.Find<ButtonWidget>("GVStepFloatingButtons.Jump");
            m_count = Children.Find<LabelWidget>("GVStepFloatingButtons.Count");
            m_time = Children.Find<LabelWidget>("GVStepFloatingButtons.Time");
            m_subsystem = subsystem;
        }

        public override void Update() {
            if (m_stopButton.IsClicked) {
                m_subsystem.debugMode = !m_subsystem.debugMode;
                if (m_subsystem.debugMode) {
                    m_subsystem.lastUpdate = new DateTime();
                    m_subsystem.last1000Updates.Clear();
                }
            }
            if (m_stepButton.IsClicked) {
                if (!m_subsystem.debugMode) {
                    m_subsystem.debugMode = true;
                }
                try {
                    m_subsystem.StepUpdate();
                }
                catch (Exception ex) {
                    Log.Error(ex);
                }
            }
            if (m_jumpButton.IsClicked) {
                if (!m_subsystem.debugMode) {
                    m_subsystem.debugMode = true;
                }
                try {
                    m_subsystem.JumpUpdate();
                }
                catch (Exception ex) {
                    Log.Error(ex);
                }
            }
            m_count.Text = (m_subsystem.last1000Updates.Count - 1).ToString();
            m_time.Text = (m_subsystem.lastUpdate - (m_subsystem.last1000Updates.Count > 0 ? m_subsystem.last1000Updates.Peek() : m_subsystem.lastUpdate)).TotalSeconds.ToString("f2");
        }
    }
}