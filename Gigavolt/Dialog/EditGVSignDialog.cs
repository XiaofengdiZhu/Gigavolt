using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using Engine;

namespace Game {
    public class EditGVSignDialog : Dialog {
        public SubsystemGVSignBlockBehavior m_subsystemSignBlockBehavior;
        public Point3 m_signPoint;

        public readonly ContainerWidget m_linesPage;
        public readonly ContainerWidget m_urlPage;
        public readonly StackPanelWidget m_convertedStackPanel;
        public readonly TextBoxWidget m_textBox1;
        public readonly TextBoxWidget m_convertedTextBox;
        public readonly ButtonWidget m_colorButton1;
        public readonly TextBoxWidget m_urlTextBox;
        public readonly ButtonWidget m_urlTestButton;
        public readonly ButtonWidget m_okButton;
        public readonly ButtonWidget m_cancelButton;
        public readonly ButtonWidget m_urlButton;
        public readonly ButtonWidget m_linesButton;
        public readonly ButtonWidget m_copyButton;
        public readonly ButtonWidget m_convertButton;

        public Color[] m_colors = [
            new Color(0, 0, 0),
            new Color(140, 0, 0),
            new Color(0, 112, 0),
            new Color(0, 0, 96),
            new Color(160, 0, 128),
            new Color(0, 112, 112),
            new Color(160, 112, 0),
            new Color(180, 180, 180),
            Color.White
        ];

        public EditGVSignDialog(SubsystemGVSignBlockBehavior subsystemSignBlockBehavior, Point3 signPoint) {
            XElement node = ContentManager.Get<XElement>("Dialogs/EditGVSignDialog");
            LoadContents(this, node);
            m_linesPage = Children.Find<ContainerWidget>("EditGVSignDialog.LinesPage");
            m_urlPage = Children.Find<ContainerWidget>("EditGVSignDialog.UrlPage");
            m_convertedStackPanel = Children.Find<StackPanelWidget>("EditGVSignDialog.ConvertedStackPanel");
            m_textBox1 = Children.Find<TextBoxWidget>("EditGVSignDialog.TextBox1");
            m_convertedTextBox = Children.Find<TextBoxWidget>("EditGVSignDialog.ConvertedTextBox");
            m_colorButton1 = Children.Find<ButtonWidget>("EditGVSignDialog.ColorButton1");
            m_urlTextBox = Children.Find<TextBoxWidget>("EditGVSignDialog.UrlTextBox");
            m_urlTestButton = Children.Find<ButtonWidget>("EditGVSignDialog.UrlTestButton");
            m_okButton = Children.Find<ButtonWidget>("EditGVSignDialog.OkButton");
            m_cancelButton = Children.Find<ButtonWidget>("EditGVSignDialog.CancelButton");
            m_urlButton = Children.Find<ButtonWidget>("EditGVSignDialog.UrlButton");
            m_linesButton = Children.Find<ButtonWidget>("EditGVSignDialog.LinesButton");
            m_copyButton = Children.Find<ButtonWidget>("EditGVSignDialog.CopyButton");
            m_convertButton = Children.Find<ButtonWidget>("EditGVSignDialog.ConvertButton");
            m_subsystemSignBlockBehavior = subsystemSignBlockBehavior;
            m_signPoint = signPoint;
            GVSignTextData signData = m_subsystemSignBlockBehavior.GetSignData(m_signPoint, 0);
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
                m_subsystemSignBlockBehavior.SetSignData(
                    m_signPoint,
                    0,
                    line,
                    color,
                    m_urlTextBox.Text
                );
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
            if (m_convertButton.IsClicked) {
                string line = m_textBox1.Text;
                if (!string.IsNullOrEmpty(line)) {
                    m_convertedTextBox.Text = string2UintHexString(line);
                    m_convertedStackPanel.IsVisible = true;
                }
            }
            if (m_copyButton.IsClicked) {
                ClipboardManager.ClipboardString = m_convertedTextBox.Text;
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

        public static string string2UintHexString(string input) {
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            List<uint> uints = [];
            int i;
            for (i = 0; i < bytes.Length / 4; i++) {
                uints.Add(bytes[i * 4 + 3] | ((uint)bytes[i * 4 + 2] << 8) | ((uint)bytes[i * 4 + 1] << 16) | ((uint)bytes[i * 4] << 24));
            }
            int mod = bytes.Length % 4;
            if (mod > 0) {
                uint num = 0u;
                if (mod == 3) {
                    num |= (uint)bytes[i * 4 + 2] << 8;
                }
                if (mod >= 2) {
                    num |= (uint)bytes[i * 4 + 1] << 16;
                }
                num |= (uint)bytes[i * 4] << 24;
                uints.Add(num);
            }
            StringBuilder builder = new();
            for (i = 0; i < uints.Count; i++) {
                builder.Append(uints[i].ToString("X"));
                if (i < uints.Count - 1) {
                    builder.Append(',');
                }
            }
            return builder.ToString();
        }
    }
}