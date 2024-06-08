using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Engine;
using Engine.Graphics;
using Engine.Input;
using Color = Engine.Color;

namespace Game {
    public class GVOscilloscopeScreen : Screen {
        public GVOscilloscopeData m_data;
        public readonly ButtonWidget m_backButton;
        public readonly ButtonWidget m_topButton;
        public readonly ButtonWidget m_rightButton;
        public readonly ButtonWidget m_bottomButton;
        public readonly ButtonWidget m_leftButton;
        public readonly LabelWidget m_topBarLabel;
        public readonly LabelWidget m_topInputLabel;
        public readonly LabelWidget m_rightInputLabel;
        public readonly LabelWidget m_bottomInputLabel;
        public readonly LabelWidget m_leftInputLabel;
        public readonly ButtonWidget m_pauseButton;
        public readonly ButtonWidget m_stepButton;
        public readonly ButtonWidget m_jumpButton;
        public readonly RectangleWidget m_pauseIcon;
        public readonly Subtexture m_continueSubtexture;
        public readonly Subtexture m_pauseSubtexture;
        public readonly CanvasWidget m_canvas;
        public readonly ScrollPanelWidget m_scrollPanel;
        public readonly StackPanelWidget m_scrollStack;
        public readonly CanvasWidget m_placeholder1;
        public readonly CanvasWidget m_placeholder2;
        public SubsystemGVOscilloscopeBlockBehavior m_blockBehavior;
        public SubsystemGVElectricity m_electricity;
        public SubsystemTime m_subsystemTime;
        RenderTarget2D m_texture;
        RenderTarget2D m_cachedWaveRenderTarget;
        RenderTarget2D m_cachedBlurRenderTarget;
        RenderTarget2D m_cachedTempBlurRenderTarget;
        TexturedBatch2D m_texturedBatch2D;
        PrimitivesRenderer2D m_primitivesRenderer2D;
        public int RecordsCountAtLastGenerateTexture;
        public int RecordsCountAtLastGenerateFlatBatch3D;
        bool m_isDebugModeWhenEnter;
        bool m_displayTable;
        float m_lastScrollPosition;

        public GVOscilloscopeScreen() {
            XElement node = ContentManager.Get<XElement>("Screens/GVOscilloscopeScreen");
            LoadContents(this, node);
            m_backButton = Children.Find<ButtonWidget>("TopBar.Back");
            m_topButton = Children.Find<ButtonWidget>("GVOscilloscopeScreen.TopButton");
            m_rightButton = Children.Find<ButtonWidget>("GVOscilloscopeScreen.RightButton");
            m_bottomButton = Children.Find<ButtonWidget>("GVOscilloscopeScreen.BottomButton");
            m_leftButton = Children.Find<ButtonWidget>("GVOscilloscopeScreen.LeftButton");
            m_topBarLabel = Children.Find<LabelWidget>("TopBar.Label");
            m_topInputLabel = Children.Find<LabelWidget>("GVOscilloscopeScreen.TopInput");
            m_rightInputLabel = Children.Find<LabelWidget>("GVOscilloscopeScreen.RightInput");
            m_bottomInputLabel = Children.Find<LabelWidget>("GVOscilloscopeScreen.BottomInput");
            m_leftInputLabel = Children.Find<LabelWidget>("GVOscilloscopeScreen.LeftInput");
            m_pauseButton = Children.Find<ButtonWidget>("GVOscilloscopeScreen.Pause");
            m_stepButton = Children.Find<ButtonWidget>("GVOscilloscopeScreen.Step");
            m_jumpButton = Children.Find<ButtonWidget>("GVOscilloscopeScreen.Jump");
            m_pauseIcon = Children.Find<RectangleWidget>("GVOscilloscopeScreen.PauseIcon");
            m_continueSubtexture = ContentManager.Get<Subtexture>("Textures/Gui/GVContinue");
            m_pauseSubtexture = ContentManager.Get<Subtexture>("Textures/Gui/GVPause");
            m_canvas = Children.Find<CanvasWidget>("GVOscilloscopeScreen.Canvas");
            m_scrollPanel = Children.Find<ScrollPanelWidget>("GVOscilloscopeScreen.ScrollPanel");
            m_scrollStack = Children.Find<StackPanelWidget>("GVOscilloscopeScreen.ScrollStack");
            m_placeholder1 = Children.Find<CanvasWidget>("GVOscilloscopeScreen.Placeholder1");
            m_placeholder2 = Children.Find<CanvasWidget>("GVOscilloscopeScreen.Placeholder2");
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
                m_topInputLabel.Color = GVOscilloscopeData.WaveColor[0];
                m_rightInputLabel.Color = GVOscilloscopeData.WaveColor[1];
                m_bottomInputLabel.Color = GVOscilloscopeData.WaveColor[2];
                m_leftInputLabel.Color = GVOscilloscopeData.WaveColor[3];
                m_displayTable = false;
                m_scrollPanel.ScrollPosition = 0f;
                m_lastScrollPosition = 0f;
                m_placeholder1.Size = new Vector2(1, 0);
                m_placeholder2.Size = new Vector2(1, 0);
                m_pauseIcon.Subtexture = m_electricity.debugMode ? m_continueSubtexture : m_pauseSubtexture;
            }
            catch (Exception e) {
                Log.Error(e);
            }
        }

