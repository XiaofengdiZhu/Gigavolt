using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;
using GameEntitySystem;

namespace Game {
    public class GVSubterrainRenderer : IDisposable {
        public readonly GVSubterrainSystem m_subterrainSystem;
        public readonly SubsystemSky m_subsystemSky;
        public SubsystemAnimatedTextures m_subsystemAnimatedTextures;

        public readonly DynamicArray<TerrainChunk> m_chunksToDraw = [];
        public static readonly DynamicArray<int> m_tmpIndices = [];
        public static DynamicArray<TerrainVertex> m_tmpVertices = [];

        public static Shader OpaqueShader;
        public static Shader AlphatestedShader;
        public static Shader TransparentShader;

        public static readonly SamplerState m_samplerState = new() { AddressModeU = TextureAddressMode.Clamp, AddressModeV = TextureAddressMode.Clamp, FilterMode = TextureFilterMode.Point, MaxLod = 0f };

        public static readonly SamplerState m_samplerStateMips = new() { AddressModeU = TextureAddressMode.Clamp, AddressModeV = TextureAddressMode.Clamp, FilterMode = TextureFilterMode.PointMipLinear, MaxLod = 4f };

        public GVSubterrainRenderer(GVSubterrainSystem system, Project project) {
            m_subterrainSystem = system;
            m_subsystemSky = project.FindSubsystem<SubsystemSky>(true);
            m_subsystemAnimatedTextures = project.FindSubsystem<SubsystemAnimatedTextures>(true);
            OpaqueShader ??= new Shader(ShaderCodeManager.GetFast("Shaders/GVSubterrainOpaqueAndAlphaTested.vsh"), ShaderCodeManager.GetFast("Shaders/GVSubterrainOpaqueAndAlphaTested.psh"));
            AlphatestedShader ??= new Shader(ShaderCodeManager.GetFast("Shaders/GVSubterrainOpaqueAndAlphaTested.vsh"), ShaderCodeManager.GetFast("Shaders/GVSubterrainOpaqueAndAlphaTested.psh"), new ShaderMacro("ALPHATESTED"));
            TransparentShader ??= new Shader(ShaderCodeManager.GetFast("Shaders/GVSubterrainTransparent.vsh"), ShaderCodeManager.GetFast("Shaders/GVSubterrainTransparent.psh"));
            Display.DeviceReset += Display_DeviceReset;
        }

        public void PrepareForDrawing(Camera camera) {
            BoundingFrustum viewFrustum = camera.ViewFrustum;
            m_chunksToDraw.Clear();
            TerrainChunk[] allocatedChunks = m_subterrainSystem.Terrain.AllocatedChunks;
            foreach (TerrainChunk terrainChunk in allocatedChunks) {
                if (terrainChunk.NewGeometryData) {
                    lock (terrainChunk.Geometry) {
                        if (terrainChunk.NewGeometryData) {
                            terrainChunk.NewGeometryData = false;
                            SetupTerrainChunkGeometryVertexIndexBuffers(terrainChunk);
                        }
                    }
                }
                BoundingBox boundingBox = BoundingBox.Transform(terrainChunk.BoundingBox, m_subterrainSystem.GlobalTransform);
                if (viewFrustum.Intersection(boundingBox)) {
                    m_chunksToDraw.Add(terrainChunk);
                }
                if (terrainChunk.State != TerrainChunkState.Valid) { }
            }
        }

        public void SetupTerrainChunkGeometryVertexIndexBuffers(TerrainChunk chunk) {
            DisposeTerrainChunkGeometryVertexIndexBuffers(chunk);
            CompileDrawSubsets(chunk.ChunkSliceGeometries, chunk.Buffers);
            chunk.CopySliceContentsHashes();
        }

        public class SubsetStat {
            public int[] subsetTotalIndexCount = new int[7];
            public int[] subsetTotalVertexCount = new int[7];
            public int[] subsetSettedIndexCount = new int[7];
            public int[] subsetSettedVertexCount = new int[7];
            public int totalIndexCount;
            public int totalVertextCount;
            public TerrainChunkGeometry.Buffer Buffer;
        }

