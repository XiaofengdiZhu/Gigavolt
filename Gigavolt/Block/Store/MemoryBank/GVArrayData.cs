using System;
using System.IO;
using Engine;
using Engine.Graphics;
using Engine.Media;

namespace Game {
    public abstract class GVArrayData : IEditableItemData {
        public uint m_ID;
        public string m_worldDirectory;
        public DateTime m_updateTime;
        public bool m_isDataInitialized;
        public string m_cachedString;
        public DateTime m_cachedStringTime;
        public byte[] m_cachedBytes;
        public DateTime m_cachedBytesTime;
        public short[] m_cachedShorts;
        public DateTime m_cachedShortsTime;
        public Image m_cachedImage;
        public DateTime m_cachedImageTime;
        public Texture2D m_cachedTexture2D;
        public DateTime m_cachedTexture2DTime;
        public RenderTarget2D m_cachedTerrainTexture2D;
        public DateTime m_cachedTerrainTexture2DTime;
        public PrimitivesRenderer2D m_primitivesRenderer2D = new();
        public SamplerState m_samplerState;
        public virtual string ExportExtension => ".png";
        static readonly Color birchLeavesColor = BlockColorsMap.BirchLeavesColorsMap.Lookup(8, 8);
        static readonly Color grassColor = BlockColorsMap.GrassColorsMap.Lookup(8, 8);
        static readonly Color ivyColor = BlockColorsMap.IvyColorsMap.Lookup(8, 8);
        static readonly Color kelpColor = BlockColorsMap.KelpColorsMap.Lookup(8, 8);
        static readonly Color mimosaLeavesColor = BlockColorsMap.MimosaLeavesColorsMap.Lookup(8, 8);
        static readonly Color oakLeavesColor = BlockColorsMap.OakLeavesColorsMap.Lookup(8, 8);
        static readonly Color seagrassColor = BlockColorsMap.SeagrassColorsMap.Lookup(8, 8);
        static readonly Color spruceLeavesColor = BlockColorsMap.SpruceLeavesColorsMap.Lookup(8, 8);
        static readonly Color tallSpruceLeavesColor = BlockColorsMap.TallSpruceLeavesColorsMap.Lookup(8, 8);
        static readonly Color waterColor = BlockColorsMap.WaterColorsMap.Lookup(8, 8);

        public abstract uint Read(uint index);
        public abstract void Write(uint index, uint data);
        public abstract IEditableItemData Copy();

        public abstract void LoadData();
        public abstract void LoadString(string data);

        public abstract string SaveString();
        public virtual void String2Data(string str, int width = 0, int height = 0) { }
        public virtual string Data2String() => null;

        public string GetString() {
            if (m_isDataInitialized) {
                if (m_cachedString == null
                    || m_updateTime != m_cachedStringTime) {
                    m_cachedString = Data2String();
                    m_cachedStringTime = m_updateTime;
                }
                return m_cachedString;
            }
            return null;
        }

        public virtual byte[] Data2Bytes(int startIndex = 0, int length = int.MaxValue) => null;

        public byte[] GetBytes(int startIndex = 0, int length = int.MaxValue) {
            if (m_isDataInitialized) {
                if (m_cachedBytes == null
                    || m_updateTime != m_cachedBytesTime) {
                    m_cachedBytes = Data2Bytes(startIndex, length);
                    m_cachedBytesTime = m_updateTime;
                }
                return m_cachedBytes;
            }
            return null;
        }

        public virtual short[] Data2Shorts() => null;

        public short[] GetShorts() {
            if (m_isDataInitialized) {
                if (m_cachedShorts == null
                    || m_updateTime != m_cachedShortsTime) {
                    m_cachedShorts = Data2Shorts();
                    m_cachedShortsTime = m_updateTime;
                }
                return m_cachedShorts;
            }
            return null;
        }

        public virtual void Shorts2Data(short[] shorts) { }

        public virtual Image Data2Image() => null;

        public Image GetImage() {
            if (m_isDataInitialized) {
                if (m_cachedImage == null
                    || m_updateTime != m_cachedImageTime) {
                    m_cachedImage = null;
                    m_cachedImage = Data2Image();
                    m_cachedImageTime = m_updateTime;
                }
                return m_cachedImage;
            }
            return null;
        }

        public virtual void Image2Data(Image image) { }
        public virtual MemoryStream Data2Stream() => null;
        public MemoryStream GetStream() => m_isDataInitialized ? Data2Stream() : null;

        public virtual string Stream2Data(Stream stream, string extension = "") => string.Empty;

        public Texture2D Data2Texture2D() {
            Image image = GetImage();
            return image == null ? null : Texture2D.Load(image);
        }

