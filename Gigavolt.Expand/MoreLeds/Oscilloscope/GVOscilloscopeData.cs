extern alias OpenTKForWindows;
extern alias OpenTKForAndroid;
using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Color = Engine.Color;

namespace Game {
    public class GVOscilloscopeData {
        public Vector3 Position;
        public Vector3 Right;
        public Vector3 Up;
        public Vector3 Forward;
        public int DisplaySize = 1024;
        public int DisplayCount = 512;
        public int TrueDisplayCount;
        public uint MinLevel;
        public uint MaxLevel = uint.MaxValue;
        public bool[] ConnectionState = new bool[4];
        public bool AutoSetMinMaxLevelMode = true;
        public int LodLevel = 0;
        public bool DisplayButtons = true;
        public bool DisplayBloom = true;
        public int RecordsCount => Records.Count;
        public int RecordsCountAtLastGenerateTexture;
        public int RecordsCountAtLastGenerateFlatBatch3D;
        public int RecordsCountAtAutoSetMinMaxLevel;

        public float DashedLineHorizontalSpacing;
        public float DashedLineVerticalSpacing;
        public float DashedLineDashLength;
        public float DashedLineDashAndGapLength;

        readonly List<uint[]> Records = new(5100);
        readonly TexturedBatch2D m_numberBatch;
        readonly TexturedBatch2D m_arrowButtonBatch;
        public TexturedBatch2D m_autoButtonBatch;
        public TexturedBatch2D m_moonButtonBatch;
        public TexturedBatch2D m_sunButtonBatch;
        readonly GVOscilloscopeWaveFlatBatch2D m_waveBatch;
        RenderTarget2D m_texture;

        readonly RenderTarget2D[] m_cachedWaveRenderTargets = new RenderTarget2D[8];
        readonly RenderTarget2D[] m_cachedBlurRenderTargets = new RenderTarget2D[3];
        readonly RenderTarget2D[] m_cachedTempBlurRenderTargets = new RenderTarget2D[3];

        TexturedBatch3D m_texturedBatch3D;

        public static readonly Color[] WaveColor = [Color.Green, Color.Cyan, Color.Red, Color.Yellow];

        public GVOscilloscopeData(SubsystemGVOscilloscopeBlockBehavior subsystem) {
            m_numberBatch = subsystem.m_numberBatch;
            m_arrowButtonBatch = subsystem.m_arrowButtonBatch;
            m_autoButtonBatch = subsystem.m_autoButtonBatch;
            m_moonButtonBatch = subsystem.m_moonButtonBatch;
            m_sunButtonBatch = subsystem.m_sunButtonBatch;
            m_waveBatch = subsystem.m_primitivesRenderer2D.FlatBatch(0, DepthStencilState.None, null, BlendState.AlphaBlend);
        }

        public bool IsTextureObsolete() => m_texture == null || m_texture.m_isDisposed || Records.Count != RecordsCountAtLastGenerateTexture;

