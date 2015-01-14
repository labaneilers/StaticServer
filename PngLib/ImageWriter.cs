using System;
using System.Drawing;
using System.IO;
using System.Drawing.Imaging;
using VP.VPSystem.IO;

namespace VP.VPSystem.Drawing
{
    public static class ImageWriter
    {
		/// <summary>
		/// Write out the Bitmap to the specified stream as either a png or jpeg (png only if transparency is
		/// required)
		/// </summary>
        /// <param name="image"></param>
		/// <param name="outputStream"></param>
		/// <returns></returns>
		public static ImageFormat WriteInPngOrJpeg(Image image, Stream outputStream)
		{
            if (ImageContainsTransparency(image))
			{
                WritePng(image, outputStream);
				return ImageFormat.Png;
			}

            WriteJpeg(image, outputStream, 85);
			return ImageFormat.Jpeg;
		}

        /// <summary>
        /// Writes the in best format.
        /// </summary>
        /// <param name="image">The bitmap.</param>
        /// <param name="outputStream">The output stream.</param>
        /// <returns></returns>
        public static ImageFormat WriteInBestFormat(Image image, Stream outputStream)
        {
            return WriteInBestFormat(image, outputStream, false);
        }

        /// <summary>
        /// Write out the Bitmap to the specified stream in the smallest format
        /// </summary>
        /// <param name="image">Input</param>
        /// <param name="outputStream">Output</param>
        /// <param name="useExpensiveOptimizations">if set to <c>true</c> [use expensive optimizations].</param>
        /// <returns>ImageFormat of the data</returns>
        public static ImageFormat WriteInBestFormat(Image image, Stream outputStream, bool useExpensiveOptimizations)
        {
            //Image the image contains transparency, it limits the output options
            //to PNG32 or PNG8
            if (ImageContainsTransparency(image))
            {
                if (Image.GetPixelFormatSize(image.PixelFormat) > 8)
                {
                    var options = new PngOptimizationOptions();
                    if (!useExpensiveOptimizations)
                    {
                        options.FilterType = PngFilterType.None;
                    }

                    WritePng(image, outputStream, options);
                    return ImageFormat.Png;
                }

                WritePng8PreQuantized(image, outputStream, image.Palette.Entries.Length, PngFilterType.Sub);
                return ImageFormat.Png;
            }

            // bake-off
            // PNG8 (only if number of colors is small)
            // PNG24, using 'SUB' filter
            // JPEG
            var quantizer = new ImageManipulation.HextreeQuantizer(255, 8);
            using (Bitmap quantized = quantizer.Quantize(image))
            {
                if (!quantizer.OriginalExceedsMaxColors)
                {
                    // PNG8 is the winner
                    // try SUB filter vs not
                    var png8SubStream = new ChunkedMemoryStream();
                    WritePng8PreQuantized(quantized, png8SubStream, quantizer.ActualPaletteSize, PngFilterType.Sub);

                    var png8NoneStream = new ChunkedMemoryStream();
                    WritePng8PreQuantized(quantized, png8NoneStream, quantizer.ActualPaletteSize, PngFilterType.None);

                    if (png8SubStream.Length < png8NoneStream.Length)
                    {
                        png8SubStream.WriteTo(outputStream);
                    }
                    else
                    {
                        png8NoneStream.WriteTo(outputStream);
                    }

                    return ImageFormat.Png;
                }

                const int PNG_FAVOR_TOLERANCE = 1024;

                // PNG24.Sub vs PNG24.None vs JPEG
                // try SUB filter vs not
                var pngStream = new ChunkedMemoryStream();
                WritePng(image, pngStream, new PngOptimizationOptions());

                var jpegStream = new ChunkedMemoryStream();
                WriteJpeg(image, jpegStream, 85);

                long jpegLen = PNG_FAVOR_TOLERANCE + jpegStream.Length;

                if (pngStream.Length <= jpegLen)
                {
                    var gifStream = new ChunkedMemoryStream();
                    WriteGif(image, gifStream);

                    if (gifStream.Length <= pngStream.Length)
                    {
                        gifStream.WriteTo(outputStream);
                        return ImageFormat.Gif;
                    }
                    else
                    {
                        pngStream.WriteTo(outputStream);
                        return ImageFormat.Png;
                    }
                }

                jpegStream.WriteTo(outputStream);
                return ImageFormat.Jpeg;
            }
        }

