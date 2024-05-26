using System;
using System.Xml.Linq;
using Engine;
using Engine.Graphics;
using Engine.Input;

namespace Game {
    public class GVOscilloscopeScreen : Screen {
        public GVOscilloscopeData m_data;
        public readonly ButtonWidget m_backButton;
        public readonly LabelWidget m_topBarLabel;
        public readonly LabelWidget m_topInputLabel;
        public readonly LabelWidget m_rightInputLabel;
        public readonly LabelWidget m_bottomInputLabel;
        public readonly LabelWidget m_leftInputLabel;
        public ButtonWidget m_stopButton;
        public ButtonWidget m_stepButton;
        public ButtonWidget m_jumpButton;
        public CanvasWidget m_canvas;
        public SubsystemGVOscilloscopeBlockBehavior m_blockBehavior;
        public SubsystemGVElectricity m_electricity;
        public SubsystemTime m_subsystemTime;
        RenderTarget2D m_texture;
        TexturedBatch2D m_texturedBatch2D;
        PrimitivesRenderer2D m_primitivesRenderer2D;
        public int RecordsCountAtLastGenerateTexture;
        public int RecordsCountAtLastGenerateFlatBatch3D;
        bool m_isDebugModeWhenEnter;

        public GVOscilloscopeScreen() {
            XElement node = ContentManager.Get<XElement>("Screens/GVOscilloscopeScreen");
            LoadContents(this, node);
            m_backButton = Children.Find<ButtonWidget>("TopBar.Back");
            m_topBarLabel = Children.Find<LabelWidget>("TopBar.Label");
            m_topInputLabel = Children.Find<LabelWidget>("GVOscilloscopeScreen.TopInput");
            m_rightInputLabel = Children.Find<LabelWidget>("GVOscilloscopeScreen.RightInput");
            m_bottomInputLabel = Children.Find<LabelWidget>("GVOscilloscopeScreen.BottomInput");
            m_leftInputLabel = Children.Find<LabelWidget>("GVOscilloscopeScreen.LeftInput");
            m_stopButton = Children.Find<ButtonWidget>("GVOscilloscopeScreen.Stop");
            m_stepButton = Children.Find<ButtonWidget>("GVOscilloscopeScreen.Step");
            m_jumpButton = Children.Find<ButtonWidget>("GVOscilloscopeScreen.Jump");
            m_canvas = Children.Find<CanvasWidget>("GVOscilloscopeScreen.Canvas");
        }

        public override void Enter(object[] parameters) {
            try {
                m_topBarLabel.Text = string.Format(LanguageControl.GetContentWidgets("GVOscilloscopeScreen", "1"), (Point3)parameters[0]);
                m_data = (GVOscilloscopeData)parameters[1];
                m_blockBehavior = (SubsystemGVOscilloscopeBlockBehavior)parameters[2];
                m_electricity = m_blockBehavior.Project.FindSubsystem<SubsystemGVElectricity>(true);
                m_isDebugModeWhenEnter = m_electricity.debugMode;
                m_electricity.debugMode = true;
                m_subsystemTime = m_blockBehavior.Project.FindSubsystem<SubsystemTime>(true);
                uint[] lastRecord = m_data.LastRecord;
                m_topInputLabel.Text = string.Format(LanguageControl.GetContentWidgets("GVOscilloscopeScreen", "2"), lastRecord[0].ToString("X8"));
                m_rightInputLabel.Text = string.Format(LanguageControl.GetContentWidgets("GVOscilloscopeScreen", "3"), lastRecord[1].ToString("X8"));
                m_bottomInputLabel.Text = string.Format(LanguageControl.GetContentWidgets("GVOscilloscopeScreen", "4"), lastRecord[2].ToString("X8"));
                m_leftInputLabel.Text = string.Format(LanguageControl.GetContentWidgets("GVOscilloscopeScreen", "5"), lastRecord[3].ToString("X8"));
            }
            catch (Exception e) {
                Log.Error(e);
            }
        }

