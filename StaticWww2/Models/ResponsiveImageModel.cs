using System.Drawing;

namespace StaticWww.Models
{
    /// <summary>
    /// Encodes/decodes querystrings for the ResponsiveImage renderer.
    /// </summary>
    public class ResponsiveImageModel
    {
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
        /// The JPEG quality (0 if is a PNG)
        /// </summary>
        public int JpegQuality { get; set; }

        /// <summary>
        /// The number of colors in the PNG palette. 0 if JPEG.
        /// </summary>
        public int PngColors { get; set; }
    }
}
