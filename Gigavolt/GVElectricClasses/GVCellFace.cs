using System;
using Engine;

namespace Game {
    public struct GVCellFace(int x, int y, int z, int face, int mask = int.MaxValue) : IEquatable<GVCellFace> {
        public int X = x;
        public int Y = y;
        public int Z = z;
        public int Face = face;
        public readonly int Mask = mask;

        public GVCellFace(CellFace cellFace) : this(cellFace.X, cellFace.Y, cellFace.Z, cellFace.Face) { }
        public GVCellFace(Point3 point3) : this(point3.X, point3.Y, point3.Z, 0) { }

        public static readonly int[] m_oppositeFaces = [
            2,
            3,
            0,
            1,
            5,
            4
        ];

        public static readonly Point3[] m_faceToPoint3 = [
            new Point3(0, 0, 1),
            new Point3(1, 0, 0),
            new Point3(0, 0, -1),
            new Point3(-1, 0, 0),
            new Point3(0, 1, 0),
            new Point3(0, -1, 0)
        ];

        public static readonly Vector3[] m_faceToVector3 = [
            new Vector3(0f, 0f, 1f),
            new Vector3(1f, 0f, 0f),
            new Vector3(0f, 0f, -1f),
            new Vector3(-1f, 0f, 0f),
            new Vector3(0f, 1f, 0f),
            new Vector3(0f, -1f, 0f)
        ];

        public Point3 Point {
            get => new(X, Y, Z);
            set {
                X = value.X;
                Y = value.Y;
                Z = value.Z;
            }
        }

        public CellFace CellFace {
            get => new(X, Y, Z, Face);
            set {
                X = value.X;
                Y = value.Y;
                Z = value.Z;
                Face = value.Face;
            }
        }

        public static int OppositeFace(int face) => m_oppositeFaces[face];

        public static Point3 FaceToPoint3(int face) => m_faceToPoint3[face];

        public static Vector3 FaceToVector3(int face) => m_faceToVector3[face];

        public static int Point3ToFace(Point3 p, int maxFace = 5) {
            for (int i = 0; i < maxFace; i++) {
                if (m_faceToPoint3[i] == p) {
                    return i;
                }
            }
            throw new InvalidOperationException("Invalid Point3.");
        }

        public static int Vector3ToFace(Vector3 v, int maxFace = 5) {
            float num = -1f / 0f;
            int result = 0;
            for (int i = 0; i <= maxFace; i++) {
                float num2 = Vector3.Dot(m_faceToVector3[i], v);
                if (num2 > num) {
                    result = i;
                    num = num2;
                }
            }
            return result;
        }

        public static GVCellFace FromAxisAndDirection(int x, int y, int z, int axis, float direction) {
            GVCellFace result = default;
            result.X = x;
            result.Y = y;
            result.Z = z;
            switch (axis) {
                case 0:
                    result.Face = direction > 0f ? 1 : 3;
                    break;
                case 1:
                    result.Face = direction > 0f ? 4 : 5;
                    break;
                case 2:
                    result.Face = !(direction > 0f) ? 2 : 0;
                    break;
            }
            return result;
        }

        public Plane CalculatePlane() {
            switch (Face) {
                case 0: return new Plane(new Vector3(0f, 0f, 1f), -(Z + 1));
                case 1: return new Plane(new Vector3(-1f, 0f, 0f), X + 1);
                case 2: return new Plane(new Vector3(0f, 0f, -1f), Z);
                case 3: return new Plane(new Vector3(1f, 0f, 0f), -X);
                case 4: return new Plane(new Vector3(0f, 1f, 0f), -(Y + 1));
                default: return new Plane(new Vector3(0f, -1f, 0f), Y);
            }
        }

        public override int GetHashCode() =>
            /*int hash = 17;
            hash = hash * 23 + X;
            hash = hash * 23 + Y;
            hash = hash * 23 + Z;
            hash = hash * 23 + Face;
            hash = hash * 23 + Mask;
            return hash;*/
            (X << 21) + (Z << 9) + (Y << 3) + Face + Mask * 23;

        public override bool Equals(object obj) => obj is GVCellFace cellFace && Equals(cellFace);

        public bool Equals(GVCellFace other) {
            if (other.X == X
                && other.Y == Y
                && other.Z == Z
                && other.Mask == Mask) {
                return other.Face == Face;
            }
            return false;
        }

        public override string ToString() => X + ", " + Y + ", " + Z + ", face " + Face + ", mask " + Mask;

        public static bool operator ==(GVCellFace c1, GVCellFace c2) => c1.Equals(c2);

        public static bool operator !=(GVCellFace c1, GVCellFace c2) => !c1.Equals(c2);
    }
}