        public static Dictionary<Texture2D, SubsetStat> stat = new();

        public static void CompileDrawSubsets(TerrainGeometry[] chunkSliceGeometries, DynamicArray<TerrainChunkGeometry.Buffer> buffers, Func<TerrainVertex, TerrainVertex> vertexTransform = null) {
            stat.Clear();
            //按贴图进行分组统计Subset的顶点数与索引数
            for (int k = 0; k < chunkSliceGeometries.Length; k++) {
                TerrainGeometry geometry = chunkSliceGeometries[k]; //第k个slice
                //统计每个subset的indexCount与VertexCount
                foreach (KeyValuePair<Texture2D, TerrainGeometry> drawItem in geometry.Draws) {
                    TerrainGeometry subGeometry = drawItem.Value;
                    for (int i = 0; i < subGeometry.Subsets.Length; i++) {
                        if (!stat.TryGetValue(drawItem.Key, out SubsetStat subsetStat)) {
                            subsetStat = new SubsetStat();
                            stat.Add(drawItem.Key, subsetStat);
                        }
                        int ic = subGeometry.Subsets[i].Indices.Count;
                        int vc = subGeometry.Subsets[i].Vertices.Count;
                        subsetStat.subsetTotalIndexCount[i] += ic;
                        subsetStat.subsetTotalVertexCount[i] += vc;
                        subsetStat.totalIndexCount += ic;
                        subsetStat.totalVertextCount += vc;
                    }
                }
            }
            //按贴图分组完成，生成buffer
            foreach (KeyValuePair<Texture2D, SubsetStat> statItem in stat) {
                if (statItem.Value.totalIndexCount == 0) {
                    continue;
                }
                TerrainChunkGeometry.Buffer buffer = new();
                buffer.IndexBuffer = new IndexBuffer(IndexFormat.ThirtyTwoBits, statItem.Value.totalIndexCount);
                buffer.VertexBuffer = new VertexBuffer(TerrainVertex.VertexDeclaration, statItem.Value.totalVertextCount);
                buffer.Texture = statItem.Key;
                statItem.Value.Buffer = buffer;
                buffers.Add(buffer);
                int subsetSettedIndexCount = 0;
                int subsetSettedVertexCount = 0;
                for (int i = 0; i < 7; i++) {
                    if (i == 0) {
                        buffer.SubsetIndexBufferStarts[i] = 0;
                        buffer.SubsetIndexBufferEnds[i] = statItem.Value.subsetTotalIndexCount[i];
                        buffer.SubsetVertexBufferStarts[i] = 0;
                        buffer.SubsetVertexBufferEnds[i] = statItem.Value.subsetTotalVertexCount[i];
                        subsetSettedIndexCount = statItem.Value.subsetTotalIndexCount[i];
                        subsetSettedVertexCount = statItem.Value.subsetTotalVertexCount[i];
                    }
                    else {
                        buffer.SubsetIndexBufferStarts[i] = subsetSettedIndexCount;
                        buffer.SubsetIndexBufferEnds[i] = statItem.Value.subsetTotalIndexCount[i] + buffer.SubsetIndexBufferStarts[i];
                        buffer.SubsetVertexBufferStarts[i] = subsetSettedVertexCount;
                        buffer.SubsetVertexBufferEnds[i] = statItem.Value.subsetTotalVertexCount[i] + buffer.SubsetVertexBufferStarts[i];
                        subsetSettedIndexCount += statItem.Value.subsetTotalIndexCount[i];
                        subsetSettedVertexCount += statItem.Value.subsetTotalVertexCount[i];
                    }
                }
            }
            //将顶点列表与索引列表写入buffer
            for (int k = 0; k < chunkSliceGeometries.Length; k++) {
                TerrainGeometry geometry = chunkSliceGeometries[k]; //第k个slice
                //统计每个subset的indexCount与VertexCount
                foreach (KeyValuePair<Texture2D, TerrainGeometry> drawItem in geometry.Draws) {
                    TerrainGeometry subGeometry = drawItem.Value;
                    for (int i = 0; i < subGeometry.Subsets.Length; i++) {
                        if (stat.TryGetValue(drawItem.Key, out SubsetStat subsetStat)) {
                            if (subsetStat.totalIndexCount == 0) {
                                continue;
                            }
                            TerrainGeometryDynamicArray<int> indices = subGeometry.Subsets[i].Indices;
                            TerrainGeometryDynamicArray<TerrainVertex> vertices = subGeometry.Subsets[i].Vertices;
                            if (indices.Count > 0) {
                                TerrainChunkGeometry.Buffer buffer = subsetStat.Buffer;
                                m_tmpIndices.Count = indices.Count;
                                ShiftIndices(indices.Array, m_tmpIndices.Array, buffer.SubsetVertexBufferStarts[i] + subsetStat.subsetSettedVertexCount[i], indices.Count);
                                buffer.IndexBuffer.SetData(m_tmpIndices.Array, 0, indices.Count, buffer.SubsetIndexBufferStarts[i] + subsetStat.subsetSettedIndexCount[i]);
                                if (vertexTransform != null) {
                                    m_tmpVertices.Count = vertices.Count;
                                    for (int j = 0; j < vertices.Count; j++) {
                                        m_tmpVertices[j] = vertexTransform(vertices[j]);
                                    }
                                    buffer.VertexBuffer.SetData(m_tmpVertices.Array, 0, vertices.Count, buffer.SubsetVertexBufferStarts[i] + subsetStat.subsetSettedVertexCount[i]);
                                }
                                else {
                                    buffer.VertexBuffer.SetData(vertices.Array, 0, vertices.Count, buffer.SubsetVertexBufferStarts[i] + subsetStat.subsetSettedVertexCount[i]);
                                }
                                subsetStat.subsetSettedIndexCount[i] += indices.Count;
                                subsetStat.subsetSettedVertexCount[i] += vertices.Count;
                            }
                        }
                    }
                }
            }
        }