        /// <summary>
        /// Gets the appropriate file extension to use for the specified ImageFormat
        /// </summary>
        /// <param name="format"></param>
        /// <returns></returns>
        public static string GetFileExtensionFromFormat(ImageFormat format)
        {
            return format.ToString().ToLower();
        }

        /// <summary>
        /// Gets the format from file extension.
        /// </summary>
        /// <param name="format">The format.</param>
        /// <returns></returns>
        public static ImageFormat GetFormatFromFileExtension(string format)
        {
            format = format.Replace(".", "");

            switch (format.ToLower())
            {
                case "png":
                    return ImageFormat.Png;
                case "jpeg":
                case "jpg":
                    return ImageFormat.Jpeg;
                case "gif":
                    return ImageFormat.Gif;
                default:
                    return null;
            }
        }

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

        private static ImageCodecInfo _jpegCodec = null;

        private static ImageCodecInfo JpegCodec
        {
            get
            {
                if (_jpegCodec == null)
                {
                    foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageEncoders())
                    {
                        if (codec.FormatDescription == "JPEG")
                        {
                            _jpegCodec = codec;
                            break;
                        }
                    }
                }

                return _jpegCodec;
            }
        }

        /// <summary>
        /// Writes a JPEG image to the specified stream
        /// </summary>
        /// <param name="image"></param>
        /// <param name="outputStream"></param>
        /// <param name="quality"></param>
        public static void WriteJpeg(Image image, Stream outputStream, int quality)
        {
            using (var encoderParameters = new EncoderParameters(1))
            {
                encoderParameters.Param[0] = new EncoderParameter(Encoder.Quality, quality);
                image.Save(outputStream, JpegCodec, encoderParameters);
            }
        }

        /// <summary>
        /// Writes a quantized GIF image to the specified stream
        /// </summary>
        /// <param name="image"></param>
        /// <param name="outputStream"></param>
        public static void WriteGif(Image image, Stream outputStream)
        {
            ImageManipulation.Quantizer quantizer = new ImageManipulation.OctreeQuantizer(255, 8);
            using (Bitmap quantized = quantizer.Quantize(image))
            {
                quantized.Save(outputStream, ImageFormat.Gif);
            }
        }

        /// <summary>
        /// Writes a quantized GIF image to the specified stream
        /// </summary>
        /// <param name="image"></param>
        /// <param name="outputStream"></param>
        private static void WriteGifPreQuantized(Image image, Stream outputStream)
        {
            image.Save(outputStream, ImageFormat.Gif);
        }

        /// <summary>
        /// Determines if the specified image contains any semi-transparent pixels.
        /// This would indicate the image cannot be formatted as a GIF.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <returns></returns>
        public static bool ImageContainsSemiTransparency(Image image)
        {
            var bmp = image as Bitmap;

            bool needsDisposing = false;
            if (bmp == null)
            {
                bmp = new Bitmap(image);
                needsDisposing = true;
            }

            try
            {
                //TODO reimplement using lockbits for performance
                for (var x = 0; x < bmp.Width; x++)
                {
                    for (var y = 0; y < bmp.Height; y++)
                    {
                        byte alpha = bmp.GetPixel(x, y).A;
                        if (alpha > 0 && alpha < 255)
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
            finally
            {
                if (needsDisposing)
                {
                    bmp.Dispose();
                }
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

            private bool _needsDisposing;

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
