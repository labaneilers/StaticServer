using System.IO;

namespace PngLib
{
    /// <summary>
    /// Class for computing CRC's of Png chunks.
    /// This was modelled after an example in the Png specification.
    /// </summary>
    internal class PngCrc
    {
        internal static uint ComputeCrc(params byte[][] data)
        {
            uint crc = uint.MaxValue;
            foreach (byte[] dataPart in data)
            {
                crc = UpdateCrc(crc, dataPart, dataPart.Length);
            }
            return crc ^ uint.MaxValue;
        }

        internal static uint ComputeCrc(byte[] chunkType, byte[] data, int dataLength)
        {
            uint crc = uint.MaxValue;
            crc = UpdateCrc(crc, chunkType, chunkType.Length);
            crc = UpdateCrc(crc, data, dataLength);
            return crc ^ uint.MaxValue;
        }

        internal static uint ComputeCrc(byte[] chunkType, Stream data)
        {
            uint crc = uint.MaxValue;
            crc = UpdateCrc(crc, chunkType, chunkType.Length);

            data.Seek(0, SeekOrigin.Begin);
            int read;
            var buffer = new byte[4096];
            while ((read = data.Read(buffer, 0, buffer.Length)) > 0)
            {
                crc = UpdateCrc(crc, buffer, read);
            }

            return crc ^ uint.MaxValue;
        }

        private static uint UpdateCrc(uint crc, byte[] data, int len)
        {
            uint c = crc;

            for (int n = 0; n < len; n++)
            {
                c = _crcTable[(c ^ data[n]) & 0xff] ^ (c >> 8);
            }
            return c;
        }

        static PngCrc()
        {
            InitializeCrcTable();
        }

        static uint[] _crcTable = new uint[256];
        static void InitializeCrcTable()
        {
            uint c;

            for (uint n = 0; n < 256; n++)
            {
                c = n;
                for (uint k = 0; k < 8; k++)
                {
                    if ((c & 1) != 0)
                    {
                        c = 0xedb88320 ^ (c >> 1);
                    }
                    else
                    {
                        c = c >> 1;
                    }
                }
                _crcTable[n] = c;
            }
        }
    }
}