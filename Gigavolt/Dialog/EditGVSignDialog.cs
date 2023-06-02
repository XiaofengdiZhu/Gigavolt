using System.Xml.Linq;
using Engine;

namespace Game {
    public class EditGVSignDialog : Dialog {
        public SubsystemGVSignBlockBehavior m_subsystemSignBlockBehavior;

        public Point3 m_signPoint;

        public ContainerWidget m_linesPage;

        public ContainerWidget m_urlPage;

        public TextBoxWidget m_textBox1;

        public ButtonWidget m_colorButton1;

        public TextBoxWidget m_urlTextBox;

        public ButtonWidget m_urlTestButton;

        public ButtonWidget m_okButton;

        public ButtonWidget m_cancelButton;

        public ButtonWidget m_urlButton;

        public ButtonWidget m_linesButton;

        public Color[] m_colors = { new Color(0, 0, 0), new Color(140, 0, 0), new Color(0, 112, 0), new Color(0, 0, 96), new Color(160, 0, 128), new Color(0, 112, 112), new Color(160, 112, 0), new Color(180, 180, 180), Color.White };

        public EditGVSignDialog(SubsystemGVSignBlockBehavior subsystemSignBlockBehavior, Point3 signPoint) {
            XElement node = ContentManager.Get<XElement>("Dialogs/EditGVSignDialog");
            LoadContents(this, node);
            m_linesPage = Children.Find<ContainerWidget>("EditGVSignDialog.LinesPage");
            m_urlPage = Children.Find<ContainerWidget>("EditGVSignDialog.UrlPage");
            m_textBox1 = Children.Find<TextBoxWidget>("EditGVSignDialog.TextBox1");
            m_colorButton1 = Children.Find<ButtonWidget>("EditGVSignDialog.ColorButton1");
            m_urlTextBox = Children.Find<TextBoxWidget>("EditGVSignDialog.UrlTextBox");
            m_urlTestButton = Children.Find<ButtonWidget>("EditGVSignDialog.UrlTestButton");
            m_okButton = Children.Find<ButtonWidget>("EditGVSignDialog.OkButton");
            m_cancelButton = Children.Find<ButtonWidget>("EditGVSignDialog.CancelButton");
            m_urlButton = Children.Find<ButtonWidget>("EditGVSignDialog.UrlButton");
            m_linesButton = Children.Find<ButtonWidget>("EditGVSignDialog.LinesButton");
            m_subsystemSignBlockBehavior = subsystemSignBlockBehavior;
            m_signPoint = signPoint;
            GVSignTextData signData = m_subsystemSignBlockBehavior.GetSignData(m_signPoint);
            if (signData != null) {
                m_textBox1.Text = signData.Line;
                m_colorButton1.Color = signData.Color;
                m_urlTextBox.Text = signData.Url;
            }
            else {
                m_textBox1.Text = string.Empty;
                m_colorButton1.Color = Color.White;
                m_urlTextBox.Text = string.Empty;
            }
            m_linesPage.IsVisible = true;
            m_urlPage.IsVisible = false;
            UpdateControls();
        }

        public override void Update() {
            UpdateControls();
            if (m_okButton.IsClicked) {
                string line = m_textBox1.Text;
                Color color = m_colorButton1.Color;
                m_subsystemSignBlockBehavior.SetSignData(m_signPoint, line, color, m_urlTextBox.Text);
                Dismiss();
            }
            if (m_urlButton.IsClicked) {
                m_urlPage.IsVisible = true;
                m_linesPage.IsVisible = false;
            }
            if (m_linesButton.IsClicked) {
                m_urlPage.IsVisible = false;
                m_linesPage.IsVisible = true;
            }
            if (m_urlTestButton.IsClicked) {
                WebBrowserManager.LaunchBrowser(m_urlTextBox.Text);
            }
            if (m_colorButton1.IsClicked) {
                m_colorButton1.Color = m_colors[(m_colors.FirstIndex(m_colorButton1.Color) + 1) % m_colors.Length];
            }
            if (Input.Cancel
                || m_cancelButton.IsClicked) {
                Dismiss();
            }
        }

        public void UpdateControls() {
            bool flag = !string.IsNullOrEmpty(m_urlTextBox.Text);
            m_urlButton.IsVisible = m_linesPage.IsVisible;
            m_linesButton.IsVisible = !m_linesPage.IsVisible;
            m_colorButton1.IsEnabled = !flag;
            m_urlTestButton.IsEnabled = flag;
        }

        public void Dismiss() {
            DialogsManager.HideDialog(this);
        }
    }
}