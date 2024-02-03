using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVOneLedGlow : Subsystem, IDrawable {
        public SubsystemSky m_subsystemSky;

        public Dictionary<GVGlowPoint, bool> m_glowPoints = new();

        public PrimitivesRenderer3D m_primitivesRenderer = new();

        public FlatBatch3D m_batche;

        public static int[] m_drawOrders = { 110 };

        public int[] DrawOrders => m_drawOrders;

        public GVGlowPoint AddGlowPoint() {
            GVGlowPoint glowPoint = new();
            m_glowPoints.Add(glowPoint, true);
            return glowPoint;
        }

        public void RemoveGlowPoint(GVGlowPoint glowPoint) {
            m_glowPoints.Remove(glowPoint);
        }

        public void Draw(Camera camera, int drawOrder) {
            foreach (GVGlowPoint key in m_glowPoints.Keys) {
                if (key.Color.A > 0) {
                    Vector3 vector = key.Position - camera.ViewPosition;
                    float num = Vector3.Dot(vector, camera.ViewDirection);
                    if (num > 0.01f) {
                        float num2 = vector.Length();
                        if (num2 < m_subsystemSky.ViewFogRange.Y) {
                            float num3 = key.Size;
                            if (key.FarDistance > 0f) {
                                num3 += (key.FarSize - key.Size) * MathUtils.Saturate(num2 / key.FarDistance);
                            }
                            //Vector3 v = (0f - (0.01f + 0.02f * num)) / num2 * vector;
                            Vector3 v = Vector3.Zero;
                            Vector3 p = key.Position + num3 * (-key.Right - key.Up) + v;
                            Vector3 p2 = key.Position + num3 * (key.Right - key.Up) + v;
                            Vector3 p3 = key.Position + num3 * (key.Right + key.Up) + v;
                            Vector3 p4 = key.Position + num3 * (-key.Right + key.Up) + v;
                            m_batche.QueueQuad(
                                p,
                                p2,
                                p3,
                                p4,
                                key.Color
                            );
                        }
                    }
                }
            }
            m_primitivesRenderer.Flush(camera.ViewProjectionMatrix);
        }

        public override void Load(ValuesDictionary valuesDictionary) {
            m_subsystemSky = Project.FindSubsystem<SubsystemSky>(true);
            m_batche = m_primitivesRenderer.FlatBatch(0, DepthStencilState.DepthRead, RasterizerState.CullCounterClockwiseScissor, BlendState.NonPremultiplied);
        }
    }
}