        public override void Update() {
            m_pauseButton.IsChecked = false;
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
                        m_texture = null;
                    }
                }
            }
            if (m_pauseButton.IsClicked
                || Keyboard.IsKeyDownOnce(Key.F5)) {
                m_pauseButton.IsChecked = true;
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
            m_pauseIcon.Subtexture = m_electricity.debugMode ? m_continueSubtexture : m_pauseSubtexture;
            if (m_topButton.IsClicked
                || m_rightButton.IsClicked
                || m_bottomButton.IsClicked
                || m_leftButton.IsClicked) {
                m_displayTable = !m_displayTable;
                if (m_displayTable) {
                    m_scrollPanel.ScrollPosition = 0f;
                    m_lastScrollPosition = 0f;
                    m_scrollStack.ClearChildren();
                    int renderCount = Math.Min((int)m_canvas.ActualSize.Y / 60 + 6, m_data.RecordsCount - 1);
                    for (int i = 1; i < renderCount + 1; i++) {
                        m_scrollStack.AddChildren(GenerateTableRow(m_data.GetLastIndexOfRecord(i), i, false));
                    }
                    int notRenderCount2 = m_data.RecordsCount - 1 - renderCount;
                    if (notRenderCount2 > 0) {
                        m_placeholder2.Size = new Vector2(1, notRenderCount2 * 60f);
                    }
                }
                else {
                    m_placeholder1.Size = new Vector2(1f, 0f);
                    m_placeholder2.Size = new Vector2(1f, 0f);
                    m_scrollStack.ClearChildren();
                }
            }
            if (m_displayTable) {
                if (m_scrollPanel.ScrollPosition != m_lastScrollPosition) {
                    m_lastScrollPosition = m_scrollPanel.ScrollPosition;
                    int notRenderCount1 = Math.Max((int)m_lastScrollPosition / 60 - 3, 1);
                    m_placeholder1.Size = new Vector2(1, notRenderCount1 * 60f);
                    int renderCount = Math.Min((int)(m_canvas.ActualSize.Y + m_lastScrollPosition) / 60 + 6 - notRenderCount1, m_data.RecordsCount - notRenderCount1);
                    Dictionary<int, Widget> renderedRows = new();
                    HashSet<Widget> toRemove = new();
                    foreach (Widget row in m_scrollStack.Children) {
                        int tag = (int)row.Tag;
                        if (tag >= notRenderCount1
                            && tag < notRenderCount1 + renderCount) {
                            renderedRows.Add(tag, row);
                            uint[] record = m_data.GetLastIndexOfRecord(tag);
                            bool isBottom = tag == m_data.RecordsCount - 1;
                            for (int i = 0; i < 4; i++) {
                                if ((row as StackPanelWidget)?.Children[i] is GVVoltageRectangleWidget rectangle) {
                                    rectangle.Text = $"{record[i]:X8} V";
                                    rectangle.IsBottom = isBottom;
                                }
                            }
                        }
                        else {
                            toRemove.Add(row);
                        }
                    }
                    foreach (Widget row in toRemove) {
                        m_scrollStack.RemoveChildren(row);
                    }
                    for (int i = notRenderCount1; i < notRenderCount1 + renderCount; i++) {
                        if (!renderedRows.ContainsKey(i)) {
                            StackPanelWidget newRow = GenerateTableRow(m_data.GetLastIndexOfRecord(i), i, i == m_data.RecordsCount - 1);
                            if (renderedRows.TryGetValue(i - 1, out Widget upperRow)) {
                                m_scrollStack.Children.InsertAfter(upperRow, newRow);
                            }
                            else if (renderedRows.TryGetValue(i + 1, out Widget lowerRow)) {
                                m_scrollStack.Children.InsertBefore(lowerRow, newRow);
                            }
                            else {
                                m_scrollStack.AddChildren(newRow);
                            }
                            renderedRows.Add(i, newRow);
                        }
                    }
                    int notRenderCount2 = Math.Max(m_data.RecordsCount - 1 - renderCount - notRenderCount1, 0);
                    m_placeholder2.Size = new Vector2(1, notRenderCount2 * 60f);
                }
                else {
                    foreach (Widget row in m_scrollStack.Children) {
                        int tag = (int)row.Tag;
                        uint[] record = m_data.GetLastIndexOfRecord(tag);
                        bool isBottom = tag == m_data.RecordsCount - 1;
                        for (int i = 0; i < 4; i++) {
                            if ((row as StackPanelWidget)?.Children[i] is GVVoltageRectangleWidget rectangle) {
                                rectangle.Text = $"{record[i]:X8} V";
                                rectangle.IsBottom = isBottom;
                            }
                        }
                    }
                }
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
                    m_texture = null;
                    m_cachedWaveRenderTarget?.Dispose();
                    m_cachedBlurRenderTarget?.Dispose();
                    m_cachedTempBlurRenderTarget?.Dispose();
                    m_cachedWaveRenderTarget = null;
                    m_cachedBlurRenderTarget = null;
                    m_cachedTempBlurRenderTarget = null;
                }
            }
            base.MeasureOverride(parentAvailableSize);
        }

        public override void Overdraw(DrawContext dc) {
            base.Overdraw(dc);
            m_primitivesRenderer2D ??= dc.PrimitivesRenderer2D;
            Vector2 size = m_canvas.GlobalBounds.Size();
            if (!m_displayTable
                && size.X > 0) {
                FlatBatch2D.QueueQuad(
                    m_canvas.GlobalBounds.Min,
                    m_canvas.GlobalBounds.Max,
                    0,
                    Vector2.Zero,
                    Vector2.One,
                    Color.White
                );
                FlatBatch2D.Flush();
            }
        }

        public RenderTarget2D Texture {
            get {
                if (m_data.RecordsCount == 0) {
                    return null;
                }
                if (IsTextureObsolete()) {
                    Vector2 size = m_canvas.GlobalBounds.Size();
                    int width = (int)size.X;
                    int height = (int)size.Y;
                    m_cachedWaveRenderTarget ??= new RenderTarget2D(
                        width,
                        height,
                        m_data.DisplayBloom ? 6 : 1,
                        ColorFormat.Rgba8888,
                        DepthFormat.None
                    );
                    m_cachedBlurRenderTarget ??= new RenderTarget2D(
                        width,
                        height,
                        1,
                        ColorFormat.Rgba8888,
                        DepthFormat.None
                    );
                    m_cachedTempBlurRenderTarget ??= new RenderTarget2D(
                        width,
                        height,
                        1,
                        ColorFormat.Rgba8888,
                        DepthFormat.None
                    );
                    m_texture = m_data.GenerateTexture(
                        0,
                        (int)size.X,
                        (int)size.Y,
                        m_cachedWaveRenderTarget,
                        m_cachedBlurRenderTarget,
                        m_cachedTempBlurRenderTarget
                    );
                    //m_texture.GetData(new Rectangle(0, 0, width, height)).m_trueImage.SaveAsBmp("oscilloscope.bmp");
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
                    m_texturedBatch2D = new TexturedBatch2D {
                        Texture = Texture,
                        UseAlphaTest = false,
                        Layer = 0,
                        DepthStencilState = DepthStencilState.None,
                        RasterizerState = RasterizerState.CullNone,
                        BlendState = BlendState.AlphaBlend,
                        SamplerState = SamplerState.LinearClamp
                    };
                    RecordsCountAtLastGenerateFlatBatch3D = m_data.RecordsCount;
                }
                return m_texturedBatch2D;
            }
        }

        public bool IsTextureObsolete() => m_texture == null || m_texture.m_isDisposed || m_data.RecordsCount != RecordsCountAtLastGenerateTexture;

        public StackPanelWidget GenerateTableRow(uint[] record, int index, bool isBottom) {
            StackPanelWidget row = new() { Direction = LayoutDirection.Horizontal, Tag = index };
            row.AddChildren(new GVVoltageRectangleWidget { Text = $"{record[0]:X8} V", Color = GVOscilloscopeData.WaveColor[0], Size = new Vector2(190f, 60f), IsBottom = isBottom });
            row.AddChildren(new GVVoltageRectangleWidget { Text = $"{record[1]:X8} V", Color = GVOscilloscopeData.WaveColor[1], Size = new Vector2(192f, 60f), IsBottom = isBottom });
            row.AddChildren(new GVVoltageRectangleWidget { Text = $"{record[2]:X8} V", Color = GVOscilloscopeData.WaveColor[2], Size = new Vector2(192f, 60f), IsBottom = isBottom });
            row.AddChildren(new GVVoltageRectangleWidget { Text = $"{record[3]:X8} V", Color = GVOscilloscopeData.WaveColor[3], Size = new Vector2(192f, 60f), IsBottom = isBottom });
            row.AddChildren(
                new GVVoltageRectangleWidget {
                    Text = (index + 1).ToString(),
                    Size = new Vector2(126f, 60f),
                    IsBottom = isBottom,
                    IsRightmost = true,
                    VoltageCentered = true
                }
            );
            return row;
        }
    }
}