        public RenderTarget2D Texture {
            get {
                if (Records.Count == 0) {
                    return null;
                }
                if (IsTextureObsolete()) {
                    int cacheIndex = 3;
                    switch (LodLevel) {
                        case < 2:
                            DisplaySize = 1024;
                            cacheIndex = 0;
                            break;
                        case < 5:
                            DisplaySize = 512;
                            cacheIndex = 1;
                            break;
                        case < 6:
                            DisplaySize = 320;
                            cacheIndex = 2;
                            break;
                        default:
                            DisplaySize = 160;
                            break;
                    }
                    RenderTarget2D blurRenderTarget = null;
                    RenderTarget2D tempBlurRenderTarget = null;
                    bool displayBloom = DisplayBloom && LodLevel < 6;
                    if (displayBloom) {
                        blurRenderTarget = m_cachedBlurRenderTargets[cacheIndex];
                        if (blurRenderTarget == null) {
                            blurRenderTarget = new RenderTarget2D(
                                DisplaySize,
                                DisplaySize,
                                1,
                                ColorFormat.Rgba8888,
                                DepthFormat.None
                            );
                            m_cachedBlurRenderTargets[cacheIndex] = blurRenderTarget;
                        }
                        tempBlurRenderTarget = m_cachedTempBlurRenderTargets[cacheIndex];
                        if (tempBlurRenderTarget == null) {
                            tempBlurRenderTarget = new RenderTarget2D(
                                DisplaySize,
                                DisplaySize,
                                1,
                                ColorFormat.Rgba8888,
                                DepthFormat.None
                            );
                            m_cachedTempBlurRenderTargets[cacheIndex] = tempBlurRenderTarget;
                        }
                        cacheIndex += 4;
                    }
                    RenderTarget2D waveRenderTarget = m_cachedWaveRenderTargets[cacheIndex];
                    if (waveRenderTarget == null) {
                        waveRenderTarget = new RenderTarget2D(
                            DisplaySize,
                            DisplaySize,
                            displayBloom ? 6 : 1,
                            ColorFormat.Rgba8888,
                            DepthFormat.None
                        );
                        m_cachedWaveRenderTargets[cacheIndex] = waveRenderTarget;
                    }
                    m_texture = GenerateTexture(
                        LodLevel,
                        DisplaySize,
                        DisplaySize,
                        waveRenderTarget,
                        blurRenderTarget,
                        tempBlurRenderTarget
                    );
                    RecordsCountAtLastGenerateTexture = Records.Count;
                }
                return m_texture;
            }
            set => m_texture = value;
        }

