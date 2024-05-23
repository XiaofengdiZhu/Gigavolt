extern alias OpenTKForWindows;
using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using OpenTKForWindows::OpenTK.Graphics.ES30;
using Color = Engine.Color;

namespace Game {
    public class GVOscilloscopeData {
        public Vector3 Position;
        public Vector3 Right;
        public Vector3 Up;
        public Vector3 Forward;
        public int DisplayWidth = 1024;
        public int DisplayHeight = 1024;
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

        public float DashedLineLeftOffset;
        public float DashedLineUpOffset;
        public float DashedLineHorizontalSpacing;
        public float DashedLineVerticalSpacing;
        public float DashedLineDashLength;
        public float DashedLineDashAndGapLength;

        readonly List<uint[]> Records = new(4100);
        readonly PrimitivesRenderer3D m_primitivesRenderer3D;
        readonly GVOscilloscopePrimitivesRenderer2D m_primitivesRenderer2D;
        readonly TexturedBatch2D m_numberBatch;
        readonly TexturedBatch2D m_arrowButtonBatch;
        public TexturedBatch2D m_autoButtonBatch;
        public TexturedBatch2D m_moonButtonBatch;
        public TexturedBatch2D m_sunButtonBatch;
        readonly GVOscilloscopeWaveFlatBatch2D m_waveBatch;
        RenderTarget2D m_texture;
        TexturedBatch3D m_texturedBatch3D;

        static readonly Color[] m_waveColor = [Color.Green, Color.Cyan, Color.Red, Color.Yellow];

        public GVOscilloscopeData(SubsystemGVOscilloscopeBlockBehavior subsystem) {
            m_primitivesRenderer3D = subsystem.m_primitivesRenderer3D;
            m_primitivesRenderer2D = subsystem.m_primitivesRenderer2D;
            m_numberBatch = subsystem.m_numberBatch;
            m_arrowButtonBatch = subsystem.m_arrowButtonBatch;
            m_autoButtonBatch = subsystem.m_autoButtonBatch;
            m_moonButtonBatch = subsystem.m_moonButtonBatch;
            m_sunButtonBatch = subsystem.m_sunButtonBatch;
            m_waveBatch = m_primitivesRenderer2D.FlatBatch(0, DepthStencilState.None, null, BlendState.AlphaBlend);
        }

        public bool IsTextureObsolete() => m_texture == null || m_texture.m_isDisposed || Records.Count != RecordsCountAtLastGenerateTexture;

