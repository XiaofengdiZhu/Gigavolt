using Engine;

namespace Game
{
    public class GVNesEmulatorGlowPoint
    {
        public Vector3 Position;

        public Vector3 Right;

        public Vector3 Up;

        public Vector3 Forward;

        public uint Voltage;
        public bool GetPowerOn()
        {
            return (Voltage & 1u) == 1u;
        }
        public bool GetReset()
        {
            return ((Voltage >> 1) & 1u) == 1u;
        }
        public uint GetRotation()
        {
            return (Voltage >> 2) & 3u;
        }
        public byte GetControler1()
        {
            return (byte)((Voltage >> 8) & 255u);
        }
        public byte GetControler2()
        {
            return (byte)((Voltage >> 16) & 255u);
        }
        public byte GetSize()
        {
            byte size = (byte)((Voltage >> 24) & 127u);
            return size == 0 ? (byte)1 : size;
        }
    }
}