        public RenderTarget2D GenerateTexture(int lodLevel, int displayWidth, int displayHeight, RenderTarget2D waveRenderTarget, RenderTarget2D blurRenderTarget = null, RenderTarget2D tempBlurRenderTarget = null) {
            RenderTarget2D originRenderTarget = Display.RenderTarget;
            try {
                bool displayBloom = DisplayBloom && lodLevel < 6;
                Display.RenderTarget = waveRenderTarget;
                Display.Clear(Color.Black);
                TrueDisplayCount = 0;
                if (Records.Count > DisplayCount) {
                    TrueDisplayCount = DisplayCount;
                }
                else {
                    foreach (int num in displayCountArray) {
                        if (num > Records.Count) {
                            TrueDisplayCount = num;
                            break;
                        }
                    }
                }
                if (AutoSetMinMaxLevelMode) {
                    AutoSetMinMaxLevel();
                }
                CalculateDashedLineArguments(lodLevel, displayWidth, displayHeight);
                m_waveBatch.PrepareBackground(DashedLineHorizontalSpacing, DashedLineVerticalSpacing, DashedLineDashLength, DashedLineDashAndGapLength);
                if (lodLevel < 3) {
                    m_waveBatch.QueueQuad(new Vector2(0, 0), new Vector2(displayWidth, displayHeight), 0, Color.Transparent);
                    m_waveBatch.FlushBackground();
                }
                switch (VersionsManager.Platform) {
                    case Platform.Desktop:
                        WindowsEnableGLPointSize();
                        break;
                    case Platform.Android:
                        AndroidEnableGLPointSize();
                        break;
                }
                float levelRange = MaxLevel - MinLevel;
                float displayWidthMax = displayWidth - 1;
                float displayHeightMax = displayHeight - 1;
                for (int i = 3; i >= 0; i--) {
                    if (!ConnectionState[i]) {
                        continue;
                    }
                    Color color = WaveColor[i];
                    for (int j = 0; j < TrueDisplayCount; j++) {
                        if (j >= Records.Count) {
                            if (MinLevel == 0u) {
                                m_waveBatch.QueueLine(new Vector2((1f - j / (float)TrueDisplayCount) * displayWidthMax, displayHeightMax), new Vector2(0f, displayHeightMax), 0, color);
                            }
                            break;
                        }
                        int index = Records.Count - j - 1;
                        uint record = Records[index][i];
                        if (index > 0) {
                            uint record2 = Records[index - 1][i];
                            if (record >= MinLevel
                                || record2 >= MinLevel) {
                                m_waveBatch.QueueLine(new Vector2((1f - j / (float)TrueDisplayCount) * displayWidthMax, (MinLevel > record ? 1f : 1f - (record - MinLevel) / levelRange) * displayHeightMax), new Vector2((1f - (j + 1) / (float)TrueDisplayCount) * displayWidthMax, (MinLevel > record2 ? 1f : 1f - (record2 - MinLevel) / levelRange) * displayHeightMax), 0, color);
                            }
                        }
                        else if (record >= MinLevel) {
                            m_waveBatch.QueueLine(new Vector2((1f - j / (float)TrueDisplayCount) * displayWidthMax, (1f - (record - MinLevel) / levelRange) * displayHeightMax), new Vector2((1f - (j + 1) / (float)TrueDisplayCount) * displayWidthMax, displayHeightMax), 0, color);
                        }
                        if (record >= MinLevel
                            && lodLevel < 5
                            && record <= MaxLevel) {
                            m_waveBatch.QueuePoint(new Vector2((1f - j / (float)TrueDisplayCount) * displayWidthMax, (1f - (record - MinLevel) / levelRange) * displayHeightMax), 0, color);
                        }
                    }
                    m_waveBatch.FlushWave();
                }
                if (displayBloom && blurRenderTarget != null) {
                    waveRenderTarget.GenerateMipMaps();
                    Display.RenderTarget = blurRenderTarget;
                    Display.Clear(Color.Black);
                    GVOscilloscopeBlurTexturedBatch2D blurBatch = new() {
                        Texture = waveRenderTarget,
                        UseAlphaTest = false,
                        Layer = 0,
                        DepthStencilState = DepthStencilState.None,
                        RasterizerState = RasterizerState.CullNone,
                        BlendState = BlendState.AlphaBlend,
                        SamplerState = SamplerState.LinearClamp
                    };
                    blurBatch.QueueQuad(
                        Vector2.Zero,
                        new Vector2(displayWidth, displayHeight),
                        0,
                        Vector2.Zero,
                        Vector2.One,
                        Color.White
                    );
                    blurBatch.FlushBlur(tempBlurRenderTarget);
                }
                if (lodLevel < 4) {
                    uint dy = (MaxLevel - MinLevel) / 8u;
                    bool flag = false;
                    if (DashedLineVerticalSpacing >= 49) {
                        flag = true;
                        for (uint i = 0u; i < 8u; i++) {
                            Draw8HexNumber(MinLevel + dy * i, new Vector2(8f, displayHeight - 41 - i * DashedLineVerticalSpacing), new Vector2(45f, 33f), Color.LightGray);
                        }
                        if (lodLevel < 2
                            && DashedLineVerticalSpacing >= 90) {
                            Draw8HexNumber(MaxLevel, new Vector2(8f, 8), new Vector2(45f, 33f), Color.LightGray);
                        }
                    }
                    if (DashedLineHorizontalSpacing >= 49) {
                        flag = true;
                        int recordLabelsCount = displayWidth / (float)displayHeight > 1.5f ? 16 : 8;
                        for (int i = 0; i < recordLabelsCount - (DisplayButtons && lodLevel < 2 ? 2 : 1); i++) {
                            Draw4DecNumber(i * TrueDisplayCount / recordLabelsCount, new Vector2(displayWidth - 53 - i * DashedLineHorizontalSpacing, displayHeight - 23), new Vector2(45, 15), Color.LightGray);
                        }
                    }
                    if (flag) {
                        m_numberBatch.Flush();
                    }
                }
                if (DisplayButtons
                    && lodLevel < 2
                    && displayWidth >= 800) {
                    m_arrowButtonBatch.QueueQuad(
                        new Vector2(76f, 20f),
                        new Vector2(164f, 108f),
                        0,
                        Vector2.Zero,
                        Vector2.One,
                        MaxLevel == uint.MaxValue ? Color.DarkGray : Color.LightGray
                    );
                    m_arrowButtonBatch.QueueQuad(
                        new Vector2(188f, 20f),
                        new Vector2(276f, 108f),
                        0,
                        Vector2.One,
                        Vector2.Zero,
                        MaxLevel <= 64u || MinLevel >= uint.MaxValue - 64u ? Color.DarkGray : Color.LightGray
                    );
                    m_arrowButtonBatch.QueueQuad(
                        new Vector2(76f, displayHeight - 108f),
                        new Vector2(164f, displayHeight - 20f),
                        0,
                        Vector2.Zero,
                        Vector2.One,
                        MinLevel >= uint.MaxValue - 64u || MaxLevel <= 64u ? Color.DarkGray : Color.LightGray
                    );
                    m_arrowButtonBatch.QueueQuad(
                        new Vector2(188f, displayHeight - 108f),
                        new Vector2(276f, displayHeight - 20f),
                        0,
                        Vector2.One,
                        Vector2.Zero,
                        MinLevel == 0u ? Color.DarkGray : Color.LightGray
                    );
                    m_arrowButtonBatch.QueueQuad(
                        new Vector2(displayWidth / 2 - 100f, 108f),
                        new Vector2(displayWidth / 2 - 100f, 20f),
                        new Vector2(displayWidth / 2 - 12f, 20f),
                        new Vector2(displayWidth / 2 - 12f, 108f),
                        0,
                        Vector2.Zero,
                        Vector2.UnitX,
                        Vector2.One,
                        Vector2.UnitY,
                        Array.IndexOf(displayCountArray, DisplayCount) == displayCountArray.Length - 1 ? Color.DarkGray : Color.LightGray
                    );
                    m_arrowButtonBatch.QueueQuad(
                        new Vector2(displayWidth / 2 + 100f, 20f),
                        new Vector2(displayWidth / 2 + 100f, 108f),
                        new Vector2(displayWidth / 2 + 12f, 108f),
                        new Vector2(displayWidth / 2 + 12f, 20f),
                        0,
                        Vector2.Zero,
                        Vector2.UnitX,
                        Vector2.One,
                        Vector2.UnitY,
                        Array.IndexOf(displayCountArray, DisplayCount) == 0 ? Color.DarkGray : Color.LightGray
                    );
                    TexturedBatch2D batch = DisplayBloom ? m_sunButtonBatch : m_moonButtonBatch;
                    (DisplayBloom ? m_sunButtonBatch : m_moonButtonBatch).QueueQuad(
                        new Vector2(displayWidth - 108f, 20f),
                        new Vector2(displayWidth - 20f, 108f),
                        0,
                        Vector2.Zero,
                        Vector2.One,
                        Color.LightGray
                    );
                    if (!AutoSetMinMaxLevelMode) {
                        m_autoButtonBatch.QueueQuad(
                            new Vector2(displayWidth - 220f, 20f),
                            new Vector2(displayWidth - 132f, 108f),
                            0,
                            Vector2.Zero,
                            Vector2.One,
                            Color.LightGray
                        );
                        m_autoButtonBatch.Flush();
                    }
                    m_arrowButtonBatch.Flush();
                    batch.Flush();
                }
                return Display.RenderTarget;
            }
            catch (Exception e) {
                Log.Error(e);
                return null;
            }
            finally {
                Display.RenderTarget = originRenderTarget;
            }
        }

