using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

namespace VP.VPSystem.Drawing
{
    /// <summary>
    /// Class that can output a PNG-8 file from a GDI+ bitmap.
    /// .Net Framework (2.0) only supports writing 24-bit PNG files.
    /// </summary>
    internal class Png8Writer : PngWriter
    {
        private int _maxTransparentPaletteIndex = -1;
        private int _paletteSize;

        /// <summary>
        /// This is a single-use class, 
        /// and is only constructed so that member variables can hold state while the operation is occurring.
        /// </summary>
        internal Png8Writer(Bitmap bitmap, Stream outputStream, int paletteSize, PngFilterType filterType)
            : base(bitmap, outputStream, ColorType.Palette, filterType)
        {
            switch (bitmap.PixelFormat) {
                case PixelFormat.Format8bppIndexed:
                case PixelFormat.Format4bppIndexed:
                case PixelFormat.Format1bppIndexed:
                    break;
                default:
                    throw new System.Exception(String.Format("Png24Writer: Unsupported pixel format {0}", bitmap.PixelFormat));
            }

            _paletteSize = paletteSize;
            if (_paletteSize < 0 || _paletteSize > 256)
            {
                throw new System.ArgumentException(String.Format("Invalid paletteSize {0}", paletteSize));
            }

            if (_paletteSize <= 2)
            {
                _paletteBitDepth = 1;
            }
            else if (_paletteSize <= 4)
            {
                _paletteBitDepth = 2;
            }
            else if (_paletteSize <= 16)
            {
                _paletteBitDepth = 4;
            }
            else
            {
                _paletteBitDepth = 8;
            }
        }

        /// <summary>
        /// Writes the transparency chunk.  Must be called after writing the palette.
        /// </summary>
        protected override void WriteTransparency()
        {
            if (_maxTransparentPaletteIndex == -1)
            {
                return;
            }

            byte[] data = new byte[_maxTransparentPaletteIndex + 1];
            ColorPalette palette = _bitmap.Palette;
            for (int colorIndex = 0; colorIndex <= _maxTransparentPaletteIndex; colorIndex++)
            {
                data[colorIndex] = palette.Entries[colorIndex].A;
            }
            // All other colors will get full opacity
            WriteChunk(CHUNK_TYPE_TRANSPARENCY, data);
        }

        protected override void WritePalette()
        {
            _maxTransparentPaletteIndex = -1;

            ColorPalette palette = _bitmap.Palette;
            byte[] data = new byte[_paletteSize * 3];
            for (int colorIndex = 0; colorIndex < _paletteSize; colorIndex++)
            {
                Color entry = palette.Entries[colorIndex];
                data[colorIndex * 3] = entry.R;
                data[colorIndex * 3 + 1] = entry.G;
                data[colorIndex * 3 + 2] = entry.B;
                if (entry.A < 255)
                {
                    _maxTransparentPaletteIndex = colorIndex;
                }
            }

            WriteChunk(CHUNK_TYPE_PALETTE, data);
        }

        protected override unsafe void WriteBitmapData(DeflaterOutputStream compressionStream)
        {
            BitmapData bitmapData = null;
            try
            {
                int increment = 8 / _paletteBitDepth;
                int width = _bitmap.Width;
                int height = _bitmap.Height;
                int rowSize = (int)Math.Ceiling(width / (double)increment);

                bitmapData = _bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format8bppIndexed);

                for (int y = 0; y < height; y++)
                {
                    byte[] rowData = new byte[rowSize];
                    byte* row = (byte*)bitmapData.Scan0 + (y * bitmapData.Stride);
                    for (int x = 0, rowDataOffset = 0; x < width; x += increment, rowDataOffset++)
                    {
                        byte dataSoFar = 0;
                        for (int x1 = 0; x1 < increment; x1++)
                        {
                            dataSoFar <<= _paletteBitDepth;
                            if (x + x1 < width)
                            {
                                dataSoFar |= row[x + x1];
                            }
                        }
                        rowData[rowDataOffset] = dataSoFar;
                    }

                    WriteScanline(compressionStream, rowData, 1);
                }
            }
            finally
            {
                if (bitmapData != null)
                {
                    _bitmap.UnlockBits(bitmapData);
                }
            }
        }
    }
}
