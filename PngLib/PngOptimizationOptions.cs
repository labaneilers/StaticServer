namespace PngLib
{
    /// <summary>
    /// Describes options for PNG optimization
    /// </summary>
    public class PngOptimizationOptions
    {
        /// <summary>
        /// The number of colors in the palette (1-255). 0 indicates 24 bit (full) color
        /// </summary>
        public int PaletteSize { get; set; }

        /// <summary>
        /// The type of pre-compression filter to use.
        /// If null, the PNG rendering code in PngLib will guess the best option (expensive).
        /// The default is usually PngFilterType.None
        /// </summary>
        public PngFilterType? FilterType { get; set; }
    }
}