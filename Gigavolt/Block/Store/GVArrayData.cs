using System;
using System.IO;
using Engine.Media;

namespace Game {
    public abstract class GVArrayData : IEditableItemData {
        public uint m_ID;
        public string m_worldDirectory;
        public DateTime m_updateTime;
        public bool m_isDataInitialized;
        public abstract uint Read(uint index);
        public abstract void Write(uint index, uint data);
        public abstract IEditableItemData Copy();

        public abstract void LoadData();
        public abstract void LoadString(string data);

        public abstract string SaveString();
        public virtual void String2Data(string data, int width = 0, int height = 0) { }
        public virtual string Data2String() => null;
        public virtual byte[] Data2Bytes(int startIndex = 0, int length = int.MaxValue) => null;
        public virtual short[] Data2Shorts() => null;
        public virtual void Shorts2Data(short[] shorts) { }

        public virtual Image Data2Image() => null;
        public virtual void Image2Data(Image image) { }
        public virtual void Stream2Data(Stream stream) { }
    }
}