using Engine;
using GameEntitySystem;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace Game
{
    public class EditGVNesEmulatorDialog : Dialog
    {
        public Action m_handler;

        public ButtonWidget m_okButton;

        public ButtonWidget m_cancelButton;

        public TextBoxWidget m_romPathTextBox;

        public EditGVNesEmulatorDialogData m_blockData;

        public SubsystemNesEmulatorBlockBehavior m_subsystem;

        public string m_lastRomPath;

        public EditGVNesEmulatorDialog(EditGVNesEmulatorDialogData blockData,SubsystemNesEmulatorBlockBehavior subsystem, Action handler)
        {
            XElement node = ContentManager.Get<XElement>("Dialogs/EditGVNesEmulatorDialog");
            LoadContents(this, node);
            m_okButton = Children.Find<ButtonWidget>("EditGVNesEmulatorDialog.OK");
            m_cancelButton = Children.Find<ButtonWidget>("EditGVNesEmulatorDialog.Cancel");
            m_romPathTextBox = Children.Find<TextBoxWidget>("EditGVNesEmulatorDialog.RomPath");
            m_romPathTextBox.Text = blockData.Data;
            m_lastRomPath = blockData.Data;
            m_handler = handler;
            m_blockData = blockData;
            m_subsystem = subsystem;
        }

        public override void Update()
        {
            if (m_okButton.IsClicked)
            {
                if (m_romPathTextBox.Text.Length > 0)
                {
                    if (m_romPathTextBox.Text == m_lastRomPath)
                    {
                        Dismiss(false);
                    }
                    else
                    {
                        try
                        {
                            byte[] bytes;
                            if (m_romPathTextBox.Text == "nestest")
                            {
                                bytes = m_subsystem.GetByteFromStream(Assembly.GetExecutingAssembly().GetManifestResourceStream("Gigavolt.Expand.NesEmulator.nestest.nes"));
                            }
                            else if (GVStaticStorage.GVMBIDDataDictionary.TryGetValue(uint.Parse(m_romPathTextBox.Text, System.Globalization.NumberStyles.HexNumber, null),out GVMemoryBankData data))
                            {
                                bytes = GVMemoryBankData.Image2Bytes(data.Data);
                            }
                            else
                            {
                                bytes = m_subsystem.GetByteFromStream(Storage.OpenFile(m_romPathTextBox.Text, OpenFileMode.Read));
                            }
                            m_subsystem._emu._cartridge.LoadROM(bytes);
                            m_blockData.Data = m_romPathTextBox.Text;
                            m_blockData.SaveString();
                            Dismiss(true);
                        }
                        catch (Exception ex)
                        {
                            DialogsManager.ShowDialog(null, new MessageDialog("发生错误", ex.ToString(), "OK", null, null));
                        }
                    }
                }
                else
                {
                    DialogsManager.ShowDialog(null, new MessageDialog("发生错误", "Rom路径未填写", "OK", null, null));
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