        public TexturedBatch3D TexturedBatch3D {
            get {
                if (Records.Count == 0) {
                    return null;
                }
                if (m_texturedBatch3D == null
                    || Records.Count != RecordsCountAtLastGenerateFlatBatch3D
                    || IsTextureObsolete()) {
                    m_texturedBatch3D = new TexturedBatch3D {
                        Texture = Texture,
                        UseAlphaTest = false,
                        Layer = 0,
                        DepthStencilState = DepthStencilState.DepthRead,
                        RasterizerState = RasterizerState.CullCounterClockwiseScissor,
                        BlendState = BlendState.NonPremultiplied,
                        SamplerState = SamplerState.PointClamp
                    };
                    RecordsCountAtLastGenerateFlatBatch3D = Records.Count;
                }
                return m_texturedBatch3D;
            }
        }

        public uint[] GetLastIndexOfRecord(int index) => Records[Records.Count - 1 - index];
        public uint[] LastRecord => Records[Records.Count - 1];

        public void AddRecord(uint[] record) {
            Records.Add(record);
            if (Records.Count > 5096) {
                Records.RemoveRange(0, 1000);
            }
        }

        static readonly int[] displayCountArray = [
            64,
            128,
            256,
            512,
            1024,
            2048,
            3072,
            4096
        ];

