using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVGlow : Subsystem, IDrawable {
        public SubsystemSky m_subsystemSky;

        public readonly Dictionary<uint, HashSet<GVGlowPoint>> m_glowPoints = [];
        public readonly PrimitivesRenderer3D m_primitivesRenderer = new();
        public TexturedBatch3D[] m_texturedBatchesByType = new TexturedBatch3D[4];
        public FlatBatch3D m_flatBatch;

        public int[] DrawOrders => [110];

        public GVGlowPoint AddGlowPoint(uint subterrainId) {
            GVGlowPoint glowPoint = new();
            if (m_glowPoints.TryGetValue(subterrainId, out HashSet<GVGlowPoint> points)) {
                points.Add(glowPoint);
            }
            else {
                m_glowPoints.Add(subterrainId, [glowPoint]);
            }
            return glowPoint;
        }

        public void RemoveGlowPoint(GVGlowPoint glowPoint, uint subterrainId) {
            m_glowPoints[subterrainId]?.Remove(glowPoint);
        }

        public void Draw(Camera camera, int drawOrder) {
            foreach ((uint subterrainId, HashSet<GVGlowPoint> points) in m_glowPoints) {
                if (points.Count == 0) {
                    continue;
                }
                Matrix transform = subterrainId == 0 ? default : GVStaticStorage.GVSubterrainSystemDictionary[subterrainId].GlobalTransform;
                foreach (GVGlowPoint key in points) {
                    if (key.Color.A > 0) {
                        Vector3 position = subterrainId == 0 ? key.Position : Vector3.Transform(key.Position, transform);
                        Vector3 direction = position - camera.ViewPosition;
                        float dotResult = Vector3.Dot(direction, camera.ViewDirection);
                        if (dotResult > 0.01f) {
                            float distance = direction.Length();
                            if (distance < m_subsystemSky.ViewFogRange.Y) {
                                Vector3 right = key.Right;
                                Vector3 up = key.Up;
                                float size = key.Size;
                                if (subterrainId != 0) {
                                    Matrix orientation = transform.OrientationMatrix;
                                    float scale = MathF.Sqrt(transform.M11 * transform.M11 + transform.M12 * transform.M12 + transform.M13 * transform.M13);
                                    size *= scale;
                                    right = Vector3.Transform(right, orientation) / scale;
                                    up = Vector3.Transform(up, orientation) / scale;
                                }
                                position -= (0.01f + 0.02f * dotResult) / distance * direction;
                                Vector3 p = position + size * (-right - up);
                                Vector3 p2 = position + size * (right - up);
                                Vector3 p3 = position + size * (right + up);
                                Vector3 p4 = position + size * (-right + up);
                                if (key.Type == GVGlowPointType.Full) {
                                    m_flatBatch.QueueQuad(
                                        p,
                                        p2,
                                        p3,
                                        p4,
                                        key.Color
                                    );
                                }
                                else {
                                    m_texturedBatchesByType[(int)key.Type]
                                    .QueueQuad(
                                        p,
                                        p2,
                                        p3,
                                        p4,
                                        Vector2.Zero,
                                        Vector2.UnitX,
                                        Vector2.One,
                                        Vector2.UnitY,
                                        key.Color
                                    );
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
            m_texturedBatchesByType = Project.FindSubsystem<SubsystemGlow>().m_batchesByType;
            m_flatBatch = m_primitivesRenderer.FlatBatch(0, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwiseScissor, BlendState.NonPremultiplied);
        }
    }
}