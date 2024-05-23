using System;
using System.Collections.Generic;
using System.Globalization;
using Engine;
using Engine.Graphics;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVOscilloscopeBlockBehavior : SubsystemBlockBehavior, IDrawable {
        public SubsystemSky m_subsystemSky;
        public readonly Dictionary<Point3, GVOscilloscopeData> m_datas = new();
        public readonly PrimitivesRenderer3D m_primitivesRenderer3D = new();
        public readonly GVOscilloscopePrimitivesRenderer2D m_primitivesRenderer2D = new();
        public TexturedBatch2D m_numberBatch;
        public TexturedBatch2D m_arrowButtonBatch;
        public TexturedBatch2D m_autoButtonBatch;
        public TexturedBatch2D m_moonButtonBatch;
        public TexturedBatch2D m_sunButtonBatch;

        public static readonly Dictionary<Rectangle, Action<GVOscilloscopeData>> m_interacts = new() {
            { new Rectangle(76, 20, 88, 88), data => data.IncreaseMaxLevel() },
            { new Rectangle(188, 20, 88, 88), data => data.DecreaseMaxLevel() },
            { new Rectangle(76, 916, 88, 88), data => data.IncreaseMinLevel() },
            { new Rectangle(188, 916, 88, 88), data => data.DecreaseMinLevel() },
            { new Rectangle(412, 20, 88, 88), data => data.IncreaseDisplayCount() },
            { new Rectangle(524, 20, 88, 88), data => data.DecreaseDisplayCount() }, {
                new Rectangle(916, 20, 88, 88), data => {
                    data.DisplayBloom = !data.DisplayBloom;
                    if (!data.IsTextureObsolete()) {
                        data.Texture.Dispose();
                    }
                }
            }, {
                new Rectangle(804, 20, 88, 88), data => {
                    if (!data.AutoSetMinMaxLevelMode) {
                        data.AutoSetMinMaxLevel(!data.IsTextureObsolete());
                    }
                }
            }
        };

        public static int[] m_drawOrders = [110];

        public int[] DrawOrders => m_drawOrders;

        public override void Load(ValuesDictionary valuesDictionary) {
            base.Load(valuesDictionary);
            m_subsystemSky = Project.FindSubsystem<SubsystemSky>(true);
            m_numberBatch = m_primitivesRenderer2D.TexturedBatch(
                Project.FindSubsystem<SubsystemGV8NumberLedGlow>(true).batchCache.Texture,
                false,
                0,
                DepthStencilState.None,
                null,
                BlendState.AlphaBlend,
                SamplerState.PointClamp
            );
            m_arrowButtonBatch = m_primitivesRenderer2D.TexturedBatch(
                ContentManager.Get<Texture2D>("Textures/GVOscilloscopeArrowButton"),
                false,
                0,
                DepthStencilState.None,
                null,
                BlendState.AlphaBlend,
                SamplerState.PointClamp
            );
            m_autoButtonBatch = m_primitivesRenderer2D.TexturedBatch(
                ContentManager.Get<Texture2D>("Textures/GVOscilloscopeAutoButton"),
                false,
                0,
                DepthStencilState.None,
                null,
                BlendState.AlphaBlend,
                SamplerState.PointClamp
            );
            m_moonButtonBatch = m_primitivesRenderer2D.TexturedBatch(
                ContentManager.Get<Texture2D>("Textures/GVOscilloscopeMoonButton"),
                false,
                0,
                DepthStencilState.None,
                null,
                BlendState.AlphaBlend,
                SamplerState.PointClamp
            );
            m_sunButtonBatch = m_primitivesRenderer2D.TexturedBatch(
                ContentManager.Get<Texture2D>("Textures/GVOscilloscopeSunButton"),
                false,
                0,
                DepthStencilState.None,
                null,
                BlendState.AlphaBlend,
                SamplerState.PointClamp
            );
            foreach (ValuesDictionary value3 in valuesDictionary.GetValue<ValuesDictionary>("Blocks").Values) {
                try {
                    m_datas.Add(value3.GetValue<Point3>("Point"), new GVOscilloscopeData(this));
                }
                catch (Exception) {
                    // ignored
                }
            }
        }

        public override int[] HandledBlocks => [GVOscilloscopeBlock.Index];

        public override void Save(ValuesDictionary valuesDictionary) {
            base.Save(valuesDictionary);
            int num = 0;
            ValuesDictionary valuesDictionary2 = new();
            valuesDictionary.SetValue("Blocks", valuesDictionary2);
            foreach (KeyValuePair<Point3, GVOscilloscopeData> pair in m_datas) {
                ValuesDictionary valuesDictionary3 = new();
                valuesDictionary2.SetValue(num++.ToString(CultureInfo.InvariantCulture), valuesDictionary3);
                valuesDictionary3.SetValue("Point", pair.Key);
            }
        }

        public GVOscilloscopeData GetData(Point3 point) {
            if (m_datas.TryGetValue(point, out GVOscilloscopeData result)) {
                return result;
            }
            GVOscilloscopeData data = new(this);
            m_datas.Add(point, data);
            return data;
        }

        public void RemoveData(Point3 point) {
            m_datas.Remove(point);
        }

        public void Draw(Camera camera, int drawOrder) {
            foreach (GVOscilloscopeData data in m_datas.Values) {
                if (data.RecordsCount > 0) {
                    Vector3 vector = data.Position - camera.ViewPosition;
                    float num = Vector3.Dot(vector, camera.ViewDirection);
                    if (num > 0.01f) {
                        float num2 = vector.Length();
                        if (num2 < m_subsystemSky.ViewFogRange.Y) {
                            int newLodLevel = vector.LengthSquared() switch {
                                < 3f => 0, //全特效
                                < 5.3f => 1, //降低虚线精度
                                < 45f => 2, //关闭按钮显示、降低分辨率至512*512、进一步降低虚线精度
                                < 56f => 3, //关闭网格显示
                                < 81f => 4, //关闭标签显示
                                < 225f => 5, //降低分辨率至360*360，停止画点
                                _ => 6 //关闭泛光特效，降低分辨率至160*160
                            };
                            if (newLodLevel != data.LodLevel) {
                                data.LodLevel = newLodLevel;
                                if (!data.IsTextureObsolete()) {
                                    data.Texture.Dispose();
                                }
                            }
                            const float size = 0.5f;
                            Vector3 p = data.Position + size * (-data.Right - data.Up);
                            Vector3 p2 = data.Position + size * (data.Right - data.Up);
                            Vector3 p3 = data.Position + size * (data.Right + data.Up);
                            Vector3 p4 = data.Position + size * (-data.Right + data.Up);
                            data.FlatBatch3D.QueueQuad(
                                p,
                                p2,
                                p3,
                                p4,
                                new Vector2(1f, 1f),
                                Vector2.UnitY,
                                Vector2.Zero,
                                Vector2.UnitX,
                                Color.White
                            );
                        }
                    }
                }
            }
            m_primitivesRenderer3D.Flush(camera.ViewProjectionMatrix);
        }

        public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner) {
            Point3 blockPoint = raycastResult.CellFace.Point;
            int face = raycastResult.CellFace.Face;
            if (GVOscilloscopeBlock.GetMountingFace(Terrain.ExtractData(raycastResult.Value)) == face
                && m_datas.TryGetValue(blockPoint, out GVOscilloscopeData data)) {
                Vector3 hitPosition = raycastResult.HitPoint();
                Vector2 interactPosition = face switch {
                    0 => new Vector2(hitPosition.X - blockPoint.X, blockPoint.Y + 1 - hitPosition.Y),
                    1 => new Vector2(blockPoint.Z + 1 - hitPosition.Z, blockPoint.Y + 1 - hitPosition.Y),
                    2 => new Vector2(hitPosition.X - blockPoint.X, blockPoint.Y + 1 - hitPosition.Y),
                    3 => new Vector2(hitPosition.Z - blockPoint.Z, blockPoint.Y + 1 - hitPosition.Y),
                    4 => new Vector2(hitPosition.X - blockPoint.X, hitPosition.Z - blockPoint.Z),
                    5 => new Vector2(hitPosition.X - blockPoint.X, blockPoint.Z + 1 - hitPosition.Z),
                    _ => Vector2.Zero
                };
                interactPosition *= 1024f;
                bool flag = true;
                foreach (KeyValuePair<Rectangle, Action<GVOscilloscopeData>> pair in m_interacts) {
                    Rectangle rectangle = pair.Key;
                    if (interactPosition.X >= rectangle.Left
                        && interactPosition.X < rectangle.Left + rectangle.Width
                        && interactPosition.Y >= rectangle.Top
                        && interactPosition.Y < rectangle.Top + rectangle.Height) {
                        pair.Value(data);
                        flag = false;
                        break;
                    }
                }
                if (flag) {
                    data.DisplayButtons = !data.DisplayButtons;
                }
                return true;
            }
            return false;
        }
    }
}