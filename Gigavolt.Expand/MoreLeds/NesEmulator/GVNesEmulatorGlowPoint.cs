using Engine;

namespace Game {
    public class GVNesEmulatorGlowPoint {
        public Vector3 Position;

        public Vector3 Right;

        public Vector3 Up;

        public Vector3 Forward;

        public uint Voltage;
        public bool GetPowerOn() => (Voltage & 1u) == 1u;

        public bool GetReset() => ((Voltage >> 1) & 1u) == 1u;
        public uint GetRotation() => (Voltage >> 2) & 3u;

        public byte GetController1() => (byte)((Voltage >> 8) & 255u);
        public byte GetController2() => (byte)((Voltage >> 16) & 255u);

        public byte GetSize() {
            byte size = (byte)((Voltage >> 24) & 127u);
            return size == 0 ? (byte)1 : size;
        }
    }
}