        public Texture2D GetTexture2D() {
            if (m_isDataInitialized) {
                if (m_cachedTexture2D == null
                    || m_updateTime != m_cachedTexture2DTime) {
                    if (m_cachedTexture2D != null) {
                        m_cachedTexture2D.Tag = null;
                        m_cachedTexture2D.Dispose();
                    }
                    m_cachedTexture2D = Data2Texture2D();
                    m_cachedTexture2DTime = m_updateTime;
                }
                return m_cachedTexture2D;
            }
            return null;
        }

        public RenderTarget2D Data2TerrainTexture2D(SamplerState samplerState) {
            Image image = GetImage();
            int dataWidth = image.Width;
            int dataHeight = image.Height;
            int maxSide = Math.Max(dataWidth, dataHeight);
            if (maxSide > 8192) {
                return null;
            }
            int multiplier = 4;
            if (maxSide > 4096) {
                multiplier = 1;
            }
            else if (maxSide > 2048) {
                multiplier = 2;
            }
            else if (maxSide > 1024) {
                multiplier = 3;
            }
            int slotSide = 1 << multiplier;
            RenderTarget2D originRenderTarget = Display.RenderTarget;
            RenderTarget2D renderTarget = new(
                dataWidth << multiplier,
                dataHeight << multiplier,
                1,
                ColorFormat.Rgba8888,
                DepthFormat.None
            );
            Display.RenderTarget = renderTarget;
            TexturedBatch2D texturedBatch = m_primitivesRenderer2D.TexturedBatch(
                GameManager.Project.FindSubsystem<SubsystemBlocksTexture>(true).BlocksTexture,
                false,
                0,
                DepthStencilState.None,
                null,
                BlendState.AlphaBlend,
                samplerState
            );
            for (int y = 0; y < dataHeight; y++) {
                for (int x = 0; x < dataWidth; x++) {
                    int value = (int)image.GetPixelFast(x, y).PackedValue;
                    int id = Terrain.ExtractContents(value);
                    if (id == 0) {
                        continue;
                    }
                    Block block = BlocksManager.Blocks[id];
                    if (block is AirBlock) {
                        continue;
                    }
                    int slotIndex = block.DefaultTextureSlot;
                    Color maskColor = Color.Transparent;
                    int blockData = Terrain.ExtractData(value);
                    switch (block) {
                        case WoodBlock woodBlock:
                            slotIndex = woodBlock.m_sideTextureSlot;
                            break;
                        case PaintedCubeBlock paintedCubeBlock: {
                            int? intColor = PaintedCubeBlock.GetColor(blockData);
                            if (intColor.HasValue) {
                                SubsystemPalette subsystemPalette = GameManager.Project.FindSubsystem<SubsystemPalette>(false);
                                if (subsystemPalette == null) {
                                    maskColor = WorldPalette.DefaultColors[intColor.Value];
                                }
                                else {
                                    maskColor = subsystemPalette.GetColor(intColor.Value);
                                }
                            }
                            break;
                        }
                    }
                    float slotX = slotIndex % 16;
                    float slotY = slotIndex / 16;
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
                    texturedBatch.QueueQuad(
                        new Vector2(x * slotSide, y * slotSide),
                        new Vector2((x + 1) * slotSide, (y + 1) * slotSide),
                        0f,
                        new Vector2(slotX / 16, slotY / 16),
                        new Vector2((slotX + 1) / 16, (slotY + 1) / 16),
                        maskColor.PackedValue == 0 ? Color.White : maskColor
                    );
                }
            }
            texturedBatch.QueueQuad(
                new Vector2(0, 0),
                new Vector2(1, 1),
                0f,
                new Vector2(0, 0),
                new Vector2(1, 1),
                Color.White
            );
            m_primitivesRenderer2D.Flush();
            Display.RenderTarget = originRenderTarget;
            return renderTarget;
        }

        public RenderTarget2D GetTerrainTexture2D(SamplerState samplerState) {
            if (m_isDataInitialized) {
                if (m_cachedTerrainTexture2D == null
                    || m_updateTime != m_cachedTerrainTexture2DTime
                    || samplerState != m_samplerState) {
                    m_cachedTerrainTexture2D?.Dispose();
                    m_cachedTerrainTexture2D = Data2TerrainTexture2D(samplerState);
                    m_cachedTerrainTexture2DTime = m_updateTime;
                    m_samplerState = samplerState;
                }
                return m_cachedTerrainTexture2D;
            }
            return null;
        }

        public virtual void UintArray2Data(uint[] uints, int width = 0, int height = 0) { }
        public virtual uint[] Data2UintArray() => null;
        public uint[] GetUintArray() => m_isDataInitialized ? Data2UintArray() : null;
    }
}