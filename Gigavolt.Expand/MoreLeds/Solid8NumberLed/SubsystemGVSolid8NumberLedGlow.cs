using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVSolid8NumberLedGlow : Subsystem, IDrawable {
        public SubsystemSky m_subsystemSky;

        public readonly Dictionary<uint, HashSet<GVSolid8NumberGlowPoint>> m_glowPoints = new();
        public TexturedBatch3D batchCache;

        public int[] DrawOrders => [110];

        public GVSolid8NumberGlowPoint AddGlowPoint(uint subterrainId) {
            GVSolid8NumberGlowPoint glowPoint = new();
            if (m_glowPoints.TryGetValue(subterrainId, out HashSet<GVSolid8NumberGlowPoint> points)) {
                points.Add(glowPoint);
            }
            else {
                m_glowPoints.Add(subterrainId, [glowPoint]);
            }
            return glowPoint;
        }

        public void RemoveGlowPoint(GVSolid8NumberGlowPoint glowPoint, uint subterrainId) {
            m_glowPoints[subterrainId]?.Remove(glowPoint);
        }

        public void Draw(Camera camera, int drawOrder) {
            foreach ((uint subterrainId, HashSet<GVSolid8NumberGlowPoint> points) in m_glowPoints) {
                if (points.Count == 0) {
                    continue;
                }
                Matrix transform = subterrainId == 0 ? default : GVStaticStorage.GVSubterrainSystemDictionary[subterrainId].GlobalTransform;
                foreach (GVSolid8NumberGlowPoint key in points) {
                    if (key.Voltage > 0) {
                        Vector3 positionVector3 = new(key.Position.X + 0.5f, key.Position.Y + 0.5f, key.Position.Z + 0.5f);
                        for (int face = 0; face < CellFace.m_faceToVector3.Length; face++) {
                            Vector3 forward = CellFace.m_faceToVector3[face];
                            Vector3 position = subterrainId == 0
                                ? positionVector3 + forward * 0.525f
                                : Vector3.Transform(positionVector3 + forward * 0.525f, transform);
                            Vector3 direction = position - camera.ViewPosition;
                            float dotResult = Vector3.Dot(direction, camera.ViewDirection);
                            if (Vector3.Dot(direction, camera.ViewDirection) > 0.01f) {
                                float distance = direction.Length();
                                if (distance < m_subsystemSky.VisibilityRange) {
                                    Vector3 up = face < 4 ? Vector3.UnitY :
                                        Math.Abs(direction.X) > Math.Abs(direction.Z) ?
                                            new Vector3((direction.X > 0 ? 1 : -1) * (direction.Y < 0 ? 1 : -1), 0, 0) :
                                            new Vector3(0, 0, (direction.Z > 0 ? 1 : -1) * (direction.Y < 0 ? 1 : -1));
                                    Vector3 right = Vector3.Cross(forward, up);
                                    float size = 0.5f;
                                    if (subterrainId != 0) {
                                        Matrix orientation = transform.OrientationMatrix;
                                        right = Vector3.Normalize(Vector3.Transform(right, orientation));
                                        up = Vector3.Normalize(Vector3.Transform(up, orientation));
                                        size *= MathF.Sqrt(
                                            transform.M11 * transform.M11 + transform.M12 * transform.M12 + transform.M13 * transform.M13
                                        );
                                    }
                                    SubsystemGV8NumberLedGlow.Draw8Number(
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
            }
            batchCache.Flush(camera.ViewProjectionMatrix);
        }

        public override void Load(ValuesDictionary valuesDictionary) {
            m_subsystemSky = Project.FindSubsystem<SubsystemSky>(true);
            batchCache = new TexturedBatch3D {
                BlendState = BlendState.AlphaBlend,
                SamplerState = SamplerState.PointClamp,
                Texture = Project.FindSubsystem<SubsystemGV8NumberLedGlow>(true).batchCache.Texture,
                DepthStencilState = DepthStencilState.Default,
                RasterizerState = RasterizerState.CullCounterClockwiseScissor,
                UseAlphaTest = false
            };
        }
    }
}