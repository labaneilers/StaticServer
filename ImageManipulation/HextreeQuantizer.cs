/* 
  THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF 
  ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO 
  THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A 
  PARTICULAR PURPOSE. 
  
    This is sample code and is freely distributable. 
*/
using System;
using System.Collections;
using System.Drawing;
using System.Drawing.Imaging;
using System.Collections.Generic;

namespace ImageManipulation
{
    /// <summary>
    /// Quantize using an Hextree
    /// </summary>
    public unsafe class HextreeQuantizer : Quantizer
    {
        /// <summary>
        /// Construct the Hextree quantizer
        /// </summary>
        /// <remarks>
        /// The Hextree quantizer is a two pass algorithm. The initial pass sets up the Hextree,
        /// the second pass quantizes a color based on the nodes in the tree
        /// </remarks>
        /// <param name="maxColors">The maximum number of colors to return</param>
        /// <param name="maxColorBits">The number of significant bits</param>
        public HextreeQuantizer(int maxColors, int maxColorBits)
            : base(false)
        {
            if (maxColors > 255)
                throw new ArgumentOutOfRangeException("maxColors", maxColors, "The number of colors should be less than 256");

            if ((maxColorBits < 1) | (maxColorBits > 8))
                throw new ArgumentOutOfRangeException("maxColorBits", maxColorBits, "This should be between 1 and 8");

            // Construct the Hextree
            _Hextree = new Hextree(maxColorBits);

            _maxColors = maxColors;
        }

        /// <summary>
        /// Process the pixel in the first pass of the algorithm
        /// </summary>
        /// <param name="pixel">The pixel to quantize</param>
        /// <remarks>
        /// This function need only be overridden if your quantize algorithm needs two passes,
        /// such as an Hextree quantizer.
        /// </remarks>
        protected override void InitialQuantizePixel(Color32* pixel)
        {
            // Add the color to the Hextree
            if (_colors.Count <= _maxColors && !_colors.ContainsKey(pixel->ARGB))
            {
                _colors.Add(pixel->ARGB, true);
            }
            
            _Hextree.AddColor(pixel);
        }

        /// <summary>
        /// Override this to process the pixel in the second pass of the algorithm
        /// </summary>
        /// <param name="pixel">The pixel to quantize</param>
        /// <returns>The quantized value</returns>
        protected override byte QuantizePixel(Color32* pixel)
        {
            return (byte)_Hextree.GetPaletteIndex(pixel);
        }

        /// <summary>
        /// Retrieve the palette for the quantized image
        /// </summary>
        /// <param name="original">Any old palette, this is overrwritten</param>
        /// <returns>The new color palette</returns>
        protected override ColorPalette GetPalette(ColorPalette original)
        {
            // First off convert the Hextree to _maxColors colors
            ArrayList palette = _Hextree.Palletize(_maxColors - 1);

            _maxPaletteIndex = palette.Count;

            // Then convert the palette based on those colors
            for (int index = 0; index < palette.Count; index++)
                original.Entries[index] = (Color)palette[index];

            return original;
        }

        /// <summary>
        /// Stores the tree
        /// </summary>
        private Hextree _Hextree;

        /// <summary>
        /// Maximum allowed color depth
        /// </summary>
        private int _maxColors;

        /// <summary>
        /// Actual size of the quantized palette
        /// </summary>
        private int _maxPaletteIndex = 0;
        public int ActualPaletteSize
        {
            get { return _maxPaletteIndex + 1; }
        }

        private Dictionary<int, bool> _colors = new Dictionary<int, bool>();

        /// <summary>
        /// Did the quantizer drop colors?
        /// The quantizer ignores the LSB, so it may drop colors even if the source
        /// image has less than maxColors
        /// </summary>
        public bool IsLossy
        {
            get
            {
                return ((_colors.Count > _maxColors) || (_colors.Count != this._Hextree.Leaves));
            }
        }

        /// <summary>
        /// Did the original image have more than maxColor colors?
        /// </summary>
        public bool OriginalExceedsMaxColors
        {
            get
            {
                return (_colors.Count > _maxColors);
            }
        }

        /// <summary>
        /// Class which does the actual quantization
        /// </summary>
        private class Hextree
        {
            /// <summary>
            /// Construct the Hextree
            /// </summary>
            /// <param name="maxColorBits">The maximum number of significant bits in the image</param>
            public Hextree(int maxColorBits)
            {
                _maxColorBits = maxColorBits;
                _leafCount = 0;
                _reducibleNodes = new HextreeNode[9];
                _root = new HextreeNode(0, _maxColorBits, this);
                _previousColor = 0;
                _previousNode = null;
            }

