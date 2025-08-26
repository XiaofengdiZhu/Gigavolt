using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGV8NumberLedGlow : Subsystem, IDrawable {
        public SubsystemSky m_subsystemSky;

        public readonly Dictionary<uint, HashSet<GV8NumberGlowPoint>> m_glowPoints = new();
        public TexturedBatch3D batchCache;

        public int[] DrawOrders => [110];

        public GV8NumberGlowPoint AddGlowPoint(uint subterrainId) {
            GV8NumberGlowPoint glowPoint = new();
            if (m_glowPoints.TryGetValue(subterrainId, out HashSet<GV8NumberGlowPoint> points)) {
                points.Add(glowPoint);
            }
            else {
                m_glowPoints.Add(subterrainId, [glowPoint]);
            }
            return glowPoint;
        }

        public void RemoveGlowPoint(GV8NumberGlowPoint glowPoint, uint subterrainId) {
            m_glowPoints[subterrainId]?.Remove(glowPoint);
        }

        public void Draw(Camera camera, int drawOrder) {
            foreach ((uint subterrainId, HashSet<GV8NumberGlowPoint> points) in m_glowPoints) {
                if (points.Count == 0) {
                    continue;
                }
                Matrix transform = subterrainId == 0 ? default : GVStaticStorage.GVSubterrainSystemDictionary[subterrainId].GlobalTransform;
                foreach (GV8NumberGlowPoint key in points) {
                    if (key.Voltage > 0) {
                        Vector3 position = subterrainId == 0 ? key.Position : Vector3.Transform(key.Position, transform);
                        Vector3 direction = position - camera.ViewPosition;
                        float dotResult = Vector3.Dot(direction, camera.ViewDirection);
                        if (Vector3.Dot(direction, camera.ViewDirection) > 0.01f) {
                            float distance = direction.Length();
                            if (distance < m_subsystemSky.VisibilityRange) {
                                Vector3 right = key.Right;
                                Vector3 up = key.Up;
                                float size = key.Size;
                                if (subterrainId != 0) {
                                    Matrix orientation = transform.OrientationMatrix;
                                    right = Vector3.Normalize(Vector3.Transform(right, orientation));
                                    up = Vector3.Normalize(Vector3.Transform(up, orientation));
                                    size *= MathF.Sqrt(transform.M11 * transform.M11 + transform.M12 * transform.M12 + transform.M13 * transform.M13);
                                }
                                Draw8Number(
                                    batchCache,
                                    key.Voltage,
                                    position - (0.01f + 0.02f * dotResult) / distance * direction,
                                    size,
                                    right,
                                    up,
                                    Color.White
                                );
                            }
                        }
                    }
                }
            }
            batchCache.Flush(camera.ViewProjectionMatrix);
        }

        public static void Draw8Number(TexturedBatch3D batch,
            uint voltage,
            Vector3 center,
            float halfSize,
            Vector3 right,
            Vector3 up,
            Color color) {
            Vector3 p = center + halfSize * (right + up);
            float size = halfSize * 2f;
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
            batchCache = new TexturedBatch3D {
                BlendState = BlendState.AlphaBlend,
                SamplerState = SamplerState.PointClamp,
                Texture = ContentManager.Get<Texture2D>("Textures/GV8NumberLed"),
                DepthStencilState = DepthStencilState.Default,
                RasterizerState = RasterizerState.CullCounterClockwiseScissor,
                UseAlphaTest = false
            };
        }
    }
}