        public void IncreaseDisplayCount() {
            int index = Array.IndexOf(displayCountArray, DisplayCount);
            if (index > -1) {
                if (index != displayCountArray.Length - 1) {
                    DisplayCount = displayCountArray[index + 1];
                    if (!IsTextureObsolete()) {
                        m_texture = null;
                    }
                }
            }
            else {
                DisplayCount = 512;
                if (!IsTextureObsolete()) {
                    m_texture = null;
                }
            }
        }

        public void DecreaseDisplayCount() {
            int index = Array.IndexOf(displayCountArray, DisplayCount);
            if (index > -1) {
                if (index != 0) {
                    DisplayCount = displayCountArray[index - 1];
                    if (!IsTextureObsolete()) {
                        m_texture = null;
                    }
                }
            }
            else {
                DisplayCount = 512;
                if (!IsTextureObsolete()) {
                    m_texture = null;
                }
            }
        }

        public void IncreaseMinLevel() {
            if (MinLevel >= uint.MaxValue - 64u) {
                MinLevel = uint.MaxValue - 64u;
                return;
            }
            if (MaxLevel <= 64u) {
                MinLevel = 0u;
                return;
            }
            AutoSetMinMaxLevelMode = false;
            MinLevel = GetNearest16Multiples(MinLevel + (MaxLevel - MinLevel) / 4u);
            if (MaxLevel - MinLevel <= 64u) {
                MinLevel = MaxLevel - 64u;
            }
            if (!IsTextureObsolete()) {
                m_texture = null;
            }
        }

        public void DecreaseMinLevel() {
            if (MinLevel == 0u) {
                return;
            }
            AutoSetMinMaxLevelMode = false;
            uint num = (MaxLevel - MinLevel) / 3u;
            MinLevel = num > MinLevel ? 0u : GetNearest16Multiples(MinLevel - num);
            if (!IsTextureObsolete()) {
                m_texture = null;
            }
        }

        public void IncreaseMaxLevel() {
            if (MaxLevel == uint.MaxValue) {
                return;
            }
            AutoSetMinMaxLevelMode = false;
            uint num = (MaxLevel - MinLevel) / 3u;
            MaxLevel = num > uint.MaxValue - MaxLevel ? uint.MaxValue : GetNearest16Multiples(MaxLevel + num);
            if (!IsTextureObsolete()) {
                m_texture = null;
            }
        }

        public void DecreaseMaxLevel() {
            if (MaxLevel <= 64u) {
                MaxLevel = 64u;
                return;
            }
            if (MinLevel >= uint.MaxValue - 64u) {
                MaxLevel = uint.MaxValue;
                return;
            }
            AutoSetMinMaxLevelMode = false;
            MaxLevel = GetNearest16Multiples(MaxLevel - (MaxLevel - MinLevel) / 4u);
            if (MaxLevel - MinLevel <= 64u) {
                MaxLevel = GetNearest16Multiples(MinLevel + 64u);
            }
            if (!IsTextureObsolete()) {
                m_texture = null;
            }
        }

