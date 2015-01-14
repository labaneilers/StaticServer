using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Drawing.Imaging;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using VP.VPSystem.IO;


// http://libpng.nigilist.ru/pub/png/spec/1.2/PNG-Contents.html

namespace VP.VPSystem.Drawing
{
    internal abstract class PngWriter
    {
        protected BinaryWriter _writer;
        protected Bitmap _bitmap;
        protected int _paletteBitDepth;
        internal ColorType _colorType;
        internal PngFilterType _filterType;

        internal enum ColorType
        {
            GrayScale = 0,
            RGB = 2,
            Palette = 3,
            GrayscaleAlpha = 4,
            RGBA = 6
        }

        internal PngWriter(Bitmap bitmap, Stream outputStream, ColorType colorType, PngFilterType filterType)
        {
            _bitmap = bitmap;
            _writer = new BinaryWriter(outputStream);
            _colorType = colorType;
            _filterType = filterType;

            switch (filterType)
            {
                case PngFilterType.None:
                case PngFilterType.Sub:
                    break;
                default:
                    throw new System.Exception(String.Format("Png24Writer: Unsupported filter {0}", filterType));
            }
        }

        internal void Write()
        {
            WriteSignature();
            WriteHeader();
            WritePalette();
            WriteTransparency();
            WriteBitmapData();
            WriteFooter();

            _writer.Flush();
        }

        private void WriteBitmapData()
        {
            var memoryStream = new ChunkedMemoryStream();
            var defl = new ICSharpCode.SharpZipLib.Zip.Compression.Deflater(5);
            var compressionStream = new DeflaterOutputStream(memoryStream, defl);
            WriteBitmapData(compressionStream);
            compressionStream.Finish();

            // unrolled writedatachunk ...
            uint length = (uint)memoryStream.Length;
            var chunkType = CHUNK_TYPE_DATA;
            _writer.Write(ToBytes(length));
            _writer.Write(chunkType);
            if (length > 0)
            {
                memoryStream.WriteTo(_writer.BaseStream);
            }
            _writer.Write(ToBytes(PngCrc.ComputeCrc(chunkType, memoryStream)));
        }

        unsafe protected void WriteScanline(DeflaterOutputStream compressionStream, byte[] scanline, int bytesPerPixel)
        {
            int len = scanline.Length;
            
            // filter
            if (_filterType == PngFilterType.Sub)
            {
                fixed (byte* bmp = scanline)
                {
                    for (int i = len - 1; i > bytesPerPixel - 1; i -= bytesPerPixel)
                    {
                        //Sub(x) = Raw(x) - Raw(x-bpp)
                        for (int j = 0; j < bytesPerPixel; j++)
                        {
                            bmp[i - j] -= bmp[i - j - bytesPerPixel];
                        }
                    }
                }
            }

            compressionStream.WriteByte((byte)_filterType);
            compressionStream.Write(scanline, 0, len);
        }

        protected abstract void WriteBitmapData(DeflaterOutputStream compressionStream);

        protected static byte[] CHUNK_TYPE_PALETTE = ASCIIEncoding.ASCII.GetBytes("PLTE");
        protected virtual void WritePalette() { }

        protected static byte[] CHUNK_TYPE_TRANSPARENCY = ASCIIEncoding.ASCII.GetBytes("tRNS");        
        protected virtual void WriteTransparency() { }

        private static byte[] SIGNATURE = { 137, 80, 78, 71, 13, 10, 26, 10 };
        private void WriteSignature()
        {
            _writer.Write(SIGNATURE);
        }

        private static byte[] CHUNK_TYPE_FOOTER = ASCIIEncoding.ASCII.GetBytes("IEND");
        private static byte[] FOOTER_DATA = { };
        private void WriteFooter()
        {
            WriteChunk(CHUNK_TYPE_FOOTER, FOOTER_DATA);
        }

        private static byte[] CHUNK_TYPE_HEADER = ASCIIEncoding.ASCII.GetBytes("IHDR");
        private void WriteHeader()
        {
            MemoryStream header = new MemoryStream(13);
            header.Write(ToBytes(_bitmap.Width), 0, 4);
            header.Write(ToBytes(_bitmap.Height), 0, 4);
            header.WriteByte((byte)_paletteBitDepth); // bit depth
            header.WriteByte((byte)_colorType); 
            header.WriteByte(0); // Compression; must be zero
            header.WriteByte(0); // Interlace method
            WriteChunk(CHUNK_TYPE_HEADER, header.GetBuffer());
        }

        private static byte[] CHUNK_TYPE_DATA = ASCIIEncoding.ASCII.GetBytes("IDAT");
        protected void WriteDataChunk(byte[] data, int length)
        {
            WriteChunk(CHUNK_TYPE_DATA, data, length);
        }

        protected void WriteChunk(byte[] chunkType, byte[] data)
        {
            WriteChunk(chunkType, data, data.Length);
        }

        private void WriteChunk(byte[] chunkType, byte[] data, int length)
        {
            _writer.Write(ToBytes((uint)length));
            _writer.Write(chunkType);
            if (length > 0)
            {
                _writer.Write(data, 0, length);
            }
            _writer.Write(ToBytes(PngCrc.ComputeCrc(chunkType, data, length)));
        }

        private static byte[] ToBytes(uint i)
        {
            byte[] bytes = BitConverter.GetBytes(i);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }

        private static byte[] ToBytes(int i)
        {
            byte[] bytes = BitConverter.GetBytes(i);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return bytes;
        }

    }
}
