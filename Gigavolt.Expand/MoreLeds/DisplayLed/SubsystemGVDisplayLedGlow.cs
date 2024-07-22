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
        public readonly DrawBlockEnvironmentData m_drawBlockEnvironmentData = new();
        public readonly Dictionary<uint, HashSet<HashSet<GVDisplayPoint>>> m_points = new();
        public Texture2D BlocksTexture;

        public readonly PrimitivesRenderer3D m_primitivesRenderer = new();

        public int[] DrawOrders => [112];

        public HashSet<GVDisplayPoint> AddGlowPoints(uint subterrainId) {
            HashSet<GVDisplayPoint> glowPoint = [];
            if (m_points.TryGetValue(subterrainId, out HashSet<HashSet<GVDisplayPoint>> points)) {
                points.Add(glowPoint);
            }
            else {
                m_points.Add(subterrainId, [glowPoint]);
            }
            return glowPoint;
        }

        public void RemoveGlowPoints(HashSet<GVDisplayPoint> glowPoint, uint subterrainId) {
            m_points[subterrainId]?.Remove(glowPoint);
        }

        public void Draw(Camera camera, int drawOrder) {
            m_drawBlockEnvironmentData.SubsystemTerrain = m_subsystemTerrain;
            m_drawBlockEnvironmentData.InWorldMatrix = Matrix.Identity;
            foreach ((uint subterrainId, HashSet<HashSet<GVDisplayPoint>> points1) in m_points) {
                if (points1.Count == 0) {
                    continue;
                }
                Matrix transform = subterrainId == 0 ? default : GVStaticStorage.GVSubterrainSystemDictionary[subterrainId].GlobalTransform;
                foreach (HashSet<GVDisplayPoint> points in points1) {
                    foreach (GVDisplayPoint key in points) {
                        if (!key.isValid()) {
                            continue;
                        }
                        Vector3 position = key.Position;
                        Matrix rotationMatrix = Matrix.CreateFromYawPitchRoll(key.Rotation.X, key.Rotation.Y, key.Rotation.Z);
                        rotationMatrix.Translation = position;
                        if (subterrainId != 0) {
                            position = Vector3.Transform(position, transform);
                        }
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
                                    && y is >= 0 and < 255) {
                                    m_drawBlockEnvironmentData.Humidity = m_subsystemTerrain.Terrain.GetSeasonalHumidity(x, z);
                                    m_drawBlockEnvironmentData.Temperature = m_subsystemTerrain.Terrain.GetSeasonalTemperature(x, z) + SubsystemWeather.GetTemperatureAdjustmentAtHeight(y);
                                }
                                m_drawBlockEnvironmentData.Light = key.Complex ? 15 : m_subsystemTerrain.Terrain.GetCellLightFast(x, y, z);
                                m_drawBlockEnvironmentData.BillboardDirection = camera.ViewDirection;
                                m_drawBlockEnvironmentData.InWorldMatrix.Translation = position;
                                if (subterrainId != 0) {
                                    rotationMatrix *= transform;
                                }
                                block.DrawBlock(
                                    m_primitivesRenderer,
                                    value,
                                    key.Color,
                                    size,
                                    ref rotationMatrix,
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
                                lightValue = m_subsystemTerrain.Terrain.GetCellLightFast(Terrain.ToCell(position.X), Terrain.ToCell(position.Y), Terrain.ToCell(position.Z));
                                forward = rotationMatrix.Forward * 0.43f;
                            }
                            Vector3 right = rotationMatrix.Right * halfWidth;
                            Vector3 up = rotationMatrix.Up * halfHeight;
                            if (subterrainId != 0) {
                                Matrix orientation = transform.OrientationMatrix;
                                forward = Vector3.Transform(forward, orientation);
                                right = Vector3.Transform(right, orientation);
                                up = Vector3.Transform(up, orientation);
                            }
                            Vector3[] offsets = [right - up, right + up, -right - up, -right + up];
                            Vector3 min = new(float.MaxValue);
                            Vector3 max = new(float.MinValue);
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
                                Vector3 direction = position - camera.ViewPosition;
                                position -= (0.01f + 0.02f * Vector3.Dot(direction, camera.ViewDirection)) / direction.Length() * direction;
                                if (key.Type == 2) {
                                    // 绘制地层
                                    TexturedBatch3D batch = new() {
                                        Texture = data.GetTerrainTexture2D(samplerState),
                                        UseAlphaTest = false,
                                        Layer = 0,
                                        DepthStencilState = DepthStencilState.DepthRead,
                                        RasterizerState = RasterizerState.CullCounterClockwiseScissor,
                                        BlendState = BlendState.NonPremultiplied,
                                        SamplerState = samplerState
                                    };
                                    batch.QueueQuad(
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
                                    batch.Flush(camera.ViewProjectionMatrix);
                                }
                                else {
                                    //绘制图片
                                    TexturedBatch3D batch = new() {
                                        Texture = data.GetTexture2D(),
                                        UseAlphaTest = false,
                                        Layer = 0,
                                        DepthStencilState = DepthStencilState.DepthRead,
                                        RasterizerState = RasterizerState.CullCounterClockwiseScissor,
                                        BlendState = BlendState.NonPremultiplied,
                                        SamplerState = samplerState
                                    };
                                    batch.QueueQuad(
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
                                    batch.Flush(camera.ViewProjectionMatrix);
                                }
                            }
                        }
                    }
                }
                m_primitivesRenderer.Flush(camera.ViewProjectionMatrix);
            }
        }

        public override void Load(ValuesDictionary valuesDictionary) {
            m_subsystemSky = Project.FindSubsystem<SubsystemSky>(true);
            m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(true);
            BlocksTexture = Project.FindSubsystem<SubsystemBlocksTexture>(true).BlocksTexture;
        }
    }
}