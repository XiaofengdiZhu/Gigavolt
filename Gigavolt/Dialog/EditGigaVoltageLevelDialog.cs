using System;
using System.Xml.Linq;

namespace Game
{
    public class EditGigaVoltageLevelDialog : Dialog
    {
        public Action m_handler;

        public ButtonWidget m_okButton;

        public ButtonWidget m_cancelButton;

        public TextBoxWidget m_voltageLevelTextBox;

        public GigaVoltageLevelData m_blockData;

        public EditGigaVoltageLevelDialog(GigaVoltageLevelData blockData, Action handler)
        {
            XElement node = ContentManager.Get<XElement>("Dialogs/EditGigaVoltageLevelDialog");
            LoadContents(this, node);
            m_okButton = Children.Find<ButtonWidget>("EditGigaVoltageLevelDialog.OK");
            m_cancelButton = Children.Find<ButtonWidget>("EditGigaVoltageLevelDialog.Cancel");
            m_voltageLevelTextBox = Children.Find<TextBoxWidget>("EditGigaVoltageLevelDialog.GigaVoltageLevel");
            m_voltageLevelTextBox.Text = Convert.ToString(blockData.Data,16);
            m_handler = handler;
            m_blockData = blockData;
        }

        public override void Update()
        {
            if (m_okButton.IsClicked)
            {
                m_blockData.Data = Convert.ToUInt32(m_voltageLevelTextBox.Text,16);
                m_blockData.SaveString();
                Dismiss(true);
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