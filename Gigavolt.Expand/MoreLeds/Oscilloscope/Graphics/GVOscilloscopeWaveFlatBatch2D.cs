extern alias OpenTKForWindows;
extern alias OpenTKForAndroid;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Game;

namespace Engine.Graphics {
    public class GVOscilloscopeWaveFlatBatch2D : FlatBatch2D {
        public readonly DynamicArray<VertexPositionColor> PointsVertices = [];
        public readonly DynamicArray<int> PointsIndices = [];
        public readonly DynamicArray<VertexPositionColor> LineStripeVertices = [];
        public readonly DynamicArray<int> LineStripeIndices = [];
        public static readonly GVOscilloscopeBackgroundShader GVOscilloscopeBackgroundShader = new();
        public static readonly GVOscilloscopeWaveShader GVOscilloscopeWaveShader = new();
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
            if (LineIndices.Count != 0
                || PointsIndices.Count != 0) {
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
                    GVPrimitiveType.LineList,
                    GVOscilloscopeWaveShader,
                    VertexPositionColor.VertexDeclaration,
                    LineVertices.Array,
                    0,
                    LineVertices.Count,
                    LineIndices.Array,
                    0,
                    LineIndices.Count
                );
                LineIndices.Clear();
                LineVertices.Clear();
                PointsIndices.Clear();
                PointsVertices.Clear();
            }
        }

        public void PrepareBackground(float horizontalSpacing, float verticalSpacing, float dashLength, float dashAndGapLength) {
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
                switch (VersionsManager.Platform) {
                    case Platform.Windows:
                        WindowsGLDrawElements(primitiveType, indicesCount, gCHandle2.AddrOfPinnedObject() + 4 * startIndex);
                        break;
                    case Platform.Android:
                        AndroidGLDrawElements(primitiveType, indicesCount, gCHandle2.AddrOfPinnedObject() + 4 * startIndex);
                        break;
                }
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
#pragma warning disable CS0618 // 类型或成员已过时
        public static void WindowsGLDrawElements(GVPrimitiveType primitiveType, int indicesCount, IntPtr indices) {
            OpenTKForWindows::OpenTK.Graphics.ES30.GL.DrawElements(WindowsTranslateGVPrimitiveType(primitiveType), indicesCount, OpenTKForWindows::OpenTK.Graphics.ES30.DrawElementsType.UnsignedInt, indices);
        }


        public static OpenTKForWindows::OpenTK.Graphics.ES30.PrimitiveType WindowsTranslateGVPrimitiveType(GVPrimitiveType type) => type switch {
            GVPrimitiveType.LineList => OpenTKForWindows::OpenTK.Graphics.ES30.PrimitiveType.Lines,
            GVPrimitiveType.LineStrip => OpenTKForWindows::OpenTK.Graphics.ES30.PrimitiveType.LineStrip,
            GVPrimitiveType.TriangleList => OpenTKForWindows::OpenTK.Graphics.ES30.PrimitiveType.Triangles,
            GVPrimitiveType.TriangleStrip => OpenTKForWindows::OpenTK.Graphics.ES30.PrimitiveType.TriangleStrip,
            GVPrimitiveType.Points => OpenTKForWindows::OpenTK.Graphics.ES30.PrimitiveType.Points,
            _ => throw new InvalidOperationException("Unsupported primitive type.")
        };
#pragma warning restore CS0618 // 类型或成员已过时
        public static void AndroidGLDrawElements(GVPrimitiveType primitiveType, int indicesCount, IntPtr indices) {
            OpenTKForAndroid::OpenTK.Graphics.ES30.GL.DrawElements(AndroidTranslateGVPrimitiveType(primitiveType), indicesCount, OpenTKForAndroid::OpenTK.Graphics.ES30.DrawElementsType.UnsignedInt, indices);
        }

        public static OpenTKForAndroid::OpenTK.Graphics.ES30.BeginMode AndroidTranslateGVPrimitiveType(GVPrimitiveType type) => type switch {
            GVPrimitiveType.LineList => OpenTKForAndroid::OpenTK.Graphics.ES30.BeginMode.Lines,
            GVPrimitiveType.LineStrip => OpenTKForAndroid::OpenTK.Graphics.ES30.BeginMode.LineStrip,
            GVPrimitiveType.TriangleList => OpenTKForAndroid::OpenTK.Graphics.ES30.BeginMode.Triangles,
            GVPrimitiveType.TriangleStrip => OpenTKForAndroid::OpenTK.Graphics.ES30.BeginMode.TriangleStrip,
            GVPrimitiveType.Points => OpenTKForAndroid::OpenTK.Graphics.ES30.BeginMode.Points,
            _ => throw new InvalidOperationException("Unsupported primitive type.")
        };
    }
}