﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Web;
using StaticWww.Models;

namespace MVC5.Models
{
    public class ImageRenderer
    {
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

        public void WriteImage(ResponsiveImageQueryString qs, Stream stream)
        {
            string physicalPath = this.MapPath(qs.Src);

            bool isPng = qs.PngColors >= 2 && qs.PngColors <= 255;
            bool isJpeg = qs.JpegQuality > 0 && qs.JpegQuality <= 100;
            var destRect = new Rectangle(0, 0, qs.Width, qs.Height);

            // Determine the natural size of the image
            using (var sourceImage = Image.FromFile(physicalPath))
            {
                var naturalSize = sourceImage.Size;

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
                        VP.VPSystem.Drawing.ImageWriter.WritePng(outputBitmap, stream, new VP.VPSystem.Drawing.PngOptimizationOptions() { PaletteSize = qs.PngColors });
                        //outputBitmap.Save(stream, ImageFormat.Png);
                    }
                    else
                    {
                        ImageCodecInfo jgpEncoder = GetEncoder(ImageFormat.Jpeg);
                        var encoderParameters = new EncoderParameters(1);
                        var encoderParameter = new EncoderParameter(Encoder.Quality, qs.JpegQuality);
                        encoderParameters.Param[0] = encoderParameter;

                        outputBitmap.Save(stream, jgpEncoder, encoderParameters);
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