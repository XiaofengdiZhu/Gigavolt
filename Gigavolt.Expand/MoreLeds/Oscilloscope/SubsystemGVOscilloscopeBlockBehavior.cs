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
        public readonly GVPrimitivesRenderer2D m_primitivesRenderer2D = new();
        public TexturedBatch2D m_numberBatch;
        public TexturedBatch2D m_arrowButtonBatch;
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
            if (m_datas.TryGetValue(raycastResult.CellFace.Point, out GVOscilloscopeData data)) {
                data.DisplayButtons = !data.DisplayButtons;
                return true;
            }
            return false;
        }
    }
}