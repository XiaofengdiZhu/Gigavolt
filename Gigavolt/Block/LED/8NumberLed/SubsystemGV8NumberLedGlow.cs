using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGV8NumberLedGlow : Subsystem, IDrawable {
        public SubsystemSky m_subsystemSky;

        public readonly Dictionary<GV8NumberGlowPoint, bool> m_glowPoints = new();

        public readonly PrimitivesRenderer3D m_primitivesRenderer = new();

        public TexturedBatch3D batchCache;

        public static int[] m_drawOrders = [110];

        public int[] DrawOrders => m_drawOrders;

        public GV8NumberGlowPoint AddGlowPoint() {
            GV8NumberGlowPoint glowPoint = new();
            m_glowPoints.Add(glowPoint, true);
            return glowPoint;
        }

        public void RemoveGlowPoint(GV8NumberGlowPoint glowPoint) {
            m_glowPoints.Remove(glowPoint);
        }

        public void Draw(Camera camera, int drawOrder) {
            foreach (GV8NumberGlowPoint key in m_glowPoints.Keys) {
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
                            Draw8Number(
                                batchCache,
                                key.Voltage,
                                key.Position,
                                num3,
                                key.Right,
                                key.Up,
                                Color.White
                            );
                        }
                    }
                }
            }
            batchCache.Flush(camera.ViewProjectionMatrix);
        }

        public static void Draw8Number(TexturedBatch3D batch, uint voltage, Vector3 position, float size, Vector3 right, Vector3 up, Color color) {
            Vector3 p = position + size * (right + up);
            size *= 2f;
            for (int y = 0; y < 2; y++) {
                for (int x = 0; x < 4; x++) {
                    int index = y * 4 + x;
                    uint number = (voltage >> (index * 4)) & 15u;
                    float px1 = (12 - x * 4) / 16f;
                    float px2 = px1 + 3 / 16f;
                    float py1 = (y == 1 ? 2 : 9) / 16f;
                    float py2 = py1 + 5 / 16f;
                    float tx1 = number % 4 * 3f / 12f;
                    float tx2 = tx1 + 3f / 12f;
                    float ty1 = number / 4 * 5f / 20f;
                    float ty2 = ty1 + 5f / 20f;
                    batch.QueueQuad(
                        p - (right * px1 + up * py1) * size,
                        p - (right * px2 + up * py1) * size,
                        p - (right * px2 + up * py2) * size,
                        p - (right * px1 + up * py2) * size,
                        new Vector2(tx1, ty1),
                        new Vector2(tx2, ty1),
                        new Vector2(tx2, ty2),
                        new Vector2(tx1, ty2),
                        color
                    );
                }
            }
        }

        public override void Load(ValuesDictionary valuesDictionary) {
            m_subsystemSky = Project.FindSubsystem<SubsystemSky>(true);
            batchCache = m_primitivesRenderer.TexturedBatch(
                ContentManager.Get<Texture2D>("Textures/GV8NumberLed"),
                false,
                0,
                DepthStencilState.Default,
                RasterizerState.CullCounterClockwiseScissor,
                BlendState.AlphaBlend,
                SamplerState.PointClamp
            );
        }
    }
}