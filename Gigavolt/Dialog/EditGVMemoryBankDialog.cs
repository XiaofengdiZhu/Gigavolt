using Engine;
using Engine.Media;
using System;
using System.Xml.Linq;

namespace Game
{
    public class EditGVMemoryBankDialog : Dialog
    {
        public Action m_handler;

        public ButtonWidget m_okButton;

        public ButtonWidget m_cancelButton;

        public TextBoxWidget m_linearTextBox;
        public TextBoxWidget m_rowCountTextBox;
        public TextBoxWidget m_colCountTextBox;
        public LabelWidget m_imageLocationLabel;

        public GVMemoryBankData m_memoryBankData;

        public EditGVMemoryBankDialog(GVMemoryBankData memoryBankData, Action handler)
        {
            XElement node = ContentManager.Get<XElement>("Dialogs/EditGVMemoryBankDialog");
            LoadContents(this, node);
            m_okButton = Children.Find<ButtonWidget>("EditGVMemoryBankDialog.OK");
            m_cancelButton = Children.Find<ButtonWidget>("EditGVMemoryBankDialog.Cancel");
            m_linearTextBox = Children.Find<TextBoxWidget>("EditGVMemoryBankDialog.LinearText");
            m_rowCountTextBox = Children.Find<TextBoxWidget>("EditGVMemoryBankDialog.RowCount");
            m_colCountTextBox = Children.Find<TextBoxWidget>("EditGVMemoryBankDialog.ColCount");
            m_imageLocationLabel = Children.Find<LabelWidget>("EditGVMemoryBankDialog.ImageLocation");
            m_imageLocationLabel.Text += $"{memoryBankData.m_worldDirectory}/GVMB/{memoryBankData.m_guid}.png";
            m_handler = handler;
            m_memoryBankData = memoryBankData;
            if (!(memoryBankData.Data == null))
            {
                m_rowCountTextBox.IsEnabled = false;
                m_rowCountTextBox.Text = memoryBankData.Data.Height.ToString();
                m_colCountTextBox.IsEnabled = false;
                m_colCountTextBox.Text = memoryBankData.Data.Width.ToString();
                if (memoryBankData.Data.Pixels.LongLength > 1000000)
                {
                    m_linearTextBox.Text = "已保存的数据过多，请本地编辑";
                    m_linearTextBox.IsEnabled = false;
                    m_okButton.IsEnabled= false;
                }
                else
                {
                    m_linearTextBox.Text = GVMemoryBankData.Image2String(memoryBankData.Data);
                }
            }
        }

        public override void Update()
        {
            if (m_okButton.IsClicked)
            {
                int width = 0;
                int height = 0;
                int.TryParse(m_colCountTextBox.Text, out width);
                int.TryParse(m_rowCountTextBox.Text, out height);
                Image image = null;
                try
                {
                    image = GVMemoryBankData.String2Image(m_linearTextBox.Text, width, height);
                }
                catch (Exception ex) { Log.Error(ex); }
                if (image != null) { m_memoryBankData.Data = image; }
                m_memoryBankData.SaveString();
                Dismiss(result: true);
            }
            if (Input.Cancel || m_cancelButton.IsClicked)
            {
                Dismiss(result: false);
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