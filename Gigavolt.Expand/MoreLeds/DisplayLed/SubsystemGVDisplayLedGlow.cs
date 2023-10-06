using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using Engine.Media;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVDisplayLedGlow : Subsystem, IDrawable {
        public SubsystemSky m_subsystemSky;
        public SubsystemTerrain m_subsystemTerrain;
        public readonly DrawBlockEnvironmentData m_drawBlockEnvironmentData = new DrawBlockEnvironmentData();
        public readonly Dictionary<List<GVDisplayPoint>, bool> m_points = new Dictionary<List<GVDisplayPoint>, bool>();
        public Texture2D BlocksTexture;
        public TexturedBatch3D[] m_batches = new TexturedBatch3D[2];

        public PrimitivesRenderer3D m_primitivesRenderer = new PrimitivesRenderer3D();

        public static int[] m_drawOrders = { 112 };

        public int[] DrawOrders => m_drawOrders;

        public List<GVDisplayPoint> AddGlowPoints() {
            List<GVDisplayPoint> glowPoint = new List<GVDisplayPoint>();
            m_points.Add(glowPoint, true);
            return glowPoint;
        }

        public void RemoveGlowPoints(List<GVDisplayPoint> glowPoint) {
            m_points.Remove(glowPoint);
        }

        public void Draw(Camera camera, int drawOrder) {
            m_drawBlockEnvironmentData.SubsystemTerrain = m_subsystemTerrain;
            m_drawBlockEnvironmentData.InWorldMatrix = Matrix.Identity;
            foreach (List<GVDisplayPoint> points in m_points.Keys) {
                foreach (GVDisplayPoint key in points) {
                    //Console.WriteLine($"{key.Position} {key.Color} {key.Size} {key.Value} {key.Complex} {key.Type} {key.CustomBit}");
                    if (!key.isValid()) {
                        continue;
                    }
                    Vector3 position = key.Position;
                    Matrix matrix = Matrix.CreateFromYawPitchRoll(key.Rotation.X, key.Rotation.Y, key.Rotation.Z);
                    matrix.Translation = position;
                    //方块展示
                    if (key.Type == 0) {
                        int value = (int)key.Value;
                        int id = Terrain.ExtractContents(value);
                        if (id == 0) {
                            continue;
                        }
                        Block block = BlocksManager.Blocks[id];
                        float size = key.Complex ? key.Size : block.InHandScale;
                        if (key.Complex ? camera.ViewFrustum.Intersection(new BoundingSphere(position, size)) : camera.ViewFrustum.Intersection(position + camera.ViewDirection)) {
                            int x = Terrain.ToCell(position.X);
                            int y = Terrain.ToCell(position.Y);
                            int z = Terrain.ToCell(position.Z);
                            TerrainChunk chunkAtCell = m_subsystemTerrain.Terrain.GetChunkAtCell(x, z);
                            if (chunkAtCell != null
                                && chunkAtCell.State >= TerrainChunkState.InvalidVertices1
                                && y >= 0
                                && y < 255) {
                                m_drawBlockEnvironmentData.Humidity = m_subsystemTerrain.Terrain.GetSeasonalHumidity(x, z);
                                m_drawBlockEnvironmentData.Temperature = m_subsystemTerrain.Terrain.GetSeasonalTemperature(x, z) + SubsystemWeather.GetTemperatureAdjustmentAtHeight(y);
                            }
                            m_drawBlockEnvironmentData.Light = key.Complex ? 15 : m_subsystemTerrain.Terrain.GetCellLightFast(x, y, z);
                            m_drawBlockEnvironmentData.BillboardDirection = camera.ViewDirection;
                            m_drawBlockEnvironmentData.InWorldMatrix.Translation = position;
                            block.DrawBlock(
                                m_primitivesRenderer,
                                value,
                                key.Color,
                                size,
                                ref matrix,
                                m_drawBlockEnvironmentData
                            );
                        }
                    }
                    else {
                        if (!GVStaticStorage.GVMBIDDataDictionary.TryGetValue(key.Value, out GVArrayData data)
                            || data == null) {
                            continue;
                        }
                        float halfWidth;
                        float halfHeight;
                        Image imageData = data.GetImage();
                        if (imageData == null) {
                            continue;
                        }
                        int dataWidth = imageData.Width;
                        int dataHeight = imageData.Height;
                        if (dataWidth > dataHeight) {
                            halfWidth = 0.5f;
                            halfHeight = dataHeight / (float)dataWidth * 0.5f;
                        }
                        else {
                            halfWidth = dataWidth / (float)dataHeight * 0.5f;
                            halfHeight = 0.5f;
                        }
                        int lightValue;
                        Vector3 forward;
                        if (key.Complex) {
                            lightValue = 15;
                            halfWidth *= key.Size;
                            halfHeight *= key.Size;
                            forward = Vector3.Zero;
                        }
                        else {
                            lightValue = m_subsystemTerrain.Terrain.GetCellLightFast((int)MathUtils.Floor(position.X), (int)MathUtils.Floor(position.Y), (int)MathUtils.Floor(position.Z));
                            forward = matrix.Forward * 0.43f;
                        }
                        Vector3 right = matrix.Right * halfWidth;
                        Vector3 up = matrix.Up * halfHeight;
                        Vector3[] offsets = { right - up, right + up, -right - up, -right + up };
                        Vector3 min = Vector3.Zero;
                        Vector3 max = Vector3.Zero;
                        foreach (Vector3 offset in offsets) {
                            min.X = Math.Min(min.X, offset.X);
                            min.Y = Math.Min(min.Y, offset.Y);
                            min.Z = Math.Min(min.Z, offset.Z);
                            max.X = Math.Max(max.X, offset.X);
                            max.Y = Math.Max(max.Y, offset.Y);
                            max.Z = Math.Max(max.Z, offset.Z);
                        }
                        if (camera.ViewFrustum.Intersection(new BoundingBox(position + forward + min, position + forward + max))) {
                            SamplerState samplerState = key.CustomBit ? SamplerState.PointClamp : SamplerState.AnisotropicClamp;
                            Color color = Color.MultiplyColorOnly(key.Color, LightingManager.LightIntensityByLightValue[lightValue]);
                            if (key.Type == 2) {
                                // 绘制地层
                                m_primitivesRenderer.TexturedBatch(
                                        data.GetTerrainTexture2D(samplerState),
                                        false,
                                        0,
                                        DepthStencilState.DepthRead,
                                        RasterizerState.CullCounterClockwiseScissor,
                                        BlendState.NonPremultiplied,
                                        samplerState
                                    )
                                    .QueueQuad(
                                        position + right - up + forward,
                                        position - right - up + forward,
                                        position - right + up + forward,
                                        position + right + up + forward,
                                        new Vector2(1f, 1f),
                                        new Vector2(0f, 1f),
                                        new Vector2(0f, 0f),
                                        new Vector2(1f, 0f),
                                        color
                                    );
                            }
                            else {
                                //绘制图片
                                m_primitivesRenderer.TexturedBatch(
                                        data.GetTexture2D(),
                                        false,
                                        0,
                                        DepthStencilState.DepthRead,
                                        RasterizerState.CullCounterClockwiseScissor,
                                        BlendState.NonPremultiplied,
                                        samplerState
                                    )
                                    .QueueQuad(
                                        position + right - up + forward,
                                        position - right - up + forward,
                                        position - right + up + forward,
                                        position + right + up + forward,
                                        new Vector2(1f, 1f),
                                        new Vector2(0f, 1f),
                                        new Vector2(0f, 0f),
                                        new Vector2(1f, 0f),
                                        color
                                    );
                            }
                        }
                    }
                }
            }
            m_primitivesRenderer.Flush(camera.ViewProjectionMatrix);
        }

        public override void Load(ValuesDictionary valuesDictionary) {
            m_subsystemSky = Project.FindSubsystem<SubsystemSky>(true);
            m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(true);
            BlocksTexture = Project.FindSubsystem<SubsystemBlocksTexture>(true).BlocksTexture;
            m_batches[0] = m_primitivesRenderer.TexturedBatch(
                BlocksTexture,
                false,
                0,
                DepthStencilState.DepthRead,
                RasterizerState.CullCounterClockwiseScissor,
                BlendState.NonPremultiplied,
                SamplerState.AnisotropicClamp
            );
            m_batches[1] = m_primitivesRenderer.TexturedBatch(
                BlocksTexture,
                false,
                0,
                DepthStencilState.DepthRead,
                RasterizerState.CullCounterClockwiseScissor,
                BlendState.NonPremultiplied,
                SamplerState.PointClamp
            );
        }
    }
}