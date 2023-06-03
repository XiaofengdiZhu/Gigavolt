using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game {
    public class SubsystemGVDisplayLedGlow : Subsystem, IDrawable {
        public SubsystemSky m_subsystemSky;
        public SubsystemTerrain m_subsystemTerrain;
        public readonly DrawBlockEnvironmentData m_drawBlockEnvironmentData = new DrawBlockEnvironmentData();
        public readonly Dictionary<GVDisplayPoint, bool> m_points = new Dictionary<GVDisplayPoint, bool>();
        public Texture2D BlocksTexture;
        public TexturedBatch3D[] m_batches = new TexturedBatch3D[2];
        public Dictionary<uint, DateTime> m_updateTimes = new Dictionary<uint, DateTime>();
        public Dictionary<uint, Texture2D> m_textures = new Dictionary<uint, Texture2D>();

        public PrimitivesRenderer3D m_primitivesRenderer = new PrimitivesRenderer3D();

        public static int[] m_drawOrders = { 110 };

        public int[] DrawOrders => m_drawOrders;

        public GVDisplayPoint AddGlowPoint() {
            GVDisplayPoint glowPoint = new GVDisplayPoint();
            m_points.Add(glowPoint, true);
            return glowPoint;
        }

        public void RemoveGlowPoint(GVDisplayPoint glowPoint) {
            m_points.Remove(glowPoint);
        }

        public void Draw(Camera camera, int drawOrder) {
            m_drawBlockEnvironmentData.SubsystemTerrain = m_subsystemTerrain;
            m_drawBlockEnvironmentData.InWorldMatrix = Matrix.Identity;
            foreach (GVDisplayPoint key in m_points.Keys) {
                if (key.Value == 0
                    || key.Color.A == 0
                    || key.Size == 0) {
                    continue;
                }
                Vector3 position = key.Position;
                if (camera.ViewFrustum.Intersection(position)) {
                    Matrix matrix = Matrix.CreateFromYawPitchRoll(key.Rotation.X, key.Rotation.Y, key.Rotation.Z);
                    matrix.Translation = position;
                    if (key.Type == 0) {
                        int x = Terrain.ToCell(position.X);
                        int y = Terrain.ToCell(position.Y);
                        int z = Terrain.ToCell(position.Z);
                        int value = (int)key.Value;
                        int id = Terrain.ExtractContents(value);
                        if (id == 0) {
                            continue;
                        }
                        Block block = BlocksManager.Blocks[id];
                        TerrainChunk chunkAtCell = m_subsystemTerrain.Terrain.GetChunkAtCell(x, z);
                        if (chunkAtCell != null
                            && chunkAtCell.State >= TerrainChunkState.InvalidVertices1
                            && y >= 0
                            && y < 255) {
                            m_drawBlockEnvironmentData.Humidity = m_subsystemTerrain.Terrain.GetSeasonalHumidity(x, z);
                            m_drawBlockEnvironmentData.Temperature = m_subsystemTerrain.Terrain.GetSeasonalTemperature(x, z) + SubsystemWeather.GetTemperatureAdjustmentAtHeight(y);
                        }
                        float size;
                        if (key.Complex) {
                            m_drawBlockEnvironmentData.Light = key.Light;
                            size = key.Size;
                        }
                        else {
                            m_drawBlockEnvironmentData.Light = m_subsystemTerrain.Terrain.GetCellLightFast(x, y, z);
                            size = block.InHandScale;
                        }
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
                    else {
                        if (!GVStaticStorage.GVMBIDDataDictionary.TryGetValue(key.Value, out GVMemoryBankData data)
                            || data == null) {
                            continue;
                        }
                        float halfWidth;
                        float halfHeight;
                        int dataWidth = data.Data.Width;
                        int dataHeight = data.Data.Height;
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
                            lightValue = key.Light;
                            halfWidth *= key.Size;
                            halfHeight *= key.Size;
                            forward = Vector3.Zero;
                        }
                        else {
                            lightValue = m_subsystemTerrain.Terrain.GetCellLightFast((int)MathUtils.Floor(position.X), (int)MathUtils.Floor(position.Y), (int)MathUtils.Floor(position.Z));
                            forward = matrix.Forward * 0.435f;
                        }
                        Color color = Color.MultiplyColorOnly(key.Color, LightingManager.LightIntensityByLightValue[lightValue]);
                        Vector3 right = matrix.Right * halfWidth;
                        Vector3 up = matrix.Up * halfHeight;
                        if (key.Type == 2) {
                            for (int y = 0; y < dataHeight; y++) {
                                for (int x = 0; x < dataWidth; x++) {
                                    int id = Terrain.ExtractContents((int)data.Data.GetPixel(x, y).PackedValue);
                                    if (id == 0) {
                                        continue;
                                    }
                                    Block block = BlocksManager.Blocks[id];
                                    if (block is AirBlock) {
                                        continue;
                                    }
                                    int slotIndex = BlocksManager.Blocks[id].DefaultTextureSlot;
                                    float slotX = slotIndex % 16;
                                    float slotY = slotIndex / 16;
                                    float floatDataWidth = dataWidth;
                                    float floatDataHeight = dataHeight;
                                    Vector3 v = position + forward;
                                    Vector3 r0 = -(x * 2 / floatDataWidth - 1) * right;
                                    Vector3 r1 = -((x + 1) * 2 / floatDataWidth - 1) * right;
                                    Vector3 u0 = -(y * 2 / floatDataHeight - 1) * up;
                                    Vector3 u1 = -((y + 1) * 2 / floatDataHeight - 1) * up;
                                    Vector3 p1 = v + r1 + u0;
                                    if (camera.ViewFrustum.Intersection(p1)) {
                                        m_batches[key.CustomBit ? 1 : 0]
                                        .QueueQuad(
                                            p1,
                                            v + r0 + u0,
                                            v + r0 + u1,
                                            v + r1 + u1,
                                            new Vector2(slotX / 16, slotY / 16),
                                            new Vector2((slotX + 1) / 16, slotY / 16),
                                            new Vector2((slotX + 1) / 16, (slotY + 1) / 16),
                                            new Vector2(slotX / 16, (slotY + 1) / 16),
                                            color
                                        );
                                    }
                                }
                            }
                        }
                        else {
                            if (m_updateTimes.TryGetValue(key.Value, out DateTime updateTime)) {
                                if (updateTime != data.m_updateTime) {
                                    m_updateTimes[key.Value] = data.m_updateTime;
                                    m_textures[key.Value] = Texture2D.Load(data.Data);
                                }
                            }
                            else {
                                m_updateTimes.Add(key.Value, data.m_updateTime);
                                m_textures[key.Value] = Texture2D.Load(data.Data);
                            }
                            m_primitivesRenderer.TexturedBatch(
                                    m_textures[key.Value],
                                    false,
                                    0,
                                    DepthStencilState.DepthRead,
                                    RasterizerState.CullCounterClockwiseScissor,
                                    BlendState.AlphaBlend,
                                    key.CustomBit ? SamplerState.PointClamp : SamplerState.AnisotropicClamp
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
                BlendState.AlphaBlend,
                SamplerState.AnisotropicClamp
            );
            m_batches[1] = m_primitivesRenderer.TexturedBatch(
                BlocksTexture,
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