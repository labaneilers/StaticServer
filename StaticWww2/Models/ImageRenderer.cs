using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Web;
//using FreeImageAPI;

namespace StaticWww2.Models
{
    public class ImageRenderer
    {
        public bool UseFreeImage { private get; set; }

        public void SetResponseHeaders(HttpContextBase context, bool isPng, bool cacheable)
        {
            context.Response.ContentType = isPng ? "image/png" : "image/jpeg";
            context.Response.Cache.SetCacheability(cacheable ? HttpCacheability.Public : HttpCacheability.NoCache);

            if (cacheable)
            {
                context.Response.Cache.SetMaxAge(new TimeSpan(364, 0, 0, 0));
                context.Response.Cache.SetLastModified(DateTime.Now.AddDays(-364));
            }
        }

        public Func<string, string> MapPath { private get; set; }

        public void WriteImage(ResponsiveImageModel qs, Stream stream)
        {
            string physicalPath = this.MapPath(qs.Src);

            bool isPng = qs.PngColors >= 2 && qs.PngColors <= 255;
            bool isJpeg = qs.JpegQuality > 0 && qs.JpegQuality <= 100;
            
            // Determine the natural size of the image
            using (var sourceImage = Image.FromFile(physicalPath))
            {
                var naturalSize = sourceImage.Size;

                if (qs.Width == 0)
                {
                    qs.Width = naturalSize.Width;
                }
                if (qs.Height == 0)
                {
                    qs.Height = naturalSize.Height;
                }

                var destRect = new Rectangle(0, 0, qs.Width, qs.Height);

                using (var outputBitmap = new Bitmap(qs.Width, qs.Height, PixelFormat.Format32bppArgb))
                using (var graphics = Graphics.FromImage(outputBitmap))
                {
                    // Use bicubic resampling because it looks silky smooth for text and other high-contrast details.
                    // We are willing to use more expensive operations here because this image will be rendered once and
                    // then cached in the CDN.
                    graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    graphics.SmoothingMode = SmoothingMode.HighQuality;

                    // If we're rendering as a JPEG, we can't support alpha transparency.
                    // Set the background to white.
                    if (!isPng)
                    {
                        graphics.Clear(Color.White);
                    }

                    using (var imageAttributes = new ImageAttributes())
                    {
                        // WrapMode.TileFlipXY prevents edges from resampling against white neighbor pixels.
                        // This prevents a halo from appearing around edges.
                        // http://stackoverflow.com/questions/1890605/ghost-borders-ringing-when-resizing-in-gdi
                        imageAttributes.SetWrapMode(WrapMode.TileFlipXY);

                        graphics.DrawImage(
                            sourceImage,
                            destRect,
                            0,
                            0,
                            naturalSize.Width,
                            naturalSize.Height,
                            GraphicsUnit.Pixel,
                            imageAttributes);
                    }

                    if (isPng)
                    {
                        PngLib.PngWriter.WritePng(outputBitmap, stream, new PngLib.PngOptimizationOptions { PaletteSize = qs.PngColors });
                    }
                    else if (isJpeg)
                    {
//                        if (this.UseFreeImage)
//                        {
//                            // Use FreeImage.NET to encode jpegs
//                            // This invokes unmanaged code, but has been used in production already, so is less risky.
//
//                            // Turn off chroma subsampling (set ratio 4:4:4)
//                            // This is the magic flag that makes flat color areas look good.
//                            // Without it, you get compression artifacts that look especially bad in flat color areas and text.
//                            var flags = FREE_IMAGE_SAVE_FLAGS.JPEG_SUBSAMPLING_444;
//
//                            // Not sure why you wouldn't want to optimize, but this reduces the size significantly without
//                            // any noticable visual impact.
//                            flags |= FREE_IMAGE_SAVE_FLAGS.JPEG_PROGRESSIVE;
//
//                            // This seems a little bizarre, but you can put an arbitrary value from 1-100
//                            // for JPEG quality in this bitmask, because all of the enum values are above 128.
//                            // http://forum.openframeworks.cc/t/saveimage-file-parameters/381/2
//                            flags |= (FREE_IMAGE_SAVE_FLAGS)qs.JpegQuality;
//
//                            var img = new FreeImageBitmap(outputBitmap);
//                            img.Save(stream, FREE_IMAGE_FORMAT.FIF_JPEG, flags);
//                        }
//                        else
//                        {
                            ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);
                            var encoderParameters = new EncoderParameters(1);
                            var encoderParameter = new EncoderParameter(Encoder.Quality, qs.JpegQuality);
                            encoderParameters.Param[0] = encoderParameter;

                            outputBitmap.Save(stream, jgpEncoder, encoderParameters);
//                        }
                    }
                    else
                    {
                        throw new Exception("No image format specified");
                    }
                }
            }
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {

            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();

            foreach (ImageCodecInfo codec in codecs)
            {
                if (codec.FormatID == format.Guid)
                {
                    return codec;
                }
            }
            return null;
        }
    }
}