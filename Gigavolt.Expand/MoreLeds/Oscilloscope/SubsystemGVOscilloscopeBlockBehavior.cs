using System;
using System.Collections.Generic;
using System.Globalization;
using Engine;
using Engine.Graphics;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVOscilloscopeBlockBehavior : Subsystem, IDrawable {
        public SubsystemSky m_subsystemSky;
        public Dictionary<Point3, GVOscilloscopeData> m_datas = new();
        public PrimitivesRenderer3D m_primitivesRenderer3D = new();
        public static int[] m_drawOrders = [110];

        public int[] DrawOrders => m_drawOrders;

        public override void Load(ValuesDictionary valuesDictionary) {
            base.Load(valuesDictionary);
            m_subsystemSky = Project.FindSubsystem<SubsystemSky>(true);
            foreach (ValuesDictionary value3 in valuesDictionary.GetValue<ValuesDictionary>("Blocks").Values) {
                try {
                    m_datas.Add(value3.GetValue<Point3>("Point"), new GVOscilloscopeData(m_primitivesRenderer3D));
                }
                catch (Exception) {
                    // ignored
                }
            }
        }

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
            GVOscilloscopeData data = new(m_primitivesRenderer3D);
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
    }
}