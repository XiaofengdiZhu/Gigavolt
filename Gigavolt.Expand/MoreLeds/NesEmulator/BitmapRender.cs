using System;
using Engine.Media;
using SixLabors.ImageSharp.PixelFormats;

namespace Game {
    /// <summary>
    ///     Renderer that generates SKBitmaps from input data
    /// </summary>
    public class BitmapRenderer {
        readonly Image _bitmap;
        readonly Rgba32[] _colorPalette;
        readonly System.Random _random = new(DateTime.Now.GetHashCode());

        /// <summary>
        ///     Default Constructor
        /// </summary>
        public BitmapRenderer() {
            //Set Palette based on NES 2C02 Palette
            //More Info: https://wiki.nesdev.com/w/index.php/PPU_palettes
            _colorPalette = new Rgba32[0x40];
            _colorPalette[0x0] = new Rgba32(84, 84, 84);
            _colorPalette[0x1] = new Rgba32(0, 30, 116);
            _colorPalette[0x2] = new Rgba32(8, 16, 144);
            _colorPalette[0x3] = new Rgba32(48, 0, 136);
            _colorPalette[0x4] = new Rgba32(68, 0, 100);
            _colorPalette[0x5] = new Rgba32(92, 0, 48);
            _colorPalette[0x6] = new Rgba32(84, 4, 0);
            _colorPalette[0x7] = new Rgba32(60, 24, 0);
            _colorPalette[0x8] = new Rgba32(32, 42, 0);
            _colorPalette[0x9] = new Rgba32(8, 58, 0);
            _colorPalette[0xa] = new Rgba32(0, 64, 0);
            _colorPalette[0xb] = new Rgba32(0, 60, 0);
            _colorPalette[0xc] = new Rgba32(0, 50, 60);
            _colorPalette[0xd] = new Rgba32(0, 0, 0);
            _colorPalette[0xe] = new Rgba32(0, 0, 0);
            _colorPalette[0xf] = new Rgba32(0, 0, 0);
            _colorPalette[0x10] = new Rgba32(152, 150, 152);
            _colorPalette[0x11] = new Rgba32(8, 76, 196);
            _colorPalette[0x12] = new Rgba32(48, 50, 236);
            _colorPalette[0x13] = new Rgba32(92, 30, 228);
            _colorPalette[0x14] = new Rgba32(136, 20, 176);
            _colorPalette[0x15] = new Rgba32(160, 20, 100);
            _colorPalette[0x16] = new Rgba32(152, 34, 32);
            _colorPalette[0x17] = new Rgba32(120, 60, 0);
            _colorPalette[0x18] = new Rgba32(84, 90, 0);
            _colorPalette[0x19] = new Rgba32(40, 114, 0);
            _colorPalette[0x1a] = new Rgba32(8, 124, 0);
            _colorPalette[0x1b] = new Rgba32(0, 118, 40);
            _colorPalette[0x1c] = new Rgba32(0, 102, 120);
            _colorPalette[0x1d] = new Rgba32(0, 0, 0);
            _colorPalette[0x1e] = new Rgba32(0, 0, 0);
            _colorPalette[0x1f] = new Rgba32(0, 0, 0);
            _colorPalette[0x20] = new Rgba32(236, 238, 236);
            _colorPalette[0x21] = new Rgba32(76, 154, 236);
            _colorPalette[0x22] = new Rgba32(120, 124, 236);
            _colorPalette[0x23] = new Rgba32(176, 98, 236);
            _colorPalette[0x24] = new Rgba32(228, 84, 236);
            _colorPalette[0x25] = new Rgba32(236, 88, 180);
            _colorPalette[0x26] = new Rgba32(236, 106, 100);
            _colorPalette[0x27] = new Rgba32(212, 136, 32);
            _colorPalette[0x28] = new Rgba32(160, 170, 0);
            _colorPalette[0x29] = new Rgba32(116, 196, 0);
            _colorPalette[0x2a] = new Rgba32(76, 208, 32);
            _colorPalette[0x2b] = new Rgba32(56, 204, 108);
            _colorPalette[0x2c] = new Rgba32(56, 180, 204);
            _colorPalette[0x2d] = new Rgba32(60, 60, 60);
            _colorPalette[0x2e] = new Rgba32(0, 0, 0);
            _colorPalette[0x2f] = new Rgba32(0, 0, 0);
            _colorPalette[0x30] = new Rgba32(236, 238, 236);
            _colorPalette[0x31] = new Rgba32(168, 204, 236);
            _colorPalette[0x32] = new Rgba32(188, 188, 236);
            _colorPalette[0x33] = new Rgba32(212, 178, 236);
            _colorPalette[0x34] = new Rgba32(236, 174, 236);
            _colorPalette[0x35] = new Rgba32(236, 174, 212);
            _colorPalette[0x36] = new Rgba32(236, 180, 176);
            _colorPalette[0x37] = new Rgba32(228, 196, 144);
            _colorPalette[0x38] = new Rgba32(204, 210, 120);
            _colorPalette[0x39] = new Rgba32(180, 222, 120);
            _colorPalette[0x3a] = new Rgba32(168, 226, 144);
            _colorPalette[0x3b] = new Rgba32(152, 226, 180);
            _colorPalette[0x3c] = new Rgba32(160, 214, 228);
            _colorPalette[0x3d] = new Rgba32(160, 162, 160);
            _colorPalette[0x3e] = new Rgba32(0, 0, 0);
            _colorPalette[0x3f] = new Rgba32(0, 0, 0);

            //SKBitmap to reuse for each frame
            _bitmap = new Image(256, 240);
        }

        /// <summary>
        ///     Takes the input 8bpp bitmap and renders it as a SKBitmap
        ///     using the pre-defined Color Palette
        /// </summary>
        /// <param name="bitmap"></param>
        /// <returns></returns>
        public Image Render(byte[] bitmap) {
            for (int y = 0; y < 240; y++) {
                for (int x = 0; x < 256; x++) {
                    _bitmap.SetPixelFast(x, y, _colorPalette[bitmap[y * 256 + x]]);
                }
            }
            return _bitmap;
        }

        /// <summary>
        ///     Renders a black/white noise pattern
        /// </summary>
        /// <returns></returns>
        public byte[] GenerateNoise() {
            byte[] output = new byte[256 * 240];
            for (int i = 0; i < output.Length; i++) {
                output[i] = _random.Next(0, 10) <= 5 ? (byte)0xd : (byte)0x30;
            }
            return output;
        }
    }
}