        public RenderTarget2D Texture {
            get {
                if (Records.Count == 0) {
                    return null;
                }
                if (IsTextureObsolete()) {
                    RenderTarget2D originRenderTarget = Display.RenderTarget;
                    try {
                        switch (LodLevel) {
                            case < 2:
                                DisplayWidth = 1024;
                                DisplayHeight = 1024;
                                break;
                            case < 5:
                                DisplayWidth = 512;
                                DisplayHeight = 512;
                                break;
                            case < 6:
                                DisplayWidth = 320;
                                DisplayHeight = 320;
                                break;
                            default:
                                DisplayWidth = 160;
                                DisplayHeight = 160;
                                break;
                        }
                        m_texture?.Dispose();
                        RenderTarget2D waveRenderTarget = new(
                            DisplayWidth,
                            DisplayHeight,
                            DisplayBloom && LodLevel < 6 ? 6 : 1,
                            ColorFormat.Rgba8888,
                            DepthFormat.None
                        );
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
                        CalculateDashedLineArguments();
                        m_waveBatch.PrepareBackground(
                            DashedLineLeftOffset,
                            DashedLineUpOffset,
                            DashedLineHorizontalSpacing,
                            DashedLineVerticalSpacing,
                            DashedLineDashLength,
                            DashedLineDashAndGapLength
                        );
                        if (LodLevel < 3) {
                            m_waveBatch.QueueQuad(new Vector2(0, 0), new Vector2(DisplayWidth, DisplayHeight), 0, Color.Transparent);
                            m_waveBatch.FlushBackground();
                        }
#pragma warning disable CS0618 // 类型或成员已过时
                        GL.Enable((All)34370);
#pragma warning restore CS0618 // 类型或成员已过时
                        float levelRange = MaxLevel - MinLevel;
                        float displayWidthMax = DisplayWidth - 1;
                        float displayHeightMax = DisplayHeight - 1;
                        for (int i = 3; i >= 0; i--) {
                            if (!ConnectionState[i]) {
                                continue;
                            }
                            Vector2[] drawPoints = new Vector2[TrueDisplayCount > Records.Count ? Records.Count + 2 : TrueDisplayCount];
                            for (int j = 0; j < TrueDisplayCount; j++) {
                                if (j >= Records.Count) {
                                    drawPoints[j] = new Vector2((1f - j / (float)TrueDisplayCount) * displayWidthMax, displayHeightMax);
                                    drawPoints[j + 1] = new Vector2(0f, displayHeightMax);
                                    break;
                                }
                                uint record = Records[Records.Count - j - 1][i];
                                drawPoints[j] = new Vector2((1f - j / (float)TrueDisplayCount) * displayWidthMax, (1f - (record - MinLevel) / levelRange) * displayHeightMax);
                            }
                            Color color = m_waveColor[i];
                            m_waveBatch.QueueLineStrip(drawPoints, 0, color);
                            if (LodLevel < 5) {
                                m_waveBatch.QueuePoints(drawPoints, 0, color);
                            }
                            m_waveBatch.FlushWave();
                        }
                        if (DisplayBloom && LodLevel < 6) {
                            waveRenderTarget.GenerateMipMaps();
                            RenderTarget2D blurRenderTarget = new(
                                DisplayWidth,
                                DisplayHeight,
                                1,
                                ColorFormat.Rgba8888,
                                DepthFormat.None
                            );
                            Display.RenderTarget = blurRenderTarget;
                            Display.Clear(Color.Black);
                            GVOscilloscopeBlurTexturedBatch2D blurBatch = m_primitivesRenderer2D.TexturedBatch(
                                waveRenderTarget,
                                false,
                                0,
                                DepthStencilState.None,
                                null,
                                BlendState.AlphaBlend
                            );
                            blurBatch.QueueQuad(
                                Vector2.Zero,
                                new Vector2(DisplayWidth, DisplayHeight),
                                0,
                                Vector2.Zero,
                                Vector2.One,
                                Color.White
                            );
                            blurBatch.FlushBlur();
                        }
                        if (LodLevel < 4) {
                            uint dy = (MaxLevel - MinLevel) / 8u;
                            for (uint i = 0u; i < 8u; i++) {
                                Draw8HexNumber(MinLevel + dy * i, new Vector2(8f, DisplayHeight - 41 - i * DashedLineVerticalSpacing), new Vector2(45f, 33f), Color.LightGray);
                            }
                            if (LodLevel < 2) {
                                Draw8HexNumber(MaxLevel, new Vector2(8f, 8), new Vector2(45f, 33f), Color.LightGray);
                            }
                            int recordLabelsCount = DisplayWidth / (float)DisplayHeight > 1.5f ? 16 : 8;
                            for (int i = 0; i < recordLabelsCount - (DisplayButtons && LodLevel < 2 ? 2 : 1); i++) {
                                Draw4DecNumber(i * TrueDisplayCount / recordLabelsCount, new Vector2(DisplayWidth - 53 - i * DashedLineHorizontalSpacing, DisplayHeight - 23), new Vector2(45, 15), Color.LightGray);
                            }
                            m_numberBatch.Flush();
                        }
                        if (DisplayButtons && LodLevel < 2) {
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
                                new Vector2(76f, 916f),
                                new Vector2(164f, 1004f),
                                0,
                                Vector2.Zero,
                                Vector2.One,
                                MinLevel >= uint.MaxValue - 64u || MaxLevel <= 64u ? Color.DarkGray : Color.LightGray
                            );
                            m_arrowButtonBatch.QueueQuad(
                                new Vector2(188f, 916f),
                                new Vector2(276f, 1004f),
                                0,
                                Vector2.One,
                                Vector2.Zero,
                                MinLevel == 0u ? Color.DarkGray : Color.LightGray
                            );
                            m_arrowButtonBatch.QueueQuad(
                                new Vector2(412f, 108f),
                                new Vector2(412f, 20f),
                                new Vector2(500f, 20f),
                                new Vector2(500f, 108f),
                                0,
                                Vector2.Zero,
                                Vector2.UnitX,
                                Vector2.One,
                                Vector2.UnitY,
                                Array.IndexOf(displayCountArray, DisplayCount) == displayCountArray.Length - 1 ? Color.DarkGray : Color.LightGray
                            );
                            m_arrowButtonBatch.QueueQuad(
                                new Vector2(612f, 20f),
                                new Vector2(612f, 108f),
                                new Vector2(524f, 108f),
                                new Vector2(524f, 20f),
                                0,
                                Vector2.Zero,
                                Vector2.UnitX,
                                Vector2.One,
                                Vector2.UnitY,
                                Array.IndexOf(displayCountArray, DisplayCount) == 0 ? Color.DarkGray : Color.LightGray
                            );
                            (DisplayBloom ? m_sunButtonBatch : m_moonButtonBatch).QueueQuad(
                                new Vector2(916f, 20f),
                                new Vector2(1004f, 108f),
                                0,
                                Vector2.Zero,
                                Vector2.One,
                                Color.LightGray
                            );
                            if (!AutoSetMinMaxLevelMode) {
                                m_autoButtonBatch.QueueQuad(
                                    new Vector2(804f, 20f),
                                    new Vector2(892f, 108f),
                                    0,
                                    Vector2.Zero,
                                    Vector2.One,
                                    Color.LightGray
                                );
                            }
                            m_primitivesRenderer2D.Flush();
                        }
                        m_texture = Display.RenderTarget;
                        if (DisplayBloom && LodLevel < 6) {
                            waveRenderTarget.Dispose();
                        }
                        RecordsCountAtLastGenerateTexture = Records.Count;
                        return m_texture;
                    }
                    catch (Exception e) {
                        Log.Error(e);
                        return null;
                    }
                    finally {
                        Display.RenderTarget = originRenderTarget;
                    }
                }
                return m_texture;
            }
        }

