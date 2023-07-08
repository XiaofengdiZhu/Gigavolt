using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Engine.Media;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGV8NumberLedGlow : Subsystem, IDrawable {
        public SubsystemSky m_subsystemSky;

        public Dictionary<GV8NumberGlowPoint, bool> m_glowPoints = new Dictionary<GV8NumberGlowPoint, bool>();

        public PrimitivesRenderer3D m_primitivesRenderer = new PrimitivesRenderer3D();

        public Dictionary<uint, TexturedBatch3D> batchCache;
        public static List<Point2>[] number2Pixels = new List<Point2>[16];

        public static int[] m_drawOrders = { 110 };

        public int[] DrawOrders => m_drawOrders;

        public GV8NumberGlowPoint AddGlowPoint() {
            GV8NumberGlowPoint glowPoint = new GV8NumberGlowPoint();
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
                            Vector3 v = (0f - (0.01f + 0.02f * num)) / num2 * vector;
                            Vector3 p = key.Position + num3 * (-key.Right - key.Up) + v;
                            Vector3 p2 = key.Position + num3 * (key.Right - key.Up) + v;
                            Vector3 p3 = key.Position + num3 * (key.Right + key.Up) + v;
                            Vector3 p4 = key.Position + num3 * (-key.Right + key.Up) + v;
                            if (batchCache.TryGetValue(key.Voltage, out TexturedBatch3D batch)) {
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
                                TexturedBatch3D newBatch = generateBatch(m_primitivesRenderer, key.Voltage);
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
                                batchCache.Add(key.Voltage, newBatch);
                            }
                        }
                    }
                }
            }
            m_primitivesRenderer.Flush(camera.ViewProjectionMatrix);
        }

        public override void Load(ValuesDictionary valuesDictionary) {
            m_subsystemSky = Project.FindSubsystem<SubsystemSky>(true);
            batchCache = new Dictionary<uint, TexturedBatch3D>();
            for (int number = 0; number < 16; number++) {
                List<Point2> points = new List<Point2>();
                Image image = ContentManager.Get<Image>($"Textures/GV8NumberLed/{number}");
                for (int x = 0; x < image.Width; x++) {
                    for (int y = 0; y < image.Height; y++) {
                        if (image.GetPixel(x, y) == Color.White) {
                            points.Add(new Point2(x, y));
                        }
                    }
                }
                number2Pixels[number] = points;
            }
        }

        public static TexturedBatch3D generateBatch(PrimitivesRenderer3D renderer, uint voltage) {
            Image image = new Image(16, 16);
            for (int y = 0; y < 2; y++) {
                for (int x = 0; x < 4; x++) {
                    int index = y * 4 + x;
                    uint number = (voltage >> (index * 4)) & 15u;
                    Point2 origin = new Point2(12 - x * 4, y == 1 ? 2 : 9);
                    foreach (Point2 point in number2Pixels[number]) {
                        image.SetPixel(origin.X + point.X, origin.Y + point.Y, Color.White);
                    }
                }
            }
            return renderer.TexturedBatch(
                Texture2D.Load(image),
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