using System;
using System.Xml.Linq;

namespace Game {
    public class EditGVCopperHammerDialog : Dialog {
        public readonly SliderWidget m_slider;
        public readonly CheckboxWidget m_checkbox;
        public readonly BlockIconWidget m_icon;
        public readonly ButtonWidget m_okButton;
        public readonly ButtonWidget m_cancelButton;

        public int m_texture;
        public bool m_isHarness;
        public readonly Action<int, bool> m_handler;

        public readonly int[] m_textureIds = [
            GVBlocksManager.GetBlockIndex<GVWireThroughPlanksBlock>(),
            GVBlocksManager.GetBlockIndex<GVWireThroughStoneBlock>(),
            GVBlocksManager.GetBlockIndex<GVWireThroughBricksBlock>(),
            GVBlocksManager.GetBlockIndex<GVWireThroughCobblestoneBlock>()
        ];

        public static readonly string[] m_textureNames = ["木板", "石头", "砖头", "鹅卵石"];

        public EditGVCopperHammerDialog(int texture, bool isHarness, Action<int, bool> handler) {
            XElement node = ContentManager.Get<XElement>("Dialogs/EditGVCopperHammerDialog");
            LoadContents(this, node);
            m_slider = Children.Find<SliderWidget>("EditGVCopperHammerDialog.Slider");
            m_slider.Value = texture;
            m_slider.Text = m_textureNames[texture];
            m_checkbox = Children.Find<CheckboxWidget>("EditGVCopperHammerDialog.Checkbox");
            m_checkbox.IsChecked = isHarness;
            m_icon = Children.Find<BlockIconWidget>("EditGVCopperHammerDialog.Icon");
            m_icon.Value = GetValue(texture, isHarness);
            m_okButton = Children.Find<ButtonWidget>("EditGVCopperHammerDialog.OK");
            m_cancelButton = Children.Find<ButtonWidget>("EditGVCopperHammerDialog.Cancel");
            m_texture = texture;
            m_isHarness = isHarness;
            m_handler = handler;
        }

        public override void Update() {
            int sliderValue = (int)m_slider.Value;
            if (sliderValue != m_texture) {
                m_texture = sliderValue;
                m_slider.Text = m_textureNames[m_texture];
                m_icon.Value = GetValue(m_texture, m_isHarness);
            }
            if (m_checkbox.IsChecked != m_isHarness) {
                m_isHarness = m_checkbox.IsChecked;
                m_icon.Value = GetValue(m_texture, m_isHarness);
            }
            if (m_okButton.IsClicked) {
                Dismiss(m_texture, m_isHarness);
            }
            if (Input.Cancel
                || m_cancelButton.IsClicked) {
                Dismiss(null);
            }
        }

        public void Dismiss(int? texture, bool isHarness = false) {
            DialogsManager.HideDialog(this);
            if (m_handler != null
                && texture != null) {
                m_handler(texture.Value, isHarness);
            }
        }

        public int GetValue(int texture, bool isHarness) => isHarness
            ? Terrain.MakeBlockValue(
                GVBlocksManager.GetBlockIndex<GVEWireThroughBlock>(),
                0,
                GVEWireThroughBlock.SetIsWireHarness(GVEWireThroughBlock.SetTexture(GVEWireThroughBlock.SetWireFacesBitmask(0, 5), texture), true)
            )
            : m_textureIds[texture];
    }
}