        public TexturedBatch3D FlatBatch3D {
            get {
                if (Records.Count == 0) {
                    return null;
                }
                if (m_texturedBatch3D == null
                    || Records.Count != RecordsCountAtLastGenerateFlatBatch3D
                    || IsTextureObsolete()) {
                    m_texturedBatch3D = m_primitivesRenderer3D.TexturedBatch(
                        Texture,
                        false,
                        0,
                        DepthStencilState.DepthRead,
                        RasterizerState.CullCounterClockwiseScissor,
                        BlendState.NonPremultiplied,
                        SamplerState.PointClamp
                    );
                    RecordsCountAtLastGenerateFlatBatch3D = Records.Count;
                }
                return m_texturedBatch3D;
            }
        }

        public void AddRecord(uint[] record) {
            Records.Add(record);
            if (Records.Count > 4097) {
                Records.RemoveRange(0, 2);
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
                        Texture.Dispose();
                    }
                }
            }
            else {
                DisplayCount = 512;
                if (!IsTextureObsolete()) {
                    Texture.Dispose();
                }
            }
        }

        public void DecreaseDisplayCount() {
            int index = Array.IndexOf(displayCountArray, DisplayCount);
            if (index > -1) {
                if (index != 0) {
                    DisplayCount = displayCountArray[index - 1];
                    if (!IsTextureObsolete()) {
                        Texture.Dispose();
                    }
                }
            }
            else {
                DisplayCount = 512;
                if (!IsTextureObsolete()) {
                    Texture.Dispose();
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
                Texture.Dispose();
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
                Texture.Dispose();
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
                Texture.Dispose();
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
                Texture.Dispose();
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
                Texture.Dispose();
            }
        }

        public static uint GetNearest16Multiples(uint value) => (value + 15u) & 0xFFFFFFF0u;

        public void CalculateDashedLineArguments() {
            DashedLineVerticalSpacing = DisplayHeight / 8f;
            DashedLineHorizontalSpacing = DisplayWidth / (DisplayWidth / (float)DisplayHeight > 1.5f ? 16f : 8f);
            DashedLineLeftOffset = 0f;
            DashedLineUpOffset = 0f;
            if (LodLevel < 1) {
                DashedLineDashLength = DashedLineVerticalSpacing / 16f;
                DashedLineDashAndGapLength = DashedLineDashLength * 3f;
            }
            else if (LodLevel < 2) {
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
    }
}