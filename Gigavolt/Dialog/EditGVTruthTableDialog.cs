using System;
using System.Xml.Linq;

namespace Game
{
    public class EditGVTruthTableDialog : Dialog
    {
        public Action<bool> m_handler;

        public Widget m_linearPanel;

        public Widget m_gridPanel;

        public ButtonWidget m_okButton;

        public ButtonWidget m_cancelButton;

        public ButtonWidget m_switchViewButton;

        public CheckboxWidget[] m_lineCheckboxes = new CheckboxWidget[16];

        public TextBoxWidget m_linearTextBox;

        public GVTruthTableData m_truthTableData;

        public GVTruthTableData m_tmpTruthTableData;

        public bool m_ignoreTextChanges;

        public EditGVTruthTableDialog(GVTruthTableData truthTableData, Action<bool> handler)
        {
            XElement node = ContentManager.Get<XElement>("Dialogs/EditGVTruthTableDialog");
            LoadContents(this, node);
            m_linearPanel = Children.Find<Widget>("EditGVTruthTableDialog.LinearPanel");
            m_gridPanel = Children.Find<Widget>("EditGVTruthTableDialog.GridPanel");
            m_okButton = Children.Find<ButtonWidget>("EditGVTruthTableDialog.OK");
            m_cancelButton = Children.Find<ButtonWidget>("EditGVTruthTableDialog.Cancel");
            m_switchViewButton = Children.Find<ButtonWidget>("EditGVTruthTableDialog.SwitchViewButton");
            m_linearTextBox = Children.Find<TextBoxWidget>("EditGVTruthTableDialog.LinearText");
            for (int i = 0; i < 16; i++)
            {
                m_lineCheckboxes[i] = Children.Find<CheckboxWidget>("EditGVTruthTableDialog.Line" + i.ToString());
            }
            m_handler = handler;
            m_truthTableData = truthTableData;
            m_tmpTruthTableData = (GVTruthTableData)m_truthTableData.Copy();
            m_linearPanel.IsVisible = false;
            m_linearTextBox.TextChanged += delegate
            {
                if (!m_ignoreTextChanges)
                {
                    m_tmpTruthTableData = new GVTruthTableData();
                    m_tmpTruthTableData.LoadBinaryString(m_linearTextBox.Text);
                }
            };
        }

        public override void Update()
        {
            m_ignoreTextChanges = true;
            try
            {
                m_linearTextBox.Text = m_tmpTruthTableData.SaveBinaryString();
            }
            finally
            {
                m_ignoreTextChanges = false;
            }
            for (int i = 0; i < 16; i++)
            {
                if (m_lineCheckboxes[i].IsClicked)
                {
                    m_tmpTruthTableData.Data[i] = (byte)((m_tmpTruthTableData.Data[i] == 0) ? 15 : 0);
                }
                m_lineCheckboxes[i].IsChecked = (m_tmpTruthTableData.Data[i] > 0);
            }
            if (m_linearPanel.IsVisible)
            {
                m_switchViewButton.Text = LanguageControl.Get(GetType().Name,1);
                if (m_switchViewButton.IsClicked)
                {
                    m_linearPanel.IsVisible = false;
                    m_gridPanel.IsVisible = true;
                }
            }
            else
            {
                m_switchViewButton.Text = LanguageControl.Get(GetType().Name, 2);
                if (m_switchViewButton.IsClicked)
                {
                    m_linearPanel.IsVisible = true;
                    m_gridPanel.IsVisible = false;
                }
            }
            if (m_okButton.IsClicked)
            {
                m_truthTableData.Data = m_tmpTruthTableData.Data;
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
            m_handler?.Invoke(result);
        }
    }
}
