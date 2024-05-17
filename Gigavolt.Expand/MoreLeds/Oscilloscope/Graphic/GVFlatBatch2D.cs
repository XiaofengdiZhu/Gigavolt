using System;
using System.Collections.Generic;

namespace Engine.Graphics {
    public class GVFlatBatch2D : BaseFlatBatch {
        public void QueueLine(Vector2 p1, Vector2 p2, float depth, Color color) {
            int count = LineVertices.Count;
            LineVertices.Add(new VertexPositionColor(new Vector3(p1, depth), color));
            LineVertices.Add(new VertexPositionColor(new Vector3(p2, depth), color));
            LineIndices.Add(count);
            LineIndices.Add(count + 1);
        }

        public void QueueLineStrip(IEnumerable<Vector2> points, float depth, Color color) {
            int count = LineVertices.Count;
            int num = 0;
            foreach (Vector2 point in points) {
                LineVertices.Add(new VertexPositionColor(new Vector3(point, depth), color));
                num++;
            }
            for (int i = 0; i < num - 1; i++) {
                LineIndices.Add(count + i);
                LineIndices.Add(count + i + 1);
            }
        }

        public void QueueRectangle(Vector2 corner1, Vector2 corner2, float depth, Color color) {
            int count = LineVertices.Count;
            LineVertices.Add(new VertexPositionColor(new Vector3(corner1.X, corner1.Y, depth), color));
            LineVertices.Add(new VertexPositionColor(new Vector3(corner1.X, corner2.Y, depth), color));
            LineVertices.Add(new VertexPositionColor(new Vector3(corner2.X, corner2.Y, depth), color));
            LineVertices.Add(new VertexPositionColor(new Vector3(corner2.X, corner1.Y, depth), color));
            LineIndices.Add(count);
            LineIndices.Add(count + 1);
            LineIndices.Add(count + 1);
            LineIndices.Add(count + 2);
            LineIndices.Add(count + 2);
            LineIndices.Add(count + 3);
            LineIndices.Add(count + 3);
            LineIndices.Add(count);
        }

        public void QueueEllipse(Vector2 center, Vector2 radius, float depth, Color color, int sides = 32, float startAngle = 0f, float endAngle = (float)Math.PI * 2f) {
            Vector2 p = Vector2.Zero;
            for (int i = 0; i <= sides; i++) {
                float x = MathUtils.Lerp(startAngle, endAngle, i / (float)sides);
                Vector2 vector = center + radius * new Vector2(MathF.Sin(x), 0f - MathF.Cos(x));
                if (i > 0) {
                    QueueLine(p, vector, depth, color);
                }
                p = vector;
            }
        }

        public void QueueDisc(Vector2 center, Vector2 radius, float depth, Color color, int sides = 32, float startAngle = 0f, float endAngle = (float)Math.PI * 2f) {
            Vector2 p = Vector2.Zero;
            for (int i = 0; i <= sides; i++) {
                float x = MathUtils.Lerp(startAngle, endAngle, i / (float)sides);
                Vector2 vector = center + radius * new Vector2(MathF.Sin(x), 0f - MathF.Cos(x));
                if (i > 0) {
                    QueueTriangle(
                        p,
                        vector,
                        center,
                        depth,
                        color
                    );
                }
                p = vector;
            }
        }

        public void QueueDisc(Vector2 center, Vector2 outerRadius, Vector2 innerRadius, float depth, Color outerColor, Color innerColor, int sides = 32, float startAngle = 0f, float endAngle = (float)Math.PI * 2f) {
            Vector2 p = Vector2.Zero;
            Vector2 p2 = Vector2.Zero;
            for (int i = 0; i <= sides; i++) {
                float x = MathUtils.Lerp(startAngle, endAngle, i / (float)sides);
                Vector2 v = new Vector2(MathF.Sin(x), 0f - MathF.Cos(x));
                Vector2 vector = center + outerRadius * v;
                Vector2 vector2 = center + innerRadius * v;
                if (i > 0) {
                    QueueTriangle(
                        p,
                        vector,
                        p2,
                        depth,
                        outerColor,
                        outerColor,
                        innerColor
                    );
                    QueueTriangle(
                        vector,
                        vector2,
                        p2,
                        depth,
                        outerColor,
                        innerColor,
                        innerColor
                    );
                }
                p = vector;
                p2 = vector2;
            }
        }

        public void QueueTriangle(Vector2 p1, Vector2 p2, Vector2 p3, float depth, Color color) {
            int count = TriangleVertices.Count;
            TriangleVertices.Add(new VertexPositionColor(new Vector3(p1.X, p1.Y, depth), color));
            TriangleVertices.Add(new VertexPositionColor(new Vector3(p2.X, p2.Y, depth), color));
            TriangleVertices.Add(new VertexPositionColor(new Vector3(p3.X, p3.Y, depth), color));
            TriangleIndices.Add(count);
            TriangleIndices.Add(count + 1);
            TriangleIndices.Add(count + 2);
        }

