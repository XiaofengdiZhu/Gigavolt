using System;
using System.Collections.Generic;
using System.Linq;
using Engine;
using Engine.Graphics;
using Color = Engine.Color;

namespace Game {
    public class GVOscilloscopeData {
        public Vector3 Position;
        public Vector3 Right;
        public Vector3 Up;
        public Vector3 Forward;
        public int DisplayCount = 512;
        public uint MinLevel;
        public uint MaxLevel = uint.MaxValue;
        public bool AutoSetMinMaxLevelMode = true;
        public int RecordsCount => Records.Count;
        public int RecordsCountAtLastGenerateWaveTexture;
        public int RecordsCountAtLastGenerateFlatBatch3D;
        public int RecordsCountAtAutoSetMinMaxLevel;

        readonly List<uint[]> Records = new(4100);
        readonly PrimitivesRenderer3D m_primitivesRenderer3D;
        readonly PrimitivesRenderer2D m_primitivesRenderer2D = new();
        readonly FlatBatch2D m_flatBatch2D;
        RenderTarget2D m_waveTexture;
        TexturedBatch3D m_texturedBatch3D;
        static readonly Color[] m_linesColor = [Color.Green, Color.Cyan, Color.Red, Color.Yellow];

        public GVOscilloscopeData(PrimitivesRenderer3D primitivesRenderer3D) {
            m_primitivesRenderer3D = primitivesRenderer3D;
            m_flatBatch2D = m_primitivesRenderer2D.FlatBatch(0, DepthStencilState.None, null, BlendState.AlphaBlend);
        }

        public RenderTarget2D WaveTexture {
            get {
                if (Records.Count == 0) {
                    return null;
                }
                if (m_waveTexture == null
                    || m_waveTexture.m_isDisposed
                    || Records.Count != RecordsCountAtLastGenerateWaveTexture) {
                    const int displayWidth = 1024;
                    const int displayHeight = 1024;
                    RenderTarget2D originRenderTarget = Display.RenderTarget;
                    try {
                        m_waveTexture?.Dispose();
                        m_waveTexture = new RenderTarget2D(
                            displayWidth,
                            displayHeight,
                            1,
                            ColorFormat.Rgba8888,
                            DepthFormat.None
                        );
                        Display.RenderTarget = m_waveTexture;
                        Display.Clear(Color.Black);
                        int trueDisplayCount = 0;
                        if (Records.Count > DisplayCount) {
                            trueDisplayCount = DisplayCount;
                        }
                        else {
                            foreach (int num in displayCountArray) {
                                if (num > Records.Count) {
                                    trueDisplayCount = num;
                                    break;
                                }
                            }
                        }
                        if (AutoSetMinMaxLevelMode) {
                            AutoSetMinMaxLevel();
                        }
                        for (int i = 3; i >= 0; i--) {
                            Vector2[] drawPoints = new Vector2[trueDisplayCount > Records.Count ? Records.Count + 2 : trueDisplayCount];
                            for (int j = 0; j < trueDisplayCount; j++) {
                                if (j >= Records.Count) {
                                    drawPoints[j] = new Vector2((1 - j / (float)trueDisplayCount) * displayWidth, displayHeight);
                                    drawPoints[j + 1] = new Vector2(0f, displayHeight);
                                    break;
                                }
                                uint record = Records[Records.Count - j - 1][i];
                                drawPoints[j] = new Vector2((1 - j / (float)trueDisplayCount) * displayWidth, (1 - (record - MinLevel) / (float)(MaxLevel - MinLevel)) * displayHeight);
                            }
                            m_flatBatch2D.QueueLineStrip(drawPoints, 0, m_linesColor[i]);
                        }
                        m_primitivesRenderer2D.Flush();
                        RecordsCountAtLastGenerateWaveTexture = Records.Count;
                        return m_waveTexture;
                    }
                    catch (Exception e) {
                        Log.Error(e);
                        return null;
                    }
                    finally {
                        Display.RenderTarget = originRenderTarget;
                    }
                }
                return m_waveTexture;
            }
        }

        public TexturedBatch3D FlatBatch3D {
            get {
                if (Records.Count == 0) {
                    return null;
                }
                if (m_texturedBatch3D == null
                    || Records.Count != RecordsCountAtLastGenerateFlatBatch3D) {
                    m_texturedBatch3D = m_primitivesRenderer3D.TexturedBatch(
                        WaveTexture,
                        false,
                        0,
                        DepthStencilState.DepthRead,
                        RasterizerState.CullCounterClockwiseScissor,
                        BlendState.NonPremultiplied,
                        SamplerState.AnisotropicClamp
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
                }
            }
            else {
                DisplayCount = 512;
            }
        }

        public void DecreaseDisplayCount() {
            int index = Array.IndexOf(displayCountArray, DisplayCount);
            if (index > -1) {
                if (index != 0) {
                    DisplayCount = displayCountArray[index + 1];
                }
            }
            else {
                DisplayCount = 512;
            }
        }

        public void InCreaseMinLevel() {
            AutoSetMinMaxLevelMode = false;
            if (MaxLevel <= 64u) {
                MinLevel = 0u;
                return;
            }
            MinLevel = GetNearest16Multiples(MinLevel + (MaxLevel - MinLevel) / 4u);
            if (MaxLevel - MinLevel <= 64u) {
                MinLevel = MaxLevel - 64u;
            }
        }

        public void DecreaseMinLevel() {
            AutoSetMinMaxLevelMode = false;
            uint num = (MaxLevel - MinLevel) / 3u;
            MinLevel = num > MinLevel ? 0u : GetNearest16Multiples(MinLevel - num);
        }

        public void InCreaseMaxLevel() {
            AutoSetMinMaxLevelMode = false;
            uint num = (MaxLevel - MinLevel) / 3u;
            MaxLevel = num > uint.MaxValue - MaxLevel ? uint.MaxValue : GetNearest16Multiples(MaxLevel + num);
        }

        public void DecreaseMaxLevel() {
            AutoSetMinMaxLevelMode = false;
            if (MinLevel >= uint.MaxValue - 64u) {
                MaxLevel = uint.MaxValue;
                return;
            }
            MaxLevel = GetNearest16Multiples(MaxLevel - (MaxLevel - MinLevel) / 4u);
            if (MaxLevel - MinLevel <= 64u) {
                MaxLevel = GetNearest16Multiples(MinLevel + 64u);
            }
        }

        public void AutoSetMinMaxLevel() {
            AutoSetMinMaxLevelMode = true;
            if (Records.Count == 0
                || (AutoSetMinMaxLevelMode && Records.Count == RecordsCountAtAutoSetMinMaxLevel)) {
                return;
            }
            uint minRecord = uint.MaxValue;
            uint maxRecord = 0u;
            for (int i = 0; i < Math.Min(DisplayCount, Records.Count); i++) {
                uint[] record = Records[Records.Count - i - 1];
                bool flag = false;
                if (minRecord == 0) {
                    flag = true;
                }
                else {
                    minRecord = Math.Min(record.Min(), minRecord);
                }
                if (maxRecord == uint.MaxValue) {
                    if (flag) {
                        break;
                    }
                }
                else {
                    maxRecord = Math.Max(record.Max(), maxRecord);
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
        }

        public static uint GetNearest16Multiples(uint value) => (value + 63u) & 0xFFFFFFF0u;
    }
}