        public override void Update() {
            m_stopButton.IsChecked = false;
            m_stepButton.IsChecked = false;
            m_jumpButton.IsChecked = false;
            uint[] lastRecord = m_data.LastRecord;
            m_topInputLabel.Text = string.Format(LanguageControl.GetContentWidgets("GVOscilloscopeScreen", "2"), lastRecord[0].ToString("X8"));
            m_rightInputLabel.Text = string.Format(LanguageControl.GetContentWidgets("GVOscilloscopeScreen", "3"), lastRecord[1].ToString("X8"));
            m_bottomInputLabel.Text = string.Format(LanguageControl.GetContentWidgets("GVOscilloscopeScreen", "4"), lastRecord[2].ToString("X8"));
            m_leftInputLabel.Text = string.Format(LanguageControl.GetContentWidgets("GVOscilloscopeScreen", "5"), lastRecord[3].ToString("X8"));
            if (Input.Click.HasValue) {
                Vector2 start = Input.Click.Value.Start;
                Vector2 min = m_canvas.GlobalBounds.Min;
                if (start.X > min.X
                    && start.Y > min.Y) {
                    Vector2 size = m_canvas.GlobalBounds.Max - min;
                    m_data.Interact(start - min, size.X, size.Y);
                    if (!IsTextureObsolete()) {
                        Texture?.Dispose();
                    }
                }
            }
            if (m_stopButton.IsClicked
                || Keyboard.IsKeyDownOnce(Key.F5)) {
                m_stopButton.IsChecked = true;
                m_electricity.debugMode = !m_electricity.debugMode;
                if (m_electricity.debugMode) {
                    m_electricity.last1000Updates.Clear();
                }
            }
            if (m_stepButton.IsClicked
                || Keyboard.IsKeyDownOnce(Key.F6)) {
                m_stepButton.IsChecked = true;
                if (!m_electricity.debugMode) {
                    m_electricity.debugMode = true;
                }
                try {
                    m_electricity.StepUpdate();
                }
                catch (Exception ex) {
                    Log.Error(ex);
                }
            }
            if (m_jumpButton.IsClicked
                || Keyboard.IsKeyDownOnce(Key.F7)) {
                m_jumpButton.IsChecked = true;
                if (!m_electricity.debugMode) {
                    m_electricity.debugMode = true;
                }
                try {
                    m_electricity.JumpUpdate();
                }
                catch (Exception ex) {
                    Log.Error(ex);
                }
            }
            if (!m_electricity.debugMode) {
                m_electricity.Update(Math.Clamp(m_subsystemTime.GameTimeDelta, 0f, 0.1f));
            }
            if (Input.Back
                || Input.Cancel
                || m_backButton.IsClicked) {
                m_electricity.debugMode = m_isDebugModeWhenEnter;
                m_blockBehavior.m_isInScreen = false;
                ScreensManager.SwitchScreen(ScreensManager.PreviousScreen);
            }
        }

        Vector2 m_lastParentAvailableSize = new(0f);

        public override void MeasureOverride(Vector2 parentAvailableSize) {
            IsOverdrawRequired = true;
            if (m_lastParentAvailableSize != parentAvailableSize) {
                m_lastParentAvailableSize = parentAvailableSize;
                if (!IsTextureObsolete()) {
                    Texture?.Dispose();
                }
            }
            base.MeasureOverride(parentAvailableSize);
        }

        public override void Overdraw(DrawContext dc) {
            base.Overdraw(dc);
            m_primitivesRenderer2D ??= dc.PrimitivesRenderer2D;
            Vector2 size = m_canvas.GlobalBounds.Size();
            if (size.X > 0) {
                FlatBatch2D.QueueQuad(
                    m_canvas.GlobalBounds.Min,
                    m_canvas.GlobalBounds.Max,
                    0,
                    Vector2.Zero,
                    Vector2.One,
                    Color.White
                );
            }
        }

        public RenderTarget2D Texture {
            get {
                if (m_data.RecordsCount == 0) {
                    return null;
                }
                if (IsTextureObsolete()) {
                    m_texture?.Dispose();
                    Vector2 size = m_canvas.GlobalBounds.Size();
                    m_texture = m_data.GenerateTexture(0, (int)size.X, (int)size.Y);
                    RecordsCountAtLastGenerateTexture = m_data.RecordsCount;
                }
                return m_texture;
            }
        }

        public TexturedBatch2D FlatBatch2D {
            get {
                if (m_data.RecordsCount == 0) {
                    return null;
                }
                if (m_texturedBatch2D == null
                    || m_data.RecordsCount != RecordsCountAtLastGenerateFlatBatch3D
                    || IsTextureObsolete()) {
                    m_texturedBatch2D = m_primitivesRenderer2D.TexturedBatch(
                        Texture,
                        false,
                        0,
                        DepthStencilState.None,
                        null,
                        BlendState.AlphaBlend
                    );
                    RecordsCountAtLastGenerateFlatBatch3D = m_data.RecordsCount;
                }
                return m_texturedBatch2D;
            }
        }

        public bool IsTextureObsolete() => m_texture == null || m_texture.m_isDisposed || m_data.RecordsCount != RecordsCountAtLastGenerateTexture;
    }
}