        public void QueueTriangle(Vector2 p1, Vector2 p2, Vector2 p3, float depth, Color color1, Color color2, Color color3) {
            int count = TriangleVertices.Count;
            TriangleVertices.Add(new VertexPositionColor(new Vector3(p1.X, p1.Y, depth), color1));
            TriangleVertices.Add(new VertexPositionColor(new Vector3(p2.X, p2.Y, depth), color2));
            TriangleVertices.Add(new VertexPositionColor(new Vector3(p3.X, p3.Y, depth), color3));
            TriangleIndices.Add(count);
            TriangleIndices.Add(count + 1);
            TriangleIndices.Add(count + 2);
        }

        public void QueueQuad(Vector2 corner1, Vector2 corner2, float depth, Color color) {
            int count = TriangleVertices.Count;
            TriangleVertices.Add(new VertexPositionColor(new Vector3(corner1.X, corner1.Y, depth), color));
            TriangleVertices.Add(new VertexPositionColor(new Vector3(corner1.X, corner2.Y, depth), color));
            TriangleVertices.Add(new VertexPositionColor(new Vector3(corner2.X, corner2.Y, depth), color));
            TriangleVertices.Add(new VertexPositionColor(new Vector3(corner2.X, corner1.Y, depth), color));
            TriangleIndices.Add(count);
            TriangleIndices.Add(count + 1);
            TriangleIndices.Add(count + 2);
            TriangleIndices.Add(count + 2);
            TriangleIndices.Add(count + 3);
            TriangleIndices.Add(count);
        }

        public void QueueQuad(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, float depth, Color color) {
            int count = TriangleVertices.Count;
            TriangleVertices.Count += 4;
            TriangleVertices.Array[count] = new VertexPositionColor(new Vector3(p1.X, p1.Y, depth), color);
            TriangleVertices.Array[count + 1] = new VertexPositionColor(new Vector3(p2.X, p2.Y, depth), color);
            TriangleVertices.Array[count + 2] = new VertexPositionColor(new Vector3(p3.X, p3.Y, depth), color);
            TriangleVertices.Array[count + 3] = new VertexPositionColor(new Vector3(p4.X, p4.Y, depth), color);
            int count2 = TriangleIndices.Count;
            TriangleIndices.Count += 6;
            TriangleIndices.Array[count2] = count;
            TriangleIndices.Array[count2 + 1] = count + 1;
            TriangleIndices.Array[count2 + 2] = count + 2;
            TriangleIndices.Array[count2 + 3] = count + 2;
            TriangleIndices.Array[count2 + 4] = count + 3;
            TriangleIndices.Array[count2 + 5] = count;
        }

        public void QueueQuad(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, float depth, Color color1, Color color2, Color color3, Color color4) {
            int count = TriangleVertices.Count;
            TriangleVertices.Count += 4;
            TriangleVertices.Array[count] = new VertexPositionColor(new Vector3(p1.X, p1.Y, depth), color1);
            TriangleVertices.Array[count + 1] = new VertexPositionColor(new Vector3(p2.X, p2.Y, depth), color2);
            TriangleVertices.Array[count + 2] = new VertexPositionColor(new Vector3(p3.X, p3.Y, depth), color3);
            TriangleVertices.Array[count + 3] = new VertexPositionColor(new Vector3(p4.X, p4.Y, depth), color4);
            int count2 = TriangleIndices.Count;
            TriangleIndices.Count += 6;
            TriangleIndices.Array[count2] = count;
            TriangleIndices.Array[count2 + 1] = count + 1;
            TriangleIndices.Array[count2 + 2] = count + 2;
            TriangleIndices.Array[count2 + 3] = count + 2;
            TriangleIndices.Array[count2 + 4] = count + 3;
            TriangleIndices.Array[count2 + 5] = count;
        }

        public void TransformLines(Matrix matrix, int start = 0, int end = -1) {
            VertexPositionColor[] array = LineVertices.Array;
            if (end < 0) {
                end = LineVertices.Count;
            }
            for (int i = start; i < end; i++) {
                Vector2 v = array[i].Position.XY;
                Vector2.Transform(ref v, ref matrix, out v);
                array[i].Position.X = v.X;
                array[i].Position.Y = v.Y;
            }
        }

        public void TransformTriangles(Matrix matrix, int start = 0, int end = -1) {
            VertexPositionColor[] array = TriangleVertices.Array;
            if (end < 0) {
                end = TriangleVertices.Count;
            }
            for (int i = start; i < end; i++) {
                Vector2 v = array[i].Position.XY;
                Vector2.Transform(ref v, ref matrix, out v);
                array[i].Position.X = v.X;
                array[i].Position.Y = v.Y;
            }
        }

        public void Flush(bool clearAfterFlush = true) {
            Flush(PrimitivesRenderer2D.ViewportMatrix(), clearAfterFlush);
        }
    }
}