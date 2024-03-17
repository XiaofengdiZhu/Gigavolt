using System.Runtime.InteropServices;
using Engine;
using OpenTK.Graphics.ES30;

namespace Game {
    public class HookViewWidget : Widget {
        public bool m_stopHook;
        public uint[] m_hookedImage;

        public HookViewWidget() {
            Name = "HookViewWidget";
            IsDrawRequired = true;
        }

        public override void Draw(DrawContext dc) {
            if (!m_stopHook) {
                m_hookedImage = null;
                m_hookedImage = new uint[Window.Size.X * Window.Size.Y];
                GCHandle gcHandle = GCHandle.Alloc(m_hookedImage, GCHandleType.Pinned);
                GL.ReadPixels(
                    0,
                    0,
                    Window.Size.X,
                    Window.Size.Y,
                    PixelFormat.Rgba,
                    PixelType.UnsignedByte,
                    gcHandle.AddrOfPinnedObject()
                );
                gcHandle.Free();
            }
        }
    }
}