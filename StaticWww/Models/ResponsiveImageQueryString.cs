using System;
using System.Collections.Specialized;
using System.Drawing;
using System.Web;
using StaticWww.Helpers;

namespace StaticWww.Models
{
    /// <summary>
    /// Encodes/decodes querystrings for the ResponsiveImage renderer.
    /// </summary>
    public class ResponsiveImageQueryString
    {
        private const int MAX_IMAGE_SIZE = 4096;

        private const int SALT = 4934902;

        /// <summary>
        /// The virtual path of the image to render
        /// </summary>
        public string Src { get; set; }

        /// <summary>
        /// The width of the image in pixels
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// The height of the image in pixels
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Crop values (DEPRECATED)
        /// </summary>
        public Rectangle Crop { get; set; }

        /// <summary>
        /// The language of the requested image
        /// </summary>
        public System.Globalization.CultureInfo CultureInfo { get; set; }

        /// <summary>
        /// The JPEG quality (0 if is a PNG)
        /// </summary>
        public int JpegQuality { get; set; }

        /// <summary>
        /// The number of colors in the PNG palette. 0 if JPEG.
        /// </summary>
        public int PngColors { get; set; }

        /// <summary>
        /// Indicates the caller is IE6, which has limitations on the types of PNG images it can accept.
        /// </summary>
        public bool IE6Mode { get; set; }

        /// <summary>
        /// Indicates that the actual hashcode in the querystring doesn't match the image on disk.
        /// </summary>
        public bool IsHashcodeMismatch { get; set; }

//        private readonly IStaticFileUrlProcessor _staticFileUrlProcessor;

		public ResponsiveImageQueryString(/* IStaticFileUrlProcessor staticFileUrlProcessor */)
        {
//            _staticFileUrlProcessor = staticFileUrlProcessor;
        }

        public ResponsiveImageQueryString(NameValueCollection queryString /* , IStaticFileUrlProcessor staticFileUrlProcessor*/)
            //: this(staticFileUrlProcessor)
        {
            this.Src = queryString.Get<string>("s", null);
			//this.CultureInfo = System.Globalization.CultureInfo(queryString.Get<string>("lang", System.Globalization.CultureInfo.GetCultureInfo("en-us")));
            this.Width = queryString.Get("w", 0);
            this.Height = queryString.Get("h", 0);
            this.Crop = new Rectangle(
                queryString.Get("cl", 0),
                queryString.Get("ct", 0),
                queryString.Get("cw", 0),
                queryString.Get("ch", 0)
            );
            this.JpegQuality = queryString.Get("q", 0);
            this.PngColors = queryString.Get("c", -1);
            this.IE6Mode = queryString.Get("ie6", false);

//            StaticFileVersionId calculatedHashCode = _staticFileUrlProcessor
//                .ResolveFromTranslatedPath(this.Src, this.LanguageId).VersionId
//                .Combine(this.Width, 
//                    this.Height, 
//                    this.Crop.Left, 
//                    this.Crop.Top, 
//                    this.Crop.Width, 
//                    this.Crop.Height, 
//                    this.LanguageId.GetHashCode(), 
//                    this.JpegQuality, 
//                    this.PngColors, 
//                    SALT);

//            if (StaticFileVersionId.Parse(queryString.Get("hc")) != calculatedHashCode)
//            {
//                this.IsHashcodeMismatch = true;
//            }

            if (string.IsNullOrWhiteSpace(this.Src))
            {
                throw new Exception("p parameter is required");
            }

//			if (this.CultureInfo == null)
//            {
//                throw new Exception("lang parameter is required");
//            }

            if (this.Width <= 0)
            {
                throw new Exception("w parameter is required. This is likely because the image doesn't exist: " + this.Src);
            }

            if (this.Width > MAX_IMAGE_SIZE)
            {
                throw new Exception("w parameter is " + this.Width + ", which larger than the maximum of " + MAX_IMAGE_SIZE + ". " + this.Src);
            }

            if (this.Height <= 0)
            {
                throw new Exception("h parameter is required. This is likely because the image doesn't exist: " + this.Src);
            }

            if (this.Height > MAX_IMAGE_SIZE)
            {
                throw new Exception("h parameter is " + this.Width + ", which larger than the maximum of " + MAX_IMAGE_SIZE + ". " + this.Src);
            }

            if (this.PngColors < 0 || this.PngColors > 255)
            {
                throw new Exception("c parameter should be in range 2-255");
            }

            if (this.JpegQuality < 0 || this.JpegQuality > 100)
            {
                throw new Exception("c parameter should be in range 1-100");
            }
        }

        public ResponsiveImageQueryString(string queryString /*, IStaticFileUrlProcessor staticFileUrlProcessor */)
            : this(HttpUtility.ParseQueryString(queryString) /* , staticFileUrlProcessor */)
        {
        }
    }
}