        public void AutoSetMinMaxLevel(bool dispose = false) {
            AutoSetMinMaxLevelMode = true;
            if (Records.Count == 0
                || (AutoSetMinMaxLevelMode && Records.Count == RecordsCountAtAutoSetMinMaxLevel)) {
                return;
            }
            uint minRecord = uint.MaxValue;
            uint maxRecord = 0u;
            HashSet<int> connected = new();
            for (int i = 0; i < 4; i++) {
                if (ConnectionState[i]) {
                    connected.Add(i);
                }
            }
            for (int i = 0; i < Math.Min(DisplayCount, Records.Count); i++) {
                uint[] record = Records[Records.Count - i - 1];
                if (minRecord == 0
                    && maxRecord == uint.MaxValue) {
                    break;
                }
                foreach (int direction in connected) {
                    uint num = record[direction];
                    if (num < minRecord) {
                        minRecord = num;
                    }
                    if (num > maxRecord) {
                        maxRecord = num;
                    }
                }
            }
            MaxLevel = maxRecord switch {
                >= uint.MaxValue - 64u => uint.MaxValue,
                <= 64u => 64u,
                _ => GetNearest16Multiples(maxRecord)
            };
            MinLevel = minRecord switch {
                <= 64u => 0u,
                >= uint.MaxValue - 64u => uint.MaxValue - 64u,
                _ => GetNearest16Multiples(minRecord)
            };
            if (MaxLevel - MinLevel < 64u) {
                MaxLevel = MinLevel + 64u;
            }
            RecordsCountAtAutoSetMinMaxLevel = Records.Count;
            if (dispose) {
                m_texture = null;
            }
        }

        public static uint GetNearest16Multiples(uint value) => (value + 15u) & 0xFFFFFFF0u;

        public void CalculateDashedLineArguments(int lodLevel, int displayWidth, int displayHeight) {
            DashedLineVerticalSpacing = displayHeight / 8f;
            DashedLineHorizontalSpacing = displayWidth / (displayWidth / (float)displayHeight > 1.5f ? 16f : 8f);
            if (lodLevel < 1) {
                DashedLineDashLength = DashedLineVerticalSpacing / 16f;
                DashedLineDashAndGapLength = DashedLineDashLength * 3f;
            }
            else if (lodLevel < 2) {
                DashedLineDashLength = DashedLineVerticalSpacing / 8f;
                DashedLineDashAndGapLength = DashedLineDashLength * 2f;
            }
            else {
                DashedLineDashLength = DashedLineVerticalSpacing / 6f;
                DashedLineDashAndGapLength = DashedLineDashLength * 2f;
            }
        }

        //smallest size: 15,11
        public void Draw8HexNumber(uint voltage, Vector2 position, Vector2 size, Color color) {
            for (int y = 0; y < 2; y++) {
                for (int x = 0; x < 4; x++) {
                    int index = y * 4 + x;
                    uint number = (voltage >> (index * 4)) & 15u;
                    float px1 = (12 - x * 4) / 15f;
                    float px2 = px1 + 3 / 15f;
                    float py1 = y == 1 ? 0f : 6f / 11f;
                    float py2 = py1 + 5 / 11f;
                    float tx1 = number % 4 * 3f / 12f;
                    float tx2 = tx1 + 3f / 12f;
                    float ty1 = number / 4 * 5f / 20f;
                    float ty2 = ty1 + 5f / 20f;
                    m_numberBatch.QueueQuad(
                        position + new Vector2(px1 * size.X, py1 * size.Y),
                        position + new Vector2(px2 * size.X, py2 * size.Y),
                        0,
                        new Vector2(tx1, ty1),
                        new Vector2(tx2, ty2),
                        color
                    );
                }
            }
        }

