using System;
using System.Xml.Linq;
using Engine;
using Engine.Input;

namespace Game {
    public class GVStepFloatingButtons : CanvasWidget {
        public readonly ButtonWidget m_pauseButton;
        public readonly ButtonWidget m_stepButton;
        public readonly ButtonWidget m_jumpButton;
        public readonly LabelWidget m_label;
        public readonly RectangleWidget m_pauseIcon;
        public readonly Subtexture m_continueSubtexture;
        public readonly Subtexture m_pauseSubtexture;
        public readonly SubsystemGVElectricity m_subsystem;

        public GVStepFloatingButtons(SubsystemGVElectricity subsystem) {
            XElement node = ContentManager.Get<XElement>("Widgets/GVStepFloatingButtons");
            LoadContents(this, node);
            m_pauseButton = Children.Find<ButtonWidget>("GVStepFloatingButtons.Pause");
            m_stepButton = Children.Find<ButtonWidget>("GVStepFloatingButtons.Step");
            m_jumpButton = Children.Find<ButtonWidget>("GVStepFloatingButtons.Jump");
            m_label = Children.Find<LabelWidget>("GVStepFloatingButtons.Label");
            m_pauseIcon = Children.Find<RectangleWidget>("GVStepFloatingButtons.PauseIcon");
            m_continueSubtexture = ContentManager.Get<Subtexture>("Textures/Gui/GVContinue");
            m_pauseSubtexture = ContentManager.Get<Subtexture>("Textures/Gui/GVPause");
            m_subsystem = subsystem;
            m_pauseIcon.Subtexture = m_subsystem.debugMode ? m_continueSubtexture : m_pauseSubtexture;
        }

        public override void Update() {
            m_pauseButton.IsChecked = false;
            m_stepButton.IsChecked = false;
            m_jumpButton.IsChecked = false;
            if (m_pauseButton.IsClicked) {
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
            if (m_subsystem.keyboardDebug) {
                if (Keyboard.IsKeyDownOnce(Key.F5)) {
                    m_pauseButton.IsChecked = true;
                }
                if (m_subsystem.debugMode) {
                    if (Keyboard.IsKeyDownOnce(Key.F6)) {
                        m_stepButton.IsChecked = true;
                    }
                    else if (Keyboard.IsKeyDownOnce(Key.F7)) {
                        m_jumpButton.IsChecked = true;
                    }
                }
            }
            double time = (m_subsystem.lastUpdate
                - (m_subsystem.last1000Updates.Count > 0 ? m_subsystem.last1000Updates.Peek() : m_subsystem.lastUpdate)).TotalSeconds;
            m_label.Text = string.Format(
                LanguageControl.Get(GetType().Name, "1"),
                m_subsystem.last1000Updates.Count - 1,
                time.ToString(time < 1 ? "f4" : "f2")
            );
            m_pauseIcon.Subtexture = m_subsystem.debugMode ? m_continueSubtexture : m_pauseSubtexture;
        }
    }
}