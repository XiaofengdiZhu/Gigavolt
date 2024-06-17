using System;

namespace Game {
    public struct GVPoint3 : IEquatable<GVPoint3> {
        public int X;
        public int Y;
        public int Z;
        public uint SubterrainId;

        public GVPoint3(int x, int y, int z, uint subterrainId = 0) {
            X = x;
            Y = y;
            Z = z;
            SubterrainId = subterrainId;
        }

        public Engine.Point3 Point => new(X, Y, Z);

        public override bool Equals(object obj) => obj is GVPoint3 other && Equals(other);

        public bool Equals(GVPoint3 other) => X == other.X && Y == other.Y && Z == other.Z && SubterrainId == other.SubterrainId;

        public override int GetHashCode() => HashCode.Combine(X, Y, Z, SubterrainId);

        public static bool operator ==(GVPoint3 left, GVPoint3 right) => left.Equals(right);

        public static bool operator !=(GVPoint3 left, GVPoint3 right) => !left.Equals(right);
    }
}