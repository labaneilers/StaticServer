﻿
namespace PngLib
{
    /// <summary>
    /// Describes PNG pre-compression filters.
    /// http://en.wikipedia.org/wiki/Portable_Network_Graphics#Filtering
    /// Default is "None".
    /// </summary>
    public enum PngFilterType
    {
        None = 0,
        Sub = 1,
        Up = 2,
        Average = 3,
        Paeth = 4
    }
}
