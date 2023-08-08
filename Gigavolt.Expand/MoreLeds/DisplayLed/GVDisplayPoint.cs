using System;
using Engine;

namespace Game {
    public class GVDisplayPoint : IEquatable<GVDisplayPoint> {
        public Vector3 Position;
        public Color Color;
        public float Size;
        public uint Value;
        public Vector3 Rotation;
        public bool Complex;
        public int Type;
        public bool CustomBit;

        public bool isValid() => Value != 0 || (Complex && (Color.A == 0 || Size == 0));
        public override bool Equals(object obj) => obj is GVDisplayPoint point && Equals(point);

        public bool Equals(GVDisplayPoint other) {
            if (ReferenceEquals(null, other)) {
                return false;
            }
            if (ReferenceEquals(this, other)) {
                return true;
            }
            return Position.Equals(other.Position) && Color.Equals(other.Color) && Size.Equals(other.Size) && Value == other.Value && Rotation.Equals(other.Rotation) && Complex == other.Complex && Type == other.Type && CustomBit == other.CustomBit;
        }

        public override int GetHashCode() => Position.GetHashCode() ^ Color.GetHashCode() ^ Size.GetHashCode() ^ Value.GetHashCode() ^ Rotation.GetHashCode() ^ Complex.GetHashCode() ^ Type.GetHashCode() ^ CustomBit.GetHashCode();
    }
}