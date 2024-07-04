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
            AlphatestedShader ??= new Shader(ShaderCodeManager.GetFast("Shaders/GVSubterrainOpaqueAndAlphaTested.vsh"), ShaderCodeManager.GetFast("Shaders/GVSubterrainOpaqueAndAlphaTested.psh"), [new ShaderMacro("ALPHATESTED")]);
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
            CompileDrawSubsets(chunk.Draws, chunk.Buffers);
            chunk.CopySliceContentsHashes();
        }

        public static void CompileDrawSubsets(Dictionary<Texture2D, TerrainGeometry[]> list, DynamicArray<TerrainChunkGeometry.Buffer> buffers, Func<TerrainVertex, TerrainVertex> vertexTransform = null) {
            foreach ((Texture2D key, TerrainGeometry[] geometry) in list) {
                int num = 0;
                while (num < 112) {
                    int num2 = 0;
                    int num3 = 0;
                    int i;
                    for (i = num; i < 112; i++) {
                        int num4 = i / 16;
                        int num5 = i % 16;
                        TerrainGeometrySubset terrainGeometrySubset = geometry[num5].Subsets[num4];
                        if (vertexTransform != null) {
                            DynamicArray<TerrainVertex> tmpList = [];
                            foreach (TerrainVertex t in terrainGeometrySubset.Vertices) {
                                TerrainVertex vertex = vertexTransform(t);
                                tmpList.Add(vertex);
                            }
                            terrainGeometrySubset.Vertices = tmpList;
                        }
                        try {
                            _ = checked(num2 + terrainGeometrySubset.Vertices.Count);
                        }
                        catch (Exception) {
                            if (i > num) {
                                break;
                            }
                        }
                        num2 += terrainGeometrySubset.Vertices.Count;
                        num3 += terrainGeometrySubset.Indices.Count;
                    }
                    if (num2 > 0
                        && num3 > 0) {
                        TerrainChunkGeometry.Buffer buffer = new();
                        buffer.Texture = key;
                        buffers.Add(buffer);
                        buffer.VertexBuffer = new VertexBuffer(TerrainVertex.VertexDeclaration, num2);
                        buffer.IndexBuffer = new IndexBuffer(IndexFormat.ThirtyTwoBits, num3);
                        int num6 = 0;
                        int num7 = 0;
                        for (int j = num; j < i; j++) {
                            int num8 = j / 16;
                            int num9 = j % 16;
                            TerrainGeometrySubset terrainGeometrySubset2 = geometry[num9].Subsets[num8];
                            if (num9 == 0
                                || j == num) {
                                buffer.SubsetIndexBufferStarts[num8] = num7;
                            }
                            if (terrainGeometrySubset2.Indices.Count > 0) {
                                m_tmpIndices.Count = terrainGeometrySubset2.Indices.Count;
                                ShiftIndices(terrainGeometrySubset2.Indices.Array, m_tmpIndices.Array, num6, terrainGeometrySubset2.Indices.Count);
                                buffer.IndexBuffer.SetData(m_tmpIndices.Array, 0, m_tmpIndices.Count, num7);
                                num7 += m_tmpIndices.Count;
                            }
                            if (terrainGeometrySubset2.Vertices.Count > 0) {
                                buffer.VertexBuffer.SetData(terrainGeometrySubset2.Vertices.Array, 0, terrainGeometrySubset2.Vertices.Count, num6);
                                num6 += terrainGeometrySubset2.Vertices.Count;
                            }
                            if (num9 == 15
                                || j == i - 1) {
                                buffer.SubsetIndexBufferEnds[num8] = num7;
                            }
                        }
                    }
                    num = i;
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