        public static void ShiftIndices(int[] source, int[] destination, int shift, int count) {
            for (int i = 0; i < count; i++) {
                destination[i] = source[i] + shift;
            }
        }

        public void DrawOpaqueAndAlphaTested(Camera camera) {
            Vector3 viewPosition = camera.ViewPosition;
            Vector3 v = new(MathF.Floor(viewPosition.X), 0f, MathF.Floor(viewPosition.Z));
            Display.BlendState = BlendState.Opaque;
            Display.DepthStencilState = DepthStencilState.Default;
            Display.RasterizerState = RasterizerState.CullCounterClockwiseScissor;
            OpaqueShader.GetParameter("u_origin", true).SetValue(v.XZ);
            AlphatestedShader.GetParameter("u_origin", true).SetValue(v.XZ);
            Matrix viewProjectionMatrix = Matrix.CreateTranslation(v - viewPosition) * camera.ViewMatrix.OrientationMatrix * camera.ProjectionMatrix;
            OpaqueShader.GetParameter("u_viewProjectionMatrix", true).SetValue(viewProjectionMatrix);
            AlphatestedShader.GetParameter("u_viewProjectionMatrix", true).SetValue(viewProjectionMatrix);
            OpaqueShader.GetParameter("u_subterrainTransform", true).SetValue(m_subterrainSystem.GlobalTransform);
            AlphatestedShader.GetParameter("u_subterrainTransform", true).SetValue(m_subterrainSystem.GlobalTransform);
            OpaqueShader.GetParameter("u_viewPosition", true).SetValue(viewPosition);
            AlphatestedShader.GetParameter("u_viewPosition", true).SetValue(viewPosition);
            SamplerState samplerState = SettingsManager.TerrainMipmapsEnabled ? m_samplerStateMips : m_samplerState;
            OpaqueShader.GetParameter("u_samplerState", true).SetValue(samplerState);
            AlphatestedShader.GetParameter("u_samplerState", true).SetValue(samplerState);
            foreach (TerrainChunk terrainChunk in m_chunksToDraw) {
                DrawTerrainChunkGeometrySubsets(OpaqueShader, terrainChunk, 31);
                DrawTerrainChunkGeometrySubsets(AlphatestedShader, terrainChunk, 32);
            }
        }