            /// <summary>
            /// Add a given color value to the Hextree
            /// </summary>
            /// <param name="pixel"></param>
            public void AddColor(Color32* pixel)
            {
                // Check if this request is for the same color as the last
                if (_previousColor == pixel->ARGB)
                {
                    // If so, check if I have a previous node setup. This will only ocurr if the first color in the image
                    // happens to be black, with an alpha component of zero.
                    if (null == _previousNode)
                    {
                        _previousColor = pixel->ARGB;
                        _root.AddColor(pixel, _maxColorBits, 0, this);
                    }
                    else
                        // Just update the previous node
                        _previousNode.Increment(pixel);
                }
                else
                {
                    _previousColor = pixel->ARGB;
                    _root.AddColor(pixel, _maxColorBits, 0, this);
                }
            }

            /// <summary>
            /// Reduce the depth of the tree
            /// </summary>
            public void Reduce()
            {
                int index;

                // Find the deepest level containing at least one reducible node
                for (index = _maxColorBits - 1; (index > 0) && (null == _reducibleNodes[index]); index--) ;

                // Reduce the node most recently added to the list at level 'index'
                HextreeNode node = _reducibleNodes[index];
                _reducibleNodes[index] = node.NextReducible;

                // Decrement the leaf count after reducing the node
                _leafCount -= node.Reduce();

                // And just in case I've reduced the last color to be added, and the next color to
                // be added is the same, invalidate the previousNode...
                _previousNode = null;
            }

            /// <summary>
            /// Get/Set the number of leaves in the tree
            /// </summary>
            public int Leaves
            {
                get { return _leafCount; }
                set { _leafCount = value; }
            }

            /// <summary>
            /// Return the array of reducible nodes
            /// </summary>
            protected HextreeNode[] ReducibleNodes
            {
                get { return _reducibleNodes; }
            }

            /// <summary>
            /// Keep track of the previous node that was quantized
            /// </summary>
            /// <param name="node">The node last quantized</param>
            protected void TrackPrevious(HextreeNode node)
            {
                _previousNode = node;
            }

            /// <summary>
            /// Convert the nodes in the Hextree to a palette with a maximum of colorCount colors
            /// </summary>
            /// <param name="colorCount">The maximum number of colors</param>
            /// <returns>An arraylist with the palettized colors</returns>
            public ArrayList Palletize(int colorCount)
            {
                while (Leaves > colorCount)
                    Reduce();

                // Now palettize the nodes
                ArrayList palette = new ArrayList(Leaves);
                int paletteIndex = 0;
                _root.ConstructPalette(palette, ref paletteIndex);

                // And return the palette
                return palette;
            }

            /// <summary>
            /// Get the palette index for the passed color
            /// </summary>
            /// <param name="pixel"></param>
            /// <returns></returns>
            public int GetPaletteIndex(Color32* pixel)
            {
                return _root.GetPaletteIndex(pixel, 0);
            }

            /// <summary>
            /// Mask used when getting the appropriate pixels for a given node
            /// </summary>
            private static int[] mask = new int[8] { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };

            /// <summary>
            /// The root of the Hextree
            /// </summary>
            private HextreeNode _root;

            /// <summary>
            /// Number of leaves in the tree
            /// </summary>
            private int _leafCount;

            /// <summary>
            /// Array of reducible nodes
            /// </summary>
            private HextreeNode[] _reducibleNodes;

            /// <summary>
            /// Maximum number of significant bits in the image
            /// </summary>
            private int _maxColorBits;

            /// <summary>
            /// Store the last node quantized
            /// </summary>
            private HextreeNode _previousNode;

            /// <summary>
            /// Cache the previous color quantized
            /// </summary>
            private int _previousColor;

            /// <summary>
            /// Class which encapsulates each node in the tree
            /// </summary>
            protected class HextreeNode
            {
                /// <summary>
                /// Construct the node
                /// </summary>
                /// <param name="level">The level in the tree = 0 - 7</param>
                /// <param name="colorBits">The number of significant color bits in the image</param>
                /// <param name="Hextree">The tree to which this node belongs</param>
                public HextreeNode(int level, int colorBits, Hextree Hextree)
                {
                    // Construct the new node
                    _leaf = (level == colorBits);

                    _red = _green = _blue = _alpha = 0;
                    _pixelCount = 0;

                    // If a leaf, increment the leaf count
                    if (_leaf)
                    {
                        Hextree.Leaves++;
                        _nextReducible = null;
                        _children = null;
                    }
                    else
                    {
                        // Otherwise add this to the reducible nodes
                        _nextReducible = Hextree.ReducibleNodes[level];
                        Hextree.ReducibleNodes[level] = this;
                        _children = new HextreeNode[16];
                    }
                }

