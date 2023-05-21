using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace Game {
    public class EditGVDebugDialog : Dialog {
        public Action m_handler;

        public ButtonWidget m_okButton;

        public ButtonWidget m_cancelButton;

        public TextBoxWidget m_speedTextBox;
        public CheckboxWidget m_displayStepFloatingButtonsCheckbox;
        public CheckboxWidget m_keyboardControlCheckbox;

        public GVDebugData m_blockData;

        public SubsystemGVElectricity m_subsystem;

        public string m_lastSpeedText;

        public EditGVDebugDialog(GVDebugData blockData, SubsystemGVElectricity subsystem, Action handler) {
            XElement node = ContentManager.Get<XElement>("Dialogs/EditGVDebugDialog");
            LoadContents(this, node);
            m_okButton = Children.Find<ButtonWidget>("EditGVDebugDialog.OK");
            m_cancelButton = Children.Find<ButtonWidget>("EditGVDebugDialog.Cancel");
            m_speedTextBox = Children.Find<TextBoxWidget>("EditGVDebugDialog.Speed");
            m_displayStepFloatingButtonsCheckbox = Children.Find<CheckboxWidget>("EditGVDebugDialog.DisplayStepFloatingButtons");
            m_keyboardControlCheckbox = Children.Find<CheckboxWidget>("EditGVDebugDialog.KeyboardControl");
            m_speedTextBox.Text = blockData.Data;
            m_lastSpeedText = blockData.Data;
            m_handler = handler;
            m_blockData = blockData;
            m_subsystem = subsystem;
            m_displayStepFloatingButtonsCheckbox.IsChecked = m_subsystem.m_debugButtonsDictionary.Count > 0;
            m_keyboardControlCheckbox.IsChecked = m_subsystem.keyboardDebug;
        }

        public override void Update() {
            if (m_displayStepFloatingButtonsCheckbox.IsClicked) {
                m_displayStepFloatingButtonsCheckbox.IsChecked = !m_displayStepFloatingButtonsCheckbox.IsChecked;
            }
            if (m_keyboardControlCheckbox.IsClicked) {
                m_keyboardControlCheckbox.IsChecked = !m_keyboardControlCheckbox.IsChecked;
            }
            if (m_okButton.IsClicked) {
                if (m_displayStepFloatingButtonsCheckbox.IsChecked) {
                    if (m_subsystem.m_debugButtonsDictionary.Count == 0) {
                        foreach (ComponentPlayer componentPlayer in m_subsystem.Project.FindSubsystem<SubsystemPlayers>(true).ComponentPlayers) {
                            GVStepFloatingButtons buttons = new GVStepFloatingButtons(m_subsystem);
                            m_subsystem.m_debugButtonsDictionary.Add(componentPlayer, buttons);
                            componentPlayer.GameWidget.GuiWidget.AddChildren(buttons);
                        }
                    }
                }
                else {
                    if (m_subsystem.m_debugButtonsDictionary.Count > 0) {
                        foreach (KeyValuePair<ComponentPlayer, GVStepFloatingButtons> pair in m_subsystem.m_debugButtonsDictionary) {
                            pair.Key.GameWidget.GuiWidget.RemoveChildren(pair.Value);
                        }
                        m_subsystem.m_debugButtonsDictionary.Clear();
                    }
                }
                m_subsystem.keyboardDebug = m_keyboardControlCheckbox.IsChecked;
                if (m_speedTextBox.Text.Length > 0) {
                    if (m_speedTextBox.Text == m_lastSpeedText) {
                        Dismiss(false);
                    }
                    else {
                        if (float.TryParse(m_speedTextBox.Text, out float newSpeed)) {
                            m_subsystem.SetSpeed(newSpeed);
                            m_blockData.Data = m_speedTextBox.Text;
                            m_blockData.SaveString();
                            Dismiss(true);
                        }
                        else {
                            DialogsManager.ShowDialog(
                                null,
                                new MessageDialog(
                                    LanguageControl.Error,
                                    "速率转换为浮点数失败",
                                    "OK",
                                    null,
                                    null
                                )
                            );
                        }
                    }
                }
                else {
                    DialogsManager.ShowDialog(
                        null,
                        new MessageDialog(
                            LanguageControl.Error,
                            "速率不能为空",
                            "OK",
                            null,
                            null
                        )
                    );
                }
                Dismiss(false);
            }
            if (Input.Cancel
                || m_cancelButton.IsClicked) {
                Dismiss(false);
            }
        }

        public void Dismiss(bool result) {
            DialogsManager.HideDialog(this);
            if (m_handler != null && result) {
                m_handler();
            }
        }
    }
}