        //smallest size: 15,5
        public void Draw4DecNumber(int number, Vector2 position, Vector2 size, Color color) {
            number %= 10000;
            int maxPlace = number switch {
                >= 1000 => 4,
                >= 100 => 3,
                >= 10 => 2,
                _ => 1
            };
            for (int i = 0; i < maxPlace; i++) {
                int digit = number % 10;
                number /= 10;
                float px1 = (12 - i * 4) / 15f;
                float px2 = px1 + 3 / 15f;
                float tx1 = digit % 4 * 3f / 12f;
                float tx2 = tx1 + 3f / 12f;
                float ty1 = digit / 4 * 5f / 20f;
                float ty2 = ty1 + 5f / 20f;
                m_numberBatch.QueueQuad(
                    position + new Vector2(px1 * size.X, 0f),
                    position + new Vector2(px2 * size.X, size.Y),
                    0,
                    new Vector2(tx1, ty1),
                    new Vector2(tx2, ty2),
                    color
                );
            }
        }

        public void Interact(Vector2 position, float displayWidth, float displayHeight) {
            if (position.X is >= 76f and <= 164f
                && position.Y is >= 20f and <= 108f) {
                IncreaseMaxLevel();
            }
            else if (position.X is >= 188f and <= 276f
                && position.Y is >= 20f and <= 108f) {
                DecreaseMaxLevel();
            }
            else if (position.X is >= 76f and <= 164f
                && position.Y >= displayHeight - 108f
                && position.Y <= displayHeight - 20f) {
                IncreaseMinLevel();
            }
            else if (position.X is >= 188f and <= 276f
                && position.Y >= displayHeight - 108f
                && position.Y <= displayHeight - 20f) {
                DecreaseMinLevel();
            }
            else if (position.X >= displayWidth / 2f - 100f
                && position.X <= displayWidth / 2f - 12f
                && position.Y is >= 20f and <= 108f) {
                IncreaseDisplayCount();
            }
            else if (position.X >= displayWidth / 2f + 12f
                && position.X <= displayWidth / 2f + 100f
                && position.Y is >= 20f and <= 108f) {
                DecreaseDisplayCount();
            }
            else if (position.X >= displayWidth - 108f
                && position.X <= displayWidth - 20f
                && position.Y is >= 20f and <= 108f) {
                DisplayBloom = !DisplayBloom;
                if (!IsTextureObsolete()) {
                    m_texture = null;
                }
            }
            else if (position.X >= displayWidth - 220f
                && position.X <= displayWidth - 132f
                && position.Y is >= 20f and <= 108f) {
                if (!AutoSetMinMaxLevelMode) {
                    AutoSetMinMaxLevel(!IsTextureObsolete());
                }
            }
            else {
                DisplayButtons = !DisplayButtons;
            }
        }

#pragma warning disable CS0618 // 类型或成员已过时
        public static void WindowsEnableGLPointSize() {
            OpenTKForWindows::OpenTK.Graphics.ES30.GL.Enable((OpenTKForWindows::OpenTK.Graphics.ES30.All)34370);
        }

        public static void AndroidEnableGLPointSize() {
            OpenTKForAndroid::OpenTK.Graphics.ES30.GL.Enable((OpenTKForAndroid::OpenTK.Graphics.ES30.All)34370);
        }
#pragma warning restore CS0618 // 类型或成员已过时
        public void Dispose() {
            m_texture?.Dispose();
            foreach (RenderTarget2D renderTarget in m_cachedWaveRenderTargets) {
                renderTarget?.Dispose();
            }
            foreach (RenderTarget2D renderTarget in m_cachedBlurRenderTargets) {
                renderTarget?.Dispose();
            }
            foreach (RenderTarget2D renderTarget in m_cachedTempBlurRenderTargets) {
                renderTarget?.Dispose();
            }
            Records.Clear();
        }
    }
}