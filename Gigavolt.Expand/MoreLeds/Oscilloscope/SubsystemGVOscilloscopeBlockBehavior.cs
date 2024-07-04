using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Engine;
using Engine.Graphics;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVOscilloscopeBlockBehavior : SubsystemBlockBehavior, IDrawable {
        public SubsystemSky m_subsystemSky;
        public readonly Dictionary<uint, Dictionary<Point3, GVOscilloscopeData>> m_datas = new() { { 0u, new Dictionary<Point3, GVOscilloscopeData>() } };
        public readonly GVOscilloscopePrimitivesRenderer2D m_primitivesRenderer2D = new();
        public TexturedBatch2D m_numberBatch;
        public TexturedBatch2D m_arrowButtonBatch;
        public TexturedBatch2D m_autoButtonBatch;
        public TexturedBatch2D m_moonButtonBatch;
        public TexturedBatch2D m_sunButtonBatch;
        public bool m_isInScreen;

        public static readonly int[] m_drawOrders = [110];

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
                    GVOscilloscopeData data = new(this);
                    m_datas[0u].Add(value3.GetValue<Point3>("Point"), data);
                    data.DisplayBloom = value3.GetValue<bool>("DisplayBloom");
                    data.DisplayCount = value3.GetValue<int>("DisplayCount");
                    data.MaxLevel = value3.GetValue<uint>("MaxLevel");
                    data.MinLevel = value3.GetValue<uint>("MinLevel");
                    data.AutoSetMinMaxLevelMode = value3.GetValue<bool>("AutoSetMinMaxLevelMode");
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
            foreach (KeyValuePair<Point3, GVOscilloscopeData> pair in m_datas[0u]) {
                ValuesDictionary valuesDictionary3 = new();
                valuesDictionary2.SetValue(num++.ToString(CultureInfo.InvariantCulture), valuesDictionary3);
                valuesDictionary3.SetValue("Point", pair.Key);
                valuesDictionary3.SetValue("DisplayBloom", pair.Value.DisplayBloom);
                valuesDictionary3.SetValue("DisplayCount", pair.Value.DisplayCount);
                valuesDictionary3.SetValue("MaxLevel", pair.Value.MaxLevel);
                valuesDictionary3.SetValue("MinLevel", pair.Value.MinLevel);
                valuesDictionary3.SetValue("AutoSetMinMaxLevelMode", pair.Value.AutoSetMinMaxLevelMode);
            }
        }

        public GVOscilloscopeData GetData(Point3 point, uint subterrainId) {
            if (m_datas.TryGetValue(subterrainId, out Dictionary<Point3, GVOscilloscopeData> datas)) {
                if (datas.TryGetValue(point, out GVOscilloscopeData result)) {
                    return result;
                }
            }
            else {
                datas = new Dictionary<Point3, GVOscilloscopeData>();
                m_datas.Add(subterrainId, datas);
            }
            GVOscilloscopeData data = new(this);
            datas.Add(point, data);
            return data;
        }

        public void RemoveData(Point3 point, uint subterrainId) {
            if (m_datas.TryGetValue(subterrainId, out Dictionary<Point3, GVOscilloscopeData> datas)
                && datas.TryGetValue(point, out GVOscilloscopeData data)) {
                data.Dispose();
                datas.Remove(point);
            }
        }

        public void Draw(Camera camera, int drawOrder) {
            if (m_isInScreen) {
                return;
            }
            foreach ((uint subterrainId, Dictionary<Point3, GVOscilloscopeData> datas) in m_datas) {
                if (datas.Count == 0) {
                    continue;
                }
                bool inSubterrain = subterrainId != 0;
                Matrix transform = inSubterrain ? GVStaticStorage.GVSubterrainSystemDictionary[subterrainId].GlobalTransform : default;
                foreach (GVOscilloscopeData data in datas.Values) {
                    if (data.RecordsCount > 0
                        && data.ConnectionState.Any(state => state)) {
                        Vector3 position = data.Position;
                        if (inSubterrain) {
                            position = Vector3.Transform(data.Position, transform);
                        }
                        Vector3 vector = position - camera.ViewPosition;
                        if (Vector3.Dot(vector, camera.ViewDirection) > 0.01f) {
                            float num2 = vector.Length();
                            if (num2 < m_subsystemSky.ViewFogRange.Y) {
                                int newLodLevel = vector.LengthSquared() switch {
                                    < 3f => 0, //全特效
                                    < 5.3f => 1, //关闭按钮显示、降低虚线精度
                                    < 45f => 2, //降低分辨率至512*512、进一步降低虚线精度
                                    < 56f => 3, //关闭网格显示
                                    < 81f => 4, //关闭标签显示
                                    < 225f => 5, //降低分辨率至360*360，停止画点
                                    _ => 6 //关闭泛光特效，降低分辨率至160*160
                                };
                                if (newLodLevel != data.LodLevel) {
                                    data.LodLevel = newLodLevel;
                                    if (!data.IsTextureObsolete()) {
                                        data.Texture = null;
                                    }
                                }
                                Vector3 right = data.Right;
                                Vector3 up = data.Up;
                                float size = 0.5f;
                                if (subterrainId != 0) {
                                    Matrix orientation = transform.OrientationMatrix;
                                    float scale = MathF.Sqrt(transform.M11 * transform.M11 + transform.M12 * transform.M12 + transform.M13 * transform.M13);
                                    size *= scale;
                                    right = Vector3.Transform(right, orientation) / scale;
                                    up = Vector3.Transform(up, orientation) / scale;
                                }
                                Vector3 p = position + size * (-right - up);
                                Vector3 p2 = position + size * (right - up);
                                Vector3 p3 = position + size * (right + up);
                                Vector3 p4 = position + size * (-right + up);
                                TexturedBatch3D batch = data.TexturedBatch3D;
                                batch.QueueQuad(
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
                                batch.Flush(camera.ViewProjectionMatrix);
                            }
                        }
                    }
                }
            }
        }

        public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner) {
            if (raycastResult.Distance > 2.3f) {
                return true;
            }
            Point3 blockPoint = raycastResult.CellFace.Point;
            int face = raycastResult.CellFace.Face;
            if (GVOscilloscopeBlock.GetMountingFace(Terrain.ExtractData(raycastResult.Value)) == face
                && m_datas[0u].TryGetValue(blockPoint, out GVOscilloscopeData data)) {
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
                data.Interact(interactPosition * 1024, 1024f, 1024f);
                return true;
            }
            return false;
        }

        public override bool OnEditBlock(int x, int y, int z, int value, ComponentPlayer componentPlayer) {
            Point3 blockPoint = new(x, y, z);
            if (m_datas[0u].TryGetValue(blockPoint, out GVOscilloscopeData data)) {
                if (!ScreensManager.m_screens.ContainsKey("GVOscilloscopeScreen")) {
                    ScreensManager.AddScreen("GVOscilloscopeScreen", new GVOscilloscopeScreen());
                }
                ScreensManager.SwitchScreen("GVOscilloscopeScreen", blockPoint, data, this);
                m_isInScreen = true;
                return true;
            }
            return false;
        }
    }
}