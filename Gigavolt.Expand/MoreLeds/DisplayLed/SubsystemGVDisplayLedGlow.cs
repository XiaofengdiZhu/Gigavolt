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
        public readonly Dictionary<GVDisplayPoint, bool> m_points = new Dictionary<GVDisplayPoint, bool>();
        public Texture2D BlocksTexture;
        public TexturedBatch3D[] m_batches = new TexturedBatch3D[2];

        public PrimitivesRenderer3D m_primitivesRenderer = new PrimitivesRenderer3D();

        public static int[] m_drawOrders = { 112 };

        public int[] DrawOrders => m_drawOrders;
        public Color birchLeavesColor = BlockColorsMap.BirchLeavesColorsMap.Lookup(8, 8);
        public Color grassColor = BlockColorsMap.GrassColorsMap.Lookup(8, 8);
        public Color ivyColor = BlockColorsMap.IvyColorsMap.Lookup(8, 8);
        public Color kelpColor = BlockColorsMap.KelpColorsMap.Lookup(8, 8);
        public Color mimosaLeavesColor = BlockColorsMap.MimosaLeavesColorsMap.Lookup(8, 8);
        public Color oakLeavesColor = BlockColorsMap.OakLeavesColorsMap.Lookup(8, 8);
        public Color seagrassColor = BlockColorsMap.SeagrassColorsMap.Lookup(8, 8);
        public Color spruceLeavesColor = BlockColorsMap.SpruceLeavesColorsMap.Lookup(8, 8);
        public Color tallSpruceLeavesColor = BlockColorsMap.TallSpruceLeavesColorsMap.Lookup(8, 8);
        public Color waterColor = BlockColorsMap.WaterColorsMap.Lookup(8, 8);

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
                    || (key.Complex && (key.Color.A == 0 || key.Size == 0))) {
                    continue;
                }
                Vector3 position = key.Position;
                Matrix matrix = Matrix.CreateFromYawPitchRoll(key.Rotation.X, key.Rotation.Y, key.Rotation.Z);
                matrix.Translation = position;
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
                        m_drawBlockEnvironmentData.Light = key.Complex ? key.Light : m_subsystemTerrain.Terrain.GetCellLightFast(x, y, z);
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
                        if (key.Type == 2) {
                            for (int y = 0; y < dataHeight; y++) {
                                for (int x = 0; x < dataWidth; x++) {
                                    int value = (int)imageData.GetPixel(x, y).PackedValue;
                                    int id = Terrain.ExtractContents(value);
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
                                    Vector3 r1 = -((x + 1) * 2 / floatDataWidth - 1) * right;
                                    Vector3 u0 = -(y * 2 / floatDataHeight - 1) * up;
                                    Vector3 p1 = v + r1 + u0;
                                    if (camera.ViewFrustum.Intersection(p1)) {
                                        Vector3 r0 = -(x * 2 / floatDataWidth - 1) * right;
                                        Vector3 u1 = -((y + 1) * 2 / floatDataHeight - 1) * up;
                                        Color maskColor = Color.Transparent;
                                        int blockData = Terrain.ExtractContents(value);
                                        switch (id) {
                                            case BirchLeavesBlock.Index:
                                                maskColor = birchLeavesColor;
                                                break;
                                            case GrassBlock.Index:
                                            case GrassTrapBlock.Index:
                                            case TallGrassBlock.Index:
                                                maskColor = grassColor;
                                                break;
                                            case CottonBlock.Index:
                                            case RyeBlock.Index:
                                                if (CottonBlock.GetIsWild(blockData)) {
                                                    maskColor = grassColor;
                                                }
                                                break;
                                            case IvyBlock.Index:
                                                maskColor = ivyColor;
                                                break;
                                            case KelpBlock.Index:
                                                maskColor = kelpColor;
                                                break;
                                            case MimosaLeavesBlock.Index:
                                                maskColor = mimosaLeavesColor;
                                                break;
                                            case OakLeavesBlock.Index:
                                                maskColor = oakLeavesColor;
                                                break;
                                            case SeagrassBlock.Index:
                                                maskColor = seagrassColor;
                                                break;
                                            case ChristmasTreeBlock.Index:
                                            case SpruceLeavesBlock.Index:
                                                maskColor = spruceLeavesColor;
                                                break;
                                            case TallSpruceLeavesBlock.Index:
                                                maskColor = tallSpruceLeavesColor;
                                                break;
                                            case WaterBlock.Index:
                                                maskColor = waterColor;
                                                break;
                                        }
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
                                            maskColor.PackedValue == 0 ? color : color * maskColor
                                        );
                                    }
                                }
                            }
                        }
                        else {
                            m_primitivesRenderer.TexturedBatch(
                                    data.GetTexture2D(),
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