using System;
using Engine;
using Engine.Media;

namespace Game
{
    /// <summary>
    ///     Renderer that generates SKBitmaps from input data
    /// </summary>
    public class BitmapRenderer
    {
        private readonly Image _bitmap;
        private readonly Color[] _colorPalette;
        private readonly System.Random _random = new System.Random(DateTime.Now.GetHashCode());

        /// <summary>
        ///     Default Constructor
        /// </summary>
        public BitmapRenderer()
        {
            //Set Palette based on NES 2C02 Palette
            //More Info: https://wiki.nesdev.com/w/index.php/PPU_palettes
            _colorPalette = new Color[0x40];
            _colorPalette[0x0] = new Color(84, 84, 84);
            _colorPalette[0x1] = new Color(0, 30, 116);
            _colorPalette[0x2] = new Color(8, 16, 144);
            _colorPalette[0x3] = new Color(48, 0, 136);
            _colorPalette[0x4] = new Color(68, 0, 100);
            _colorPalette[0x5] = new Color(92, 0, 48);
            _colorPalette[0x6] = new Color(84, 4, 0);
            _colorPalette[0x7] = new Color(60, 24, 0);
            _colorPalette[0x8] = new Color(32, 42, 0);
            _colorPalette[0x9] = new Color(8, 58, 0);
            _colorPalette[0xa] = new Color(0, 64, 0);
            _colorPalette[0xb] = new Color(0, 60, 0);
            _colorPalette[0xc] = new Color(0, 50, 60);
            _colorPalette[0xd] = new Color(0, 0, 0);
            _colorPalette[0xe] = new Color(0, 0, 0);
            _colorPalette[0xf] = new Color(0, 0, 0);
            _colorPalette[0x10] = new Color(152, 150, 152);
            _colorPalette[0x11] = new Color(8, 76, 196);
            _colorPalette[0x12] = new Color(48, 50, 236);
            _colorPalette[0x13] = new Color(92, 30, 228);
            _colorPalette[0x14] = new Color(136, 20, 176);
            _colorPalette[0x15] = new Color(160, 20, 100);
            _colorPalette[0x16] = new Color(152, 34, 32);
            _colorPalette[0x17] = new Color(120, 60, 0);
            _colorPalette[0x18] = new Color(84, 90, 0);
            _colorPalette[0x19] = new Color(40, 114, 0);
            _colorPalette[0x1a] = new Color(8, 124, 0);
            _colorPalette[0x1b] = new Color(0, 118, 40);
            _colorPalette[0x1c] = new Color(0, 102, 120);
            _colorPalette[0x1d] = new Color(0, 0, 0);
            _colorPalette[0x1e] = new Color(0, 0, 0);
            _colorPalette[0x1f] = new Color(0, 0, 0);
            _colorPalette[0x20] = new Color(236, 238, 236);
            _colorPalette[0x21] = new Color(76, 154, 236);
            _colorPalette[0x22] = new Color(120, 124, 236);
            _colorPalette[0x23] = new Color(176, 98, 236);
            _colorPalette[0x24] = new Color(228, 84, 236);
            _colorPalette[0x25] = new Color(236, 88, 180);
            _colorPalette[0x26] = new Color(236, 106, 100);
            _colorPalette[0x27] = new Color(212, 136, 32);
            _colorPalette[0x28] = new Color(160, 170, 0);
            _colorPalette[0x29] = new Color(116, 196, 0);
            _colorPalette[0x2a] = new Color(76, 208, 32);
            _colorPalette[0x2b] = new Color(56, 204, 108);
            _colorPalette[0x2c] = new Color(56, 180, 204);
            _colorPalette[0x2d] = new Color(60, 60, 60);
            _colorPalette[0x2e] = new Color(0, 0, 0);
            _colorPalette[0x2f] = new Color(0, 0, 0);
            _colorPalette[0x30] = new Color(236, 238, 236);
            _colorPalette[0x31] = new Color(168, 204, 236);
            _colorPalette[0x32] = new Color(188, 188, 236);
            _colorPalette[0x33] = new Color(212, 178, 236);
            _colorPalette[0x34] = new Color(236, 174, 236);
            _colorPalette[0x35] = new Color(236, 174, 212);
            _colorPalette[0x36] = new Color(236, 180, 176);
            _colorPalette[0x37] = new Color(228, 196, 144);
            _colorPalette[0x38] = new Color(204, 210, 120);
            _colorPalette[0x39] = new Color(180, 222, 120);
            _colorPalette[0x3a] = new Color(168, 226, 144);
            _colorPalette[0x3b] = new Color(152, 226, 180);
            _colorPalette[0x3c] = new Color(160, 214, 228);
            _colorPalette[0x3d] = new Color(160, 162, 160);
            _colorPalette[0x3e] = new Color(0, 0, 0);
            _colorPalette[0x3f] = new Color(0, 0, 0);

            //SKBitmap to reuse for each frame
            _bitmap = new Image(256, 240);
        }

        /// <summary>
        ///     Takes the input 8bpp bitmap and renders it as a SKBitmap
        ///     using the pre-defined Color Palette
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public Image Render(byte[] bitmap)
        {
            for (var y = 0; y < 240; y++)
            {
                for (var x = 0; x < 256; x++)
                {
                    _bitmap.SetPixel(x, y, _colorPalette[bitmap[y * 256 + x]]);
                }
            }
            return _bitmap;
        }

        /// <summary>
        ///     Renders a black/white noise pattern
        /// </summary>
        /// <returns></returns>
        public byte[] GenerateNoise()
        {
            var output = new byte[256 * 240];
            for (var i = 0; i < output.Length; i++)
            {
                output[i] = _random.Next(0, 10) <= 5 ? (byte)0xd : (byte)0x30;
            }
            return output;
        }
    }
}
