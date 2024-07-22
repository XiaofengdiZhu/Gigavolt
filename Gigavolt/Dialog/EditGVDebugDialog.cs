using System;
using System.Xml.Linq;

namespace Game {
    public class EditGVDebugDialog : Dialog {
        public readonly Action m_handler;

        public readonly ButtonWidget m_okButton;

        public readonly ButtonWidget m_cancelButton;

        public readonly TextBoxWidget m_speedTextBox;
        public readonly CheckboxWidget m_displayStepFloatingButtonsCheckbox;
        public readonly CheckboxWidget m_keyboardControlCheckbox;
        public readonly CheckboxWidget m_preventChunkFromBeingFreeCheckbox;
        public readonly CheckboxWidget m_displayVoltageCheckbox;
        public readonly CheckboxWidget m_wheelPanelEnabledCheckbox;

        public readonly StackPanelWidget m_GVHelperPanel;
        public readonly CheckboxWidget m_GVHelperSlotActiveCheckbox;

        public readonly SubsystemGVDebugBlockBehavior m_subsystem;

        public readonly SubsystemGVElectricity m_subsystemGVElectricity;

        public readonly string m_lastSpeedText;

        public EditGVDebugDialog(SubsystemGVDebugBlockBehavior subsystem, SubsystemGVElectricity subsystemGVElectricity, Action handler) {
            XElement node = ContentManager.Get<XElement>("Dialogs/EditGVDebugDialog");
            LoadContents(this, node);
            m_okButton = Children.Find<ButtonWidget>("EditGVDebugDialog.OK");
            m_cancelButton = Children.Find<ButtonWidget>("EditGVDebugDialog.Cancel");
            m_speedTextBox = Children.Find<TextBoxWidget>("EditGVDebugDialog.Speed");
            m_displayStepFloatingButtonsCheckbox = Children.Find<CheckboxWidget>("EditGVDebugDialog.DisplayStepFloatingButtons");
            m_keyboardControlCheckbox = Children.Find<CheckboxWidget>("EditGVDebugDialog.KeyboardControl");
            m_preventChunkFromBeingFreeCheckbox = Children.Find<CheckboxWidget>("EditGVDebugDialog.PreventChunkFromBeingFree");
            m_displayVoltageCheckbox = Children.Find<CheckboxWidget>("EditGVDebugDialog.DisplayVoltage");
            m_wheelPanelEnabledCheckbox = Children.Find<CheckboxWidget>("EditGVDebugDialog.WheelPanelEnabled");
            m_GVHelperPanel = Children.Find<StackPanelWidget>("EditGVDebugDialog.GVHelperPanel");
            m_GVHelperSlotActiveCheckbox = Children.Find<CheckboxWidget>("EditGVDebugDialog.GVHelperSlotActive");
            if (GVStaticStorage.GVHelperAvailable) {
                m_GVHelperPanel.IsVisible = true;
                m_GVHelperSlotActiveCheckbox.IsChecked = GVStaticStorage.GVHelperSlotActive;
            }
            m_handler = handler;
            m_subsystem = subsystem;
            m_subsystemGVElectricity = subsystemGVElectricity;
            GVDebugData data = subsystem.m_data;
            m_speedTextBox.Text = data.Speed.ToString("F2");
            m_lastSpeedText = data.Speed.ToString("F2");
            m_displayStepFloatingButtonsCheckbox.IsChecked = data.DisplayStepFloatingButtons;
            m_keyboardControlCheckbox.IsChecked = data.KeyboardControl;
            m_preventChunkFromBeingFreeCheckbox.IsChecked = GVStaticStorage.PreventChunkFromBeingFree;
            m_displayVoltageCheckbox.IsChecked = GVStaticStorage.DisplayVoltage;
            m_wheelPanelEnabledCheckbox.IsChecked = GVStaticStorage.WheelPanelEnabled;
        }

        public override void Update() {
            if (m_displayStepFloatingButtonsCheckbox.IsClicked) {
                m_displayStepFloatingButtonsCheckbox.IsChecked = !m_displayStepFloatingButtonsCheckbox.IsChecked;
            }
            if (m_keyboardControlCheckbox.IsClicked) {
                m_keyboardControlCheckbox.IsChecked = !m_keyboardControlCheckbox.IsChecked;
            }
            if (m_preventChunkFromBeingFreeCheckbox.IsClicked) {
                m_preventChunkFromBeingFreeCheckbox.IsChecked = !m_preventChunkFromBeingFreeCheckbox.IsChecked;
            }
            if (m_displayVoltageCheckbox.IsClicked) {
                m_displayVoltageCheckbox.IsChecked = !m_displayVoltageCheckbox.IsChecked;
            }
            if (m_wheelPanelEnabledCheckbox.IsClicked) {
                m_wheelPanelEnabledCheckbox.IsChecked = !m_wheelPanelEnabledCheckbox.IsChecked;
            }
            if (m_GVHelperSlotActiveCheckbox.IsClicked) {
                m_GVHelperSlotActiveCheckbox.IsChecked = !m_GVHelperSlotActiveCheckbox.IsChecked;
            }
            if (m_okButton.IsClicked) {
                if (m_speedTextBox.Text.Length > 0) {
                    if (m_speedTextBox.Text != m_lastSpeedText) {
                        if (float.TryParse(m_speedTextBox.Text, out float newSpeed)) {
                            if (newSpeed < 0.1f) {
                                newSpeed = 0.1f;
                                m_speedTextBox.Text = "0.10";
                            }
                            m_subsystem.SetSpeed(newSpeed);
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
                GVStaticStorage.DisplayVoltage = m_displayVoltageCheckbox.IsChecked;
                GVStaticStorage.PreventChunkFromBeingFree = m_preventChunkFromBeingFreeCheckbox.IsChecked;
                GVStaticStorage.WheelPanelEnabled = m_wheelPanelEnabledCheckbox.IsChecked;
                m_subsystem.SetDisplayStepFloatingButtons(m_displayStepFloatingButtonsCheckbox.IsChecked);
                m_subsystem.SetKeyboardDebug(m_keyboardControlCheckbox.IsChecked);
                GVStaticStorage.GVHelperSlotActive = m_GVHelperSlotActiveCheckbox.IsChecked;
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