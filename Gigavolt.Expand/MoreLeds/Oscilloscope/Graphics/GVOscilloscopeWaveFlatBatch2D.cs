extern alias OpenTKForWindows;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using OpenTKForWindows::OpenTK.Graphics.ES30;

namespace Engine.Graphics {
    public class GVOscilloscopeWaveFlatBatch2D : FlatBatch2D {
        public readonly DynamicArray<VertexPositionColor> PointsVertices = [];
        public readonly DynamicArray<int> PointsIndices = [];
        public readonly DynamicArray<VertexPositionColor> LineStripeVertices = [];
        public readonly DynamicArray<int> LineStripeIndices = [];
        public static GVOscilloscopeBackgroundShader GVOscilloscopeBackgroundShader = new();
        public static GVOscilloscopeWaveShader GVOscilloscopeWaveShader = new();
        public override bool IsEmpty() => LineIndices.Count == 0 && TriangleIndices.Count == 0 && PointsIndices.Count == 0 && LineStripeIndices.Count == 0;

        public override void Clear() {
            LineVertices.Clear();
            LineIndices.Clear();
            TriangleVertices.Clear();
            TriangleIndices.Clear();
            PointsVertices.Clear();
            PointsIndices.Clear();
            LineStripeVertices.Clear();
            LineStripeIndices.Clear();
        }

        public new void QueueLineStrip(IEnumerable<Vector2> points, float depth, Color color) {
            int count = LineStripeVertices.Count;
            int num = 0;
            foreach (Vector2 point in points) {
                LineStripeVertices.Add(new VertexPositionColor(new Vector3(point, depth), color));
                LineStripeIndices.Add(count + num++);
            }
        }

        public void QueuePoint(Vector2 p1, float depth, Color color) {
            int count = PointsVertices.Count;
            PointsVertices.Add(new VertexPositionColor(new Vector3(p1, depth), color));
            PointsIndices.Add(count);
        }

        public void QueuePoints(IEnumerable<Vector2> points, float depth, Color color) {
            int count = PointsVertices.Count;
            int num = 0;
            foreach (Vector2 point in points) {
                PointsVertices.Add(new VertexPositionColor(new Vector3(point, depth), color));
                PointsIndices.Add(count + num++);
            }
        }

        public void FlushBackground() {
            if (TriangleIndices.Count != 0) {
                Display.DepthStencilState = DepthStencilState;
                Display.RasterizerState = RasterizerState;
                Display.BlendState = BlendState;
                GVOscilloscopeBackgroundShader.Transforms.World[0] = PrimitivesRenderer2D.ViewportMatrix();
                int num = 0;
                int num2 = TriangleIndices.Count;
                while (num2 > 0) {
                    int num3 = Math.Min(num2, 196605);
                    DrawUserIndexed(
                        GVPrimitiveType.TriangleList,
                        GVOscilloscopeBackgroundShader,
                        VertexPositionColor.VertexDeclaration,
                        TriangleVertices.Array,
                        0,
                        TriangleVertices.Count,
                        TriangleIndices.Array,
                        num,
                        num3
                    );
                    num += num3;
                    num2 -= num3;
                }
                TriangleIndices.Clear();
                TriangleVertices.Clear();
            }
        }

        public void FlushWave() {
            if (LineStripeIndices.Count != 0
                && PointsIndices.Count != 0) {
                Display.DepthStencilState = DepthStencilState;
                Display.RasterizerState = RasterizerState;
                Display.BlendState = BlendState;
                GVOscilloscopeWaveShader.Transforms.World[0] = PrimitivesRenderer2D.ViewportMatrix();
                int num7 = 0;
                int num8 = PointsIndices.Count;
                while (num8 > 0) {
                    int num9 = Math.Min(num8, 131070);
                    DrawUserIndexed(
                        GVPrimitiveType.Points,
                        GVOscilloscopeWaveShader,
                        VertexPositionColor.VertexDeclaration,
                        PointsVertices.Array,
                        0,
                        PointsVertices.Count,
                        PointsIndices.Array,
                        num7,
                        num9
                    );
                    num7 += num9;
                    num8 -= num9;
                }
                DrawUserIndexed(
                    GVPrimitiveType.LineStrip,
                    GVOscilloscopeWaveShader,
                    VertexPositionColor.VertexDeclaration,
                    LineStripeVertices.Array,
                    0,
                    LineStripeVertices.Count,
                    LineStripeIndices.Array,
                    0,
                    LineStripeIndices.Count
                );
                LineStripeIndices.Clear();
                LineStripeVertices.Clear();
                PointsIndices.Clear();
                PointsVertices.Clear();
            }
        }

        public void PrepareBackground(float leftOffset, float upOffset, float horizontalSpacing, float verticalSpacing, float dashLength, float dashAndGapLength) {
            GVOscilloscopeBackgroundShader.LeftOffset = leftOffset;
            GVOscilloscopeBackgroundShader.UpOffset = upOffset;
            GVOscilloscopeBackgroundShader.HorizontalSpacing = horizontalSpacing;
            GVOscilloscopeBackgroundShader.VerticalSpacing = verticalSpacing;
            GVOscilloscopeBackgroundShader.DashLength = dashLength;
            GVOscilloscopeBackgroundShader.DashAndGapLength = dashAndGapLength;
        }

        public static void DrawUserIndexed<T>(GVPrimitiveType primitiveType, Shader shader, VertexDeclaration vertexDeclaration, T[] vertexData, int startVertex, int verticesCount, int[] indexData, int startIndex, int indicesCount) where T : struct {
            //VerifyParametersDrawUserIndexed(primitiveType, shader, vertexDeclaration, vertexData, startVertex, verticesCount, indexData, startIndex, indicesCount);
            GCHandle gCHandle = GCHandle.Alloc(vertexData, GCHandleType.Pinned);
            GCHandle gCHandle2 = GCHandle.Alloc(indexData, GCHandleType.Pinned);
            try {
                GLWrapper.ApplyRenderTarget(Display.RenderTarget);
                GLWrapper.ApplyViewportScissor(Display.Viewport, Display.ScissorRectangle, Display.RasterizerState.ScissorTestEnable);
                GLWrapper.ApplyShaderAndBuffers(
                    shader,
                    vertexDeclaration,
                    gCHandle.AddrOfPinnedObject(),
                    0,
                    0
                );
                GLWrapper.ApplyRasterizerState(Display.RasterizerState);
                GLWrapper.ApplyDepthStencilState(Display.DepthStencilState);
                GLWrapper.ApplyBlendState(Display.BlendState);
#pragma warning disable CS0618 // 类型或成员已过时
                GL.DrawElements(TranslateGVPrimitiveType(primitiveType), indicesCount, All.UnsignedInt, gCHandle2.AddrOfPinnedObject() + 4 * startIndex);
#pragma warning restore CS0618 // 类型或成员已过时
            }
            finally {
                gCHandle.Free();
                gCHandle2.Free();
            }
        }

        public enum GVPrimitiveType {
            LineList,
            LineStrip,
            TriangleList,
            TriangleStrip,
            Points
        }

        public static All TranslateGVPrimitiveType(GVPrimitiveType type) => type switch {
            GVPrimitiveType.LineList => All.Lines,
            GVPrimitiveType.LineStrip => All.LineStrip,
            GVPrimitiveType.TriangleList => All.Triangles,
            GVPrimitiveType.TriangleStrip => All.TriangleStrip,
            GVPrimitiveType.Points => All.Points,
            _ => throw new InvalidOperationException("Unsupported primitive type.")
        };
    }
}