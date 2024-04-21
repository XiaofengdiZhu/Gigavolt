using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Engine.Media;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGV8x4LedGlow : Subsystem, IDrawable {
        public SubsystemSky m_subsystemSky;

        public Dictionary<GV8x4GlowPoint, bool> m_glowPoints = new();

        public PrimitivesRenderer3D m_primitivesRenderer = new();

        public Dictionary<uint, TexturedBatch3D>[] batchCache;

        public static int[] m_drawOrders = { 110 };

        public int[] DrawOrders => m_drawOrders;

        public GV8x4GlowPoint AddGlowPoint() {
            GV8x4GlowPoint glowPoint = new();
            m_glowPoints.Add(glowPoint, true);
            return glowPoint;
        }

        public void RemoveGlowPoint(GV8x4GlowPoint glowPoint) {
            m_glowPoints.Remove(glowPoint);
        }

        public void Draw(Camera camera, int drawOrder) {
            foreach (GV8x4GlowPoint key in m_glowPoints.Keys) {
                if (key.Voltage > 0) {
                    Vector3 vector = key.Position - camera.ViewPosition;
                    float num = Vector3.Dot(vector, camera.ViewDirection);
                    if (num > 0.01f) {
                        float num2 = vector.Length();
                        if (num2 < m_subsystemSky.ViewFogRange.Y) {
                            float num3 = key.Size;
                            if (key.FarDistance > 0f) {
                                num3 += (key.FarSize - key.Size) * MathUtils.Saturate(num2 / key.FarDistance);
                            }
                            Vector3 v = (0f - (0.01f + 0.02f * num)) / num2 * vector;
                            Vector3 p = key.Position + num3 * (-key.Right - key.Up) + v;
                            Vector3 p2 = key.Position + num3 * (key.Right - key.Up) + v;
                            Vector3 p3 = key.Position + num3 * (key.Right + key.Up) + v;
                            Vector3 p4 = key.Position + num3 * (-key.Right + key.Up) + v;
                            if (batchCache[key.Type].TryGetValue(key.Voltage, out TexturedBatch3D batch)) {
                                batch.QueueQuad(
                                    p,
                                    p2,
                                    p3,
                                    p4,
                                    new Vector2(1f, 1f),
                                    new Vector2(0f, 1f),
                                    new Vector2(0f, 0f),
                                    new Vector2(1f, 0f),
                                    Color.White
                                );
                            }
                            else {
                                TexturedBatch3D newBatch = generateBatch(m_primitivesRenderer, key.Type, key.Voltage);
                                newBatch.QueueQuad(
                                    p,
                                    p2,
                                    p3,
                                    p4,
                                    new Vector2(1f, 1f),
                                    new Vector2(0f, 1f),
                                    new Vector2(0f, 0f),
                                    new Vector2(1f, 0f),
                                    Color.White
                                );
                                batchCache[key.Type].Add(key.Voltage, newBatch);
                            }
                        }
                    }
                }
            }
            m_primitivesRenderer.Flush(camera.ViewProjectionMatrix);
        }

        public override void Load(ValuesDictionary valuesDictionary) {
            m_subsystemSky = Project.FindSubsystem<SubsystemSky>(true);
            batchCache = new[] { new Dictionary<uint, TexturedBatch3D>(), new Dictionary<uint, TexturedBatch3D>(), new Dictionary<uint, TexturedBatch3D>() };
        }

        public static TexturedBatch3D generateBatch(PrimitivesRenderer3D renderer, int type, uint voltage) {
            int width = type > 1 ? 8 : 4;
            int height = type > 0 ? 4 : 2;
            bool heightx2 = type != 1;
            Image image = new(width, width);
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    int index = y * width + x;
                    if (((voltage >> index) & 1u) == 1u) {
                        image.SetPixelFast(x, heightx2 ? y * 2 : y, SixLabors.ImageSharp.Color.White);
                        if (heightx2) {
                            image.SetPixelFast(x, y * 2 + 1, SixLabors.ImageSharp.Color.White);
                        }
                    }
                }
            }
            Texture2D texture = Texture2D.Load(image);
            return renderer.TexturedBatch(
                texture,
                false,
                0,
                DepthStencilState.DepthRead,
                RasterizerState.CullCounterClockwiseScissor,
                BlendState.AlphaBlend,
                SamplerState.PointClamp
            );
        }
    }
}