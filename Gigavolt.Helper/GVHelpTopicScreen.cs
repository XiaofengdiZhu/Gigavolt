using System;
using System.Xml.Linq;
using Engine;
using Engine.Graphics;
using Engine.Media;

namespace Game {
    public class GVHelpTopicScreen : Screen {
        public readonly BevelledButtonWidget m_urlButton;
        public readonly LabelWidget m_urlLabel;
        public readonly RectangleWidget m_imageWidget;
        public readonly ScrollPanelWidget m_scrollPanel;
        public readonly BevelledButtonWidget m_copyButton;
        public readonly ButtonWidget m_backButton;

        public GVHelpTopicScreen() {
            XElement node = ContentManager.Get<XElement>("Screens/GVHelpTopicScreen");
            LoadContents(this, node);
            m_urlButton = Children.Find<BevelledButtonWidget>("GVHelpTopicScreen.UrlButton");
            m_urlLabel = Children.Find<LabelWidget>("GVHelpTopicScreen.UrlLabel");
            m_imageWidget = Children.Find<RectangleWidget>("GVHelpTopicScreen.Image");
            m_scrollPanel = Children.Find<ScrollPanelWidget>("ScrollPanel");
            m_copyButton = Children.Find<BevelledButtonWidget>("GVHelpTopicScreen.Copy");
            m_backButton = Children.Find<ButtonWidget>("TopBar.Back");
        }

        public override void Enter(object[] parameters) {
            try {
                string url = $"https://github.com/XiaofengdiZhu/Gigavolt#{(string)parameters[0]}";
                m_urlLabel.Text = url;
                Image image = ContentManager.Get<Image>($"GVHelperImages/{(string)parameters[1]}");
                m_imageWidget.Subtexture = new Subtexture(Texture2D.Load(image), Vector2.Zero, Vector2.One);
                float width = RootWidget.ActualSize.X - 64;
                m_imageWidget.Size = new Vector2(width, width / image.Width * image.Height);
                m_scrollPanel.ScrollPosition = 0f;
            }
            catch (Exception e) {
                Log.Error(e);
            }
        }

        public override void Update() {
            if (m_copyButton.IsClicked) {
                ClipboardManager.ClipboardString = m_urlLabel.Text;
            }
            if (m_urlButton.IsClicked) {
                WebBrowserManager.LaunchBrowser(m_urlLabel.Text);
            }
            if (Input.Back
                || Input.Cancel
                || m_backButton.IsClicked) {
                ScreensManager.SwitchScreen(ScreensManager.PreviousScreen);
            }
        }
    }
}