using Engine;
using GameEntitySystem;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Game
{
    public class EditGVDebugDialog : Dialog
    {
        public Action m_handler;

        public ButtonWidget m_okButton;

        public ButtonWidget m_cancelButton;

        public TextBoxWidget m_speedTextBox;
        public CheckboxWidget m_displayStepFloatingButtonsCheckbox;
        public CheckboxWidget m_keyboardControlCheckbox;

        public GVDebugData m_blockData;

        public SubsystemGVElectricity m_subsystem;

        public string m_lastSpeedText;

        public Dictionary<ComponentPlayer, GVStepFloatingButtons> m_buttonsDictionary = new Dictionary<ComponentPlayer, GVStepFloatingButtons>();

        public EditGVDebugDialog(GVDebugData blockData, SubsystemGVElectricity subsystem, Action handler)
        {
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
        }

        public override void Update()
        {
            if (m_okButton.IsClicked)
            {
                if (m_speedTextBox.Text.Length > 0)
                {
                    if (m_speedTextBox.Text == m_lastSpeedText)
                    {
                        Dismiss(false);
                    }
                    else
                    {
                        if(float.TryParse(m_speedTextBox.Text, out float newSpeed))
                        {
                            m_subsystem.SetSpeed(newSpeed);
                            m_blockData.Data = m_speedTextBox.Text;
                            m_blockData.SaveString();
                            Dismiss(true);
                        }
                        else
                        {
                            DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Error, "转换为float失败", "OK", null, null));
                        }
                    }
                }
                else
                {
                    DialogsManager.ShowDialog(null, new MessageDialog(LanguageControl.Error, "不能为空", "OK", null, null));
                }
            }
            if(m_displayStepFloatingButtonsCheckbox.IsClicked)
            {
                if(m_displayStepFloatingButtonsCheckbox.IsChecked)
                {
                    foreach (KeyValuePair<ComponentPlayer, GVStepFloatingButtons> pair in m_buttonsDictionary)
                    {
                        pair.Key.GameWidget.GuiWidget.RemoveChildren(pair.Value);
                    }
                    m_buttonsDictionary.Clear();
                    m_displayStepFloatingButtonsCheckbox.IsChecked = false;
                }
                else
                {
                    foreach (ComponentPlayer componentPlayer in m_subsystem.Project.FindSubsystem<SubsystemPlayers>(throwOnError: true).ComponentPlayers)
                    {
                        GVStepFloatingButtons buttons = new GVStepFloatingButtons(m_subsystem);
                        m_buttonsDictionary.Add(componentPlayer, buttons);
                        componentPlayer.GameWidget.GuiWidget.AddChildren(buttons);
                    }
                    m_displayStepFloatingButtonsCheckbox.IsChecked = true;
                }
            }
            if (m_keyboardControlCheckbox.IsClicked)
            {
                if (m_keyboardControlCheckbox.IsChecked)
                {
                    m_subsystem.keyboardDebug = false;
                    m_keyboardControlCheckbox.IsChecked = false;
                }
                else
                {
                    m_subsystem.keyboardDebug = true;
                    m_keyboardControlCheckbox.IsChecked = true;
                }
            }
            if (base.Input.Cancel || m_cancelButton.IsClicked)
            {
                Dismiss(false);
            }
        }

        public void Dismiss(bool result)
        {
            DialogsManager.HideDialog(this);
            if (m_handler != null && result)
            {
                m_handler();
            }
        }
    }
}