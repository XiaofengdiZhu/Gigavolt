using System;
using System.Xml.Linq;

namespace Game
{
    public class EditGVPistonDialog : Dialog
    {
        public LabelWidget m_title;

        public TextBoxWidget m_maxExtensionWidget;

        public TextBoxWidget m_pullCountWidget;

        public ContainerWidget m_panel2;

        public SliderWidget m_slider3;

        public ButtonWidget m_okButton;

        public ButtonWidget m_cancelButton;

        public Action m_handler;

        public GVPistonData m_pistonData;

        public int m_speed;

        public static string[] m_speedNames = new string[7]
        {
            "Very Slow",
            "Slow",
            "Medium",
            "Fast",
            "2xFast",
            "3xFast",
            "4xFast"
        };

        public static string[] m_speedCNNames = new string[7]
        {
            "非常慢",
            "慢",
            "中",
            "快",
            "2x快",
            "3x快",
            "4x快"
        };

        public string m_languageType;

        public EditGVPistonDialog(PistonMode mode, GVPistonData pistonData, Action handler)
        {
            XElement node = ContentManager.Get<XElement>("Dialogs/EditGVPistonDialog");
            LoadContents(this, node);
            m_title = Children.Find<LabelWidget>("EditGVPistonDialog.Title");
            m_maxExtensionWidget = Children.Find<TextBoxWidget>("EditGVPistonDialog.MaxExtension");
            m_panel2 = Children.Find<ContainerWidget>("EditGVPistonDialog.Panel2");
            m_pullCountWidget = Children.Find<TextBoxWidget>("EditGVPistonDialog.PullCount");
            m_slider3 = Children.Find<SliderWidget>("EditGVPistonDialog.Slider3");
            m_okButton = Children.Find<ButtonWidget>("EditGVPistonDialog.OK");
            m_cancelButton = Children.Find<ButtonWidget>("EditGVPistonDialog.Cancel");
            m_handler = handler;
            m_pistonData= pistonData;
            m_speed = m_pistonData.Speed;
            m_languageType = (ModsManager.Configs.ContainsKey("Language")) ? ModsManager.Configs["Language"] : "zh-CN";
            m_title.Text = GVPistonBlock.Mode2Name(mode);
            m_maxExtensionWidget.Text = (pistonData.MaxExtension + 1).ToString();
            m_pullCountWidget.Text = (pistonData.PullCount + 1).ToString();
            m_slider3.Granularity = 1f;
            m_slider3.MinValue = 0f;
            m_slider3.MaxValue = 6f;
            m_panel2.IsVisible = mode!=PistonMode.Pushing;
            UpdateControls();
        }

        public override void Update()
        {
            if (m_slider3.IsSliding)
            {
                m_speed = (int)m_slider3.Value;
            }
            if (m_okButton.IsClicked)
            {
                if(int.TryParse(m_maxExtensionWidget.Text,out int m))
                {
                    m_pistonData.MaxExtension = m - 1;
                    if(int.TryParse(m_pullCountWidget.Text, out int p)){
                        m_pistonData.PullCount = p - 1;
                        m_pistonData.Speed = m_speed;
                        m_pistonData.SaveString();
                        Dismiss(true);
                    }
                    else
                    {
                        DialogsManager.ShowDialog(null, new MessageDialog("发生错误", "最大推拉数不能转换为整数", "OK", null, null));
                    }
                }
                else
                {
                    DialogsManager.ShowDialog(null, new MessageDialog("发生错误", "最大延伸数不能转换为整数", "OK", null, null));
                }
            }
            if (Input.Cancel || m_cancelButton.IsClicked)
            {
                Dismiss(false);
            }
            UpdateControls();
        }

        public void UpdateControls()
        {
            m_slider3.Value = m_speed;
            m_slider3.Text = (m_languageType == "zh-CN") ? m_speedCNNames[m_speed] : m_speedNames[m_speed];
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
