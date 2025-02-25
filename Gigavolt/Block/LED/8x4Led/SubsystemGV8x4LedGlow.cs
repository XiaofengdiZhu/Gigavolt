using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Engine.Media;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGV8x4LedGlow : Subsystem, IDrawable {
        public SubsystemSky m_subsystemSky;

        public readonly Dictionary<uint, HashSet<GV8x4GlowPoint>> m_glowPoints = new();
        public readonly PrimitivesRenderer3D m_primitivesRenderer = new();
        public readonly Dictionary<uint, TexturedBatch3D>[] batchCache = [new(), new(), new()];

        public int[] DrawOrders => [110];

        public GV8x4GlowPoint AddGlowPoint(uint subterrainId) {
            GV8x4GlowPoint glowPoint = new();
            if (m_glowPoints.TryGetValue(subterrainId, out HashSet<GV8x4GlowPoint> points)) {
                points.Add(glowPoint);
            }
            else {
                m_glowPoints.Add(subterrainId, [glowPoint]);
            }
            return glowPoint;
        }

        public void RemoveGlowPoint(GV8x4GlowPoint glowPoint, uint subterrainId) {
            m_glowPoints[subterrainId]?.Remove(glowPoint);
        }

        public void Draw(Camera camera, int drawOrder) {
            foreach ((uint subterrainId, HashSet<GV8x4GlowPoint> points) in m_glowPoints) {
                if (points.Count == 0) {
                    continue;
                }
                Matrix transform = subterrainId == 0 ? default : GVStaticStorage.GVSubterrainSystemDictionary[subterrainId].GlobalTransform;
                foreach (GV8x4GlowPoint key in points) {
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
                                position -= (0.01f + 0.02f * dotResult) / distance * direction;
                                Vector3 p = position + size * (-right - up);
                                Vector3 p2 = position + size * (right - up);
                                Vector3 p3 = position + size * (right + up);
                                Vector3 p4 = position + size * (-right + up);
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
            }
            m_primitivesRenderer.Flush(camera.ViewProjectionMatrix);
        }

        public override void Load(ValuesDictionary valuesDictionary) {
            m_subsystemSky = Project.FindSubsystem<SubsystemSky>(true);
        }

        public static TexturedBatch3D generateBatch(PrimitivesRenderer3D renderer, int type, uint voltage) {
            int width = type > 1 ? 8 : 4;
            int height = type > 0 ? 4 : 2;
            bool heightX2 = type != 1;
            Image image = new(width, width);
            for (int y = 0; y < height; y++) {
                for (int x = 0; x < width; x++) {
                    int index = y * width + x;
                    if (((voltage >> index) & 1u) == 1u) {
                        image.SetPixelFast(x, heightX2 ? y * 2 : y, SixLabors.ImageSharp.Color.White);
                        if (heightX2) {
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