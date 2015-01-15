using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace PngLib
{
    public static class PngWriter
    {
        /// <summary>
        /// Writes a 32 bit PNG with alpha to the specified stream
        /// </summary>
        public static void WritePng(Image image, Stream outputStream)
        {
            WritePng(image, outputStream, null);
        }

        /// <summary>
        /// Writes a 32 bit PNG with alpha to the specified stream
        /// </summary>
        /// <param name="image">The bitmap.</param>
        /// <param name="outputStream">The output stream.</param>
        /// <param name="options">if set to <c>true</c> [use expensive optimizations].</param>
        public static void WritePng(Image image, Stream outputStream, PngOptimizationOptions options)
        {
            // A reasonable default
            options = options ?? new PngOptimizationOptions {FilterType = PngFilterType.None};

            if (options.PaletteSize > 0)
            {
                WritePng8(image, outputStream, options);
                return;
            }

            if (options.FilterType.HasValue)
            {
                WritePng(image, outputStream, options.FilterType.Value);
                return;
            }

            //Bake-off
            // Sub vs None precompression
            var subStream = new ChunkedMemoryStream();
            WritePng(image, subStream, PngFilterType.Sub);

            var noneStream = new ChunkedMemoryStream();
            WritePng(image, noneStream, PngFilterType.None);

            if (subStream.Length < noneStream.Length)
            {
                subStream.WriteTo(outputStream);
            }
            else
            {
                noneStream.WriteTo(outputStream);
            }
        }

        /// <summary>
        /// Write a PNG file to the specified stream with the specified pre-compression filter
        /// </summary>
        /// <param name="image"></param>
        /// <param name="outputStream"></param>
        /// <param name="filterType">The pre-compression filter type to use</param>
        internal static void WritePng(Image image, Stream outputStream, PngFilterType filterType)
        {
            using (var bmp = new DisposableBitmapWrapper(image))
            {
                var writer = new Png24Writer(bmp.Bitmap, outputStream, filterType);
                writer.Write();
            }
        }

        /// <summary>
        /// Writes a 8 bit, quantized PNG image to the specified stream.
        /// </summary>
        /// <param name="image"></param>
        /// <param name="outputStream"></param>
        public static void WritePng8(Image image, Stream outputStream)
        {
            WritePng8(image, outputStream, new PngOptimizationOptions { FilterType = PngFilterType.None, PaletteSize = 255 } );
        }

        /// <summary>
        /// Writes a 8 bit, quantized PNG image to the specified stream.
        /// </summary>
        /// <param name="image">The bitmap.</param>
        /// <param name="outputStream">The output stream.</param>
        /// <param name="options">if set to <c>true</c> [use expensive optimizations].</param>
        public static void WritePng8(Image image, Stream outputStream, PngOptimizationOptions options)
        {
            var quantizer = new ImageManipulation.HextreeQuantizer(options.PaletteSize, 8);
            using (Bitmap quantized = quantizer.Quantize(image))
            {
                if (!options.FilterType.HasValue)
                {
                    var subStream = new ChunkedMemoryStream();
                    WritePng8PreQuantized(quantized, subStream, quantizer.ActualPaletteSize, PngFilterType.Sub);

                    var noneStream = new ChunkedMemoryStream();
                    WritePng8PreQuantized(quantized, noneStream, quantizer.ActualPaletteSize, PngFilterType.None);

                    if (subStream.Length > noneStream.Length)
                    {
                        noneStream.WriteTo(outputStream);
                    }
                    else
                    {
                        subStream.WriteTo(outputStream);
                    }
                }
                else
                {
                    WritePng8PreQuantized(quantized, outputStream, quantizer.ActualPaletteSize, options.FilterType.Value);
                }
            }
        }

        /// <summary>
        /// Write a PNG-8 file to the specified stream.
        /// </summary>
        /// <param name="image">The GDI+ bitmap to write.  This PixelFormat for this image must be 8bppIndexed.
        /// Use the image quantizier in 3rd Party/Microsoft/ImageManipulation to convert 24-bit images down to indexed color.</param>
        /// <param name="outputStream">The stream to write the image to.  As of now, this can be a forward-only stream.</param>
        /// <param name="paletteSize">The number of palette colors to use.  GDI+ doesn't support all of the bit depths
        /// for indexed-color images that PNG supports.  If you set this value, you can reduce the size of the PNG, but any
        /// colors beyond paletteSize will not be shown correctly.</param>
        /// <param name="filterType">Type of the filter.</param>
        internal static void WritePng8PreQuantized(Image image, Stream outputStream, int paletteSize, PngFilterType filterType)
        {
            using (var bmp = new DisposableBitmapWrapper(image))
            {
                var writer = new Png8Writer(bmp.Bitmap, outputStream, paletteSize, filterType);
                writer.Write();
            }
        }

        /// <summary>
        /// Utility to wrap a function argument that is an Image
        /// If it needs to allocate a bitmap, it also destroys it on Dispose.
        /// If not, it doesn't.
        /// </summary>
        private class DisposableBitmapWrapper : IDisposable
        {
            public DisposableBitmapWrapper(Image image)
            {
                this.Bitmap = image as Bitmap;

                if (this.Bitmap == null)
                {
                    _needsDisposing = true;
                    this.Bitmap = new Bitmap(image);
                }
            }

            public Bitmap Bitmap { get; private set; }

            private readonly bool _needsDisposing;

            public void Dispose()
            {
                if (_needsDisposing)
                {
                    this.Bitmap.Dispose();
                }
            }
        }

        /// <summary>
        /// Determines if the specified image contains any transparent or semi-transparent pixels.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns></returns>
        public static bool ImageContainsTransparency(Image image)
        {
            if (Image.IsAlphaPixelFormat(image.PixelFormat))
            {
                return true;
            }

            if (image.PixelFormat == PixelFormat.Format8bppIndexed)
            {
                if ((image.Palette.Flags & 0x00000001) != 0) //contains alpha
                {
                    return true;
                }
            }

            return false;
        }
    }
}