                /// <summary>
                /// Add a color into the tree
                /// </summary>
                /// <param name="pixel">The color</param>
                /// <param name="colorBits">The number of significant color bits</param>
                /// <param name="level">The level in the tree</param>
                /// <param name="Hextree">The tree to which this node belongs</param>
                public void AddColor(Color32* pixel, int colorBits, int level, Hextree Hextree)
                {
                    // Update the color information if this is a leaf
                    if (_leaf)
                    {
                        Increment(pixel);
                        // Setup the previous node
                        Hextree.TrackPrevious(this);
                    }
                    else
                    {
                        // Go to the next level down in the tree
                        int shift = 7 - level;
                        int index = ((pixel->Alpha & mask[level]) >> (shift - 3)) |
                                    ((pixel->Red & mask[level]) >> (shift - 2)) |
                                    ((pixel->Green & mask[level]) >> (shift - 1)) |
                                    ((pixel->Blue & mask[level]) >> (shift));

                        HextreeNode child = _children[index];

                        if (null == child)
                        {
                            // Create a new child node & store in the array
                            child = new HextreeNode(level + 1, colorBits, Hextree);
                            _children[index] = child;
                        }

                        // Add the color to the child node
                        child.AddColor(pixel, colorBits, level + 1, Hextree);
                    }

                }

                /// <summary>
                /// Get/Set the next reducible node
                /// </summary>
                public HextreeNode NextReducible
                {
                    get { return _nextReducible; }
                    set { _nextReducible = value; }
                }

                /// <summary>
                /// Return the child nodes
                /// </summary>
                public HextreeNode[] Children
                {
                    get { return _children; }
                }

                /// <summary>
                /// Reduce this node by removing all of its children
                /// </summary>
                /// <returns>The number of leaves removed</returns>
                public int Reduce()
                {
                    _red = _green = _blue = _alpha = 0;
                    int children = 0;

                    // Loop through all children and add their information to this node
                    for (int index = 0; index < 16; index++)
                    {
                        if (null != _children[index])
                        {
                            _red += _children[index]._red;
                            _green += _children[index]._green;
                            _blue += _children[index]._blue;
                            _alpha += _children[index]._alpha;
                            _pixelCount += _children[index]._pixelCount;
                            ++children;
                            _children[index] = null;
                        }
                    }

                    // Now change this to a leaf node
                    _leaf = true;

                    // Return the number of nodes to decrement the leaf count by
                    return (children - 1);
                }

                /// <summary>
                /// Traverse the tree, building up the color palette
                /// </summary>
                /// <param name="palette">The palette</param>
                /// <param name="paletteIndex">The current palette index</param>
                public void ConstructPalette(ArrayList palette, ref int paletteIndex)
                {
                    if (_leaf)
                    {
                        // Consume the next palette index
                        _paletteIndex = paletteIndex++;

                        // And set the color of the palette entry
                        palette.Add(Color.FromArgb(_alpha / _pixelCount, _red / _pixelCount, _green / _pixelCount, _blue / _pixelCount));
                    }
                    else
                    {
                        // Loop through children looking for leaves
                        for (int index = 0; index < 16; index++)
                        {
                            if (null != _children[index])
                                _children[index].ConstructPalette(palette, ref paletteIndex);
                        }
                    }
                }

                /// <summary>
                /// Return the palette index for the passed color
                /// </summary>
                public int GetPaletteIndex(Color32* pixel, int level)
                {
                    int paletteIndex = _paletteIndex;

                    if (!_leaf)
                    {
                        int shift = 7 - level;
                        int index = ((pixel->Alpha & mask[level]) >> (shift - 3)) |
                                    ((pixel->Red & mask[level]) >> (shift - 2)) |
                                    ((pixel->Green & mask[level]) >> (shift - 1)) |
                                    ((pixel->Blue & mask[level]) >> (shift));

                        if (null != _children[index])
                            paletteIndex = _children[index].GetPaletteIndex(pixel, level + 1);
                        else
                            throw new Exception("Didn't expect this!");
                    }

                    return paletteIndex;
                }

                /// <summary>
                /// Increment the pixel count and add to the color information
                /// </summary>
                public void Increment(Color32* pixel)
                {
                    _pixelCount++;
                    _red += pixel->Red;
                    _green += pixel->Green;
                    _blue += pixel->Blue;
                    _alpha += pixel->Alpha;
                }

                /// <summary>
                /// Flag indicating that this is a leaf node
                /// </summary>
                private bool _leaf;

                /// <summary>
                /// Number of pixels in this node
                /// </summary>
                private int _pixelCount;

                /// <summary>
                /// Red component
                /// </summary>
                private int _red;

                /// <summary>
                /// Green Component
                /// </summary>
                private int _green;

                /// <summary>
                /// Blue component
                /// </summary>
                private int _blue;

                /// <summary>
                /// Alpha Component
                /// </summary>
                private int _alpha;

                /// <summary>
                /// Pointers to any child nodes
                /// </summary>
                private HextreeNode[] _children;

                /// <summary>
                /// Pointer to next reducible node
                /// </summary>
                private HextreeNode _nextReducible;

                /// <summary>
                /// The index of this node in the palette
                /// </summary>
                private int _paletteIndex;

            }
        }
    }
}
