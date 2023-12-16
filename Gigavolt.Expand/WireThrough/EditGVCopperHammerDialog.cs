using System;
using System.Xml.Linq;

namespace Game {
    public class EditGVCopperHammerDialog : Dialog {
        public readonly SliderWidget m_slider;
        public readonly BlockIconWidget m_icon;
        public readonly ButtonWidget m_okButton;
        public readonly ButtonWidget m_cancelButton;

        public int m_type;
        public readonly Action<int> m_handler;

        public readonly int[] m_typeIds = { GVWireThroughPlanksBlock.Index, GVWireThroughStoneBlock.Index, GVWireThroughBricksBlock.Index, GVWireThroughCobblestoneBlock.Index };
        public readonly string[] m_typeNames = { "木板", "石头", "砖头", "鹅卵石" };

        public EditGVCopperHammerDialog(int type, Action<int> handler) {
            XElement node = ContentManager.Get<XElement>("Dialogs/EditGVCopperHammerDialog");
            LoadContents(this, node);
            m_slider = Children.Find<SliderWidget>("EditGVCopperHammerDialog.Slider");
            m_slider.Value = type;
            m_slider.Text = m_typeNames[type];
            m_icon = Children.Find<BlockIconWidget>("EditGVCopperHammerDialog.Icon");
            m_icon.Contents = m_typeIds[type];
            m_okButton = Children.Find<ButtonWidget>("EditGVCopperHammerDialog.OK");
            m_cancelButton = Children.Find<ButtonWidget>("EditGVCopperHammerDialog.Cancel");
            m_type = type;
            m_handler = handler;
        }

        public override void Update() {
            int sliderValue = (int)m_slider.Value;
            if (sliderValue != m_type) {
                m_type = sliderValue;
                m_slider.Text = m_typeNames[m_type];
                m_icon.Contents = m_typeIds[m_type];
            }
            if (m_okButton.IsClicked) {
                Dismiss(m_type);
            }
            if (Input.Cancel
                || m_cancelButton.IsClicked) {
                Dismiss(null);
            }
        }

        public void Dismiss(int? result) {
            DialogsManager.HideDialog(this);
            if (m_handler != null
                && result != null) {
                m_handler(result.Value);
            }
        }
    }
}