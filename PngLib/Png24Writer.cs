using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;

// http://libpng.nigilist.ru/pub/png/spec/1.2/PNG-Contents.html

namespace PngLib
{
    /// <summary>
    /// Class that can output a PNG-24 file from a GDI+ bitmap.
    /// This outputs smaller PNGs because it performs "precompression"
    /// </summary>
    internal class Png24Writer : PngWriterInternal
    {
        /// <summary>
        /// This is a single-use class, 
        /// and is only constructed so that member variables can hold state while the operation is occurring.
        /// </summary>
        internal Png24Writer(Bitmap bitmap, Stream outputStream, PngFilterType filterType)
            : base(bitmap, outputStream, ColorType.RGB, filterType)
        {
            switch (bitmap.PixelFormat) {
                case PixelFormat.Format32bppArgb:
                    _colorType = ColorType.RGBA;
                    _bytesPerPixel = 4;
                    break;
                case PixelFormat.Format24bppRgb:
                    _colorType = ColorType.RGB;
                    _bytesPerPixel = 3;
                    break;
                default:
                    throw new Exception(String.Format("Png24Writer: Unsupported pixel format {0}", bitmap.PixelFormat));
            }

            _paletteBitDepth = 8;
        }

        private readonly int _bytesPerPixel;

        unsafe protected override void WriteBitmapData(DeflaterOutputStream compressionStream)
        {
            BitmapData bitmapData = null;
            try
            {
                int height = _bitmap.Height;
                int width = _bitmap.Width;

                bitmapData = _bitmap.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, _bitmap.PixelFormat);

                var rgbValues = new byte[bitmapData.Width * _bytesPerPixel];
                int len = rgbValues.Length;

                for (int y = 0; y < height; y++)
                {
                    var row = new IntPtr((byte*)bitmapData.Scan0 + (y * bitmapData.Stride));
                    System.Runtime.InteropServices.Marshal.Copy(row, rgbValues, 0, len);

                    fixed (byte* bmp = rgbValues)
                    {
                        // PNG = RGB[A]
                        // GDI = BGR[A]
                        // need to swap B,R
                        for (int i = 0; i < len; i += _bytesPerPixel)
                        {
                            byte tmp = bmp[i];
                            bmp[i] = bmp[i + 2];
                            bmp[i + 2] = tmp;
                        }
                    }

                    WriteScanline(compressionStream, rgbValues, _bytesPerPixel);
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
