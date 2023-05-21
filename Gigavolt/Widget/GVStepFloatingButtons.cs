using System;
using System.Xml.Linq;
using Engine;

namespace Game {
    public class GVStepFloatingButtons : CanvasWidget {
        public ButtonWidget m_stopButton;
        public ButtonWidget m_stepButton;
        public ButtonWidget m_jumpButton;
        public LabelWidget m_label;
        public SubsystemGVElectricity m_subsystem;

        public GVStepFloatingButtons(SubsystemGVElectricity subsystem) {
            XElement node = ContentManager.Get<XElement>("Widgets/GVStepFloatingButtons");
            LoadContents(this, node);
            m_stopButton = Children.Find<ButtonWidget>("GVStepFloatingButtons.Stop");
            m_stepButton = Children.Find<ButtonWidget>("GVStepFloatingButtons.Step");
            m_jumpButton = Children.Find<ButtonWidget>("GVStepFloatingButtons.Jump");
            m_label = Children.Find<LabelWidget>("GVStepFloatingButtons.Label");
            m_subsystem = subsystem;
        }

        public override void Update() {
            m_label.Text = (DateTime.Now - m_subsystem.last1000Updates.Peek()).TotalSeconds.ToString("f2");
            if (m_stopButton.IsClicked) {
                m_subsystem.debugMode = !m_subsystem.debugMode;
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
        }
    }
}