        public void DrawTransparent(Camera camera) {
            Vector3 viewPosition = camera.ViewPosition;
            Vector3 v = new(MathF.Floor(viewPosition.X), 0f, MathF.Floor(viewPosition.Z));
            Display.BlendState = BlendState.AlphaBlend;
            Display.DepthStencilState = DepthStencilState.Default;
            Display.RasterizerState = m_subsystemSky.ViewUnderWaterDepth > 0f ? RasterizerState.CullClockwiseScissor : RasterizerState.CullCounterClockwiseScissor;
            TransparentShader.GetParameter("u_origin", true).SetValue(v.XZ);
            TransparentShader.GetParameter("u_viewProjectionMatrix", true).SetValue(Matrix.CreateTranslation(v - viewPosition) * camera.ViewMatrix.OrientationMatrix * camera.ProjectionMatrix);
            TransparentShader.GetParameter("u_subterrainTransform", true).SetValue(m_subterrainSystem.GlobalTransform);
            TransparentShader.GetParameter("u_viewPosition", true).SetValue(viewPosition);
            TransparentShader.GetParameter("u_samplerState", true).SetValue(SettingsManager.TerrainMipmapsEnabled ? m_samplerStateMips : m_samplerState);
            foreach (TerrainChunk terrainChunk in m_chunksToDraw) {
                DrawTerrainChunkGeometrySubsets(TransparentShader, terrainChunk, 64);
            }
        }

        public void DrawTerrainChunkGeometrySubsets(Shader shader, TerrainChunk chunk, int subsetsMask, bool ApplyTexture = true) {
            foreach (TerrainChunkGeometry.Buffer buffer in chunk.Buffers) {
                int num = int.MaxValue;
                int num2 = 0;
                for (int i = 0; i < 8; i++) {
                    if (i < 7
                        && (subsetsMask & (1 << i)) != 0) {
                        if (buffer.SubsetIndexBufferEnds[i] > 0) {
                            if (num == int.MaxValue) {
                                num = buffer.SubsetIndexBufferStarts[i];
                            }
                            num2 = buffer.SubsetIndexBufferEnds[i];
                        }
                    }
                    else {
                        if (num2 > num) {
                            if (ApplyTexture) {
                                shader.GetParameter("u_texture", true).SetValue(buffer.Texture);
                            }
                            int num3 = num2 - num;
                            Display.DrawIndexed(
                                PrimitiveType.TriangleList,
                                shader,
                                buffer.VertexBuffer,
                                buffer.IndexBuffer,
                                num,
                                num3
                            );
                        }
                        num = int.MaxValue;
                    }
                }
            }
        }

        public void Dispose() {
            Display.DeviceReset -= Display_DeviceReset;
        }

        public void Display_DeviceReset() {
            m_subterrainSystem.TerrainUpdater.DowngradeAllChunksState(TerrainChunkState.InvalidVertices1, false);
            foreach (TerrainChunk terrainChunk in m_subterrainSystem.Terrain.AllocatedChunks) {
                DisposeTerrainChunkGeometryVertexIndexBuffers(terrainChunk);
            }
        }

        public void DisposeTerrainChunkGeometryVertexIndexBuffers(TerrainChunk chunk) {
            foreach (TerrainChunkGeometry.Buffer buffer in chunk.Buffers) {
                buffer.Dispose();
            }
            chunk.Buffers.Clear();
            chunk.InvalidateSliceContentsHashes();
        }
    }
}