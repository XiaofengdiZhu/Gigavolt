using System;
using System.IO;
using Engine.Graphics;
using Engine.Media;

namespace Game {
    public abstract class GVArrayData : IEditableItemData {
        public uint m_ID;
        public string m_worldDirectory;
        public DateTime m_updateTime;
        public bool m_isDataInitialized;
        public string m_cachedString;
        public DateTime m_cachedStringTime;
        public byte[] m_cachedBytes;
        public DateTime m_cachedBytesTime;
        public short[] m_cachedShorts;
        public DateTime m_cachedShortsTime;
        public Image m_cachedImage;
        public DateTime m_cachedImageTime;
        public Texture2D m_cachedTexture2D;
        public DateTime m_cachedTexture2DTime;

        public abstract uint Read(uint index);
        public abstract void Write(uint index, uint data);
        public abstract IEditableItemData Copy();

        public abstract void LoadData();
        public abstract void LoadString(string data);

        public abstract string SaveString();
        public virtual void String2Data(string data, int width = 0, int height = 0) { }
        public virtual string Data2String() => null;

        public string GetString() {
            if (m_isDataInitialized) {
                if (m_cachedString == null
                    || m_updateTime != m_cachedStringTime) {
                    m_cachedString = Data2String();
                    m_cachedStringTime = m_updateTime;
                }
                return m_cachedString;
            }
            return null;
        }

        public virtual byte[] Data2Bytes(int startIndex = 0, int length = int.MaxValue) => null;

        public byte[] GetBytes(int startIndex = 0, int length = int.MaxValue) {
            if (m_isDataInitialized) {
                if (m_cachedBytes == null
                    || m_updateTime != m_cachedBytesTime) {
                    m_cachedBytes = Data2Bytes(startIndex, length);
                    m_cachedBytesTime = m_updateTime;
                }
                return m_cachedBytes;
            }
            return null;
        }

        public virtual short[] Data2Shorts() => null;

        public short[] GetShorts() {
            if (m_isDataInitialized) {
                if (m_cachedShorts == null
                    || m_updateTime != m_cachedShortsTime) {
                    m_cachedShorts = Data2Shorts();
                    m_cachedShortsTime = m_updateTime;
                }
                return m_cachedShorts;
            }
            return null;
        }

        public virtual void Shorts2Data(short[] shorts) { }

        public virtual Image Data2Image() => null;

        public Image GetImage() {
            if (m_isDataInitialized) {
                if (m_cachedImage == null
                    || m_updateTime != m_cachedImageTime) {
                    m_cachedImage = Data2Image();
                    m_cachedImageTime = m_updateTime;
                }
                return m_cachedImage;
            }
            return null;
        }

        public virtual void Image2Data(Image image) { }
        public virtual void Stream2Data(Stream stream) { }

        public Texture2D Data2Texture2D() {
            Image image = GetImage();
            return image == null ? null : Texture2D.Load(image);
        }

        public Texture2D GetTexture2D() {
            if (m_isDataInitialized) {
                if (m_cachedTexture2D == null
                    || m_updateTime != m_cachedTexture2DTime) {
                    m_cachedTexture2D = Data2Texture2D();
                    m_cachedTexture2DTime = m_updateTime;
                }
                return m_cachedTexture2D;
            }
            return null;
        }
    }
}