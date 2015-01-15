using System;
using System.IO;

namespace PngLib
{
    public class ChunkedMemoryStream : Stream
    {
        private MemoryChunk _chunks;
        private MemoryChunk _readChunk;
        private int _readOffset;
        private MemoryChunk _writeChunk;
        private int _writeOffset;

        public ChunkedMemoryStream() { }

        private MemoryChunk AllocateMemoryChunk()
        {
            var chunk = new MemoryChunk
            {
                Buffer = new byte[64 * 1024],
                Next = null
            };
            return chunk;
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                // allow us to get array
                //_closed = true;
                //_chunks = null;
                //_writeChunk = null;
                //_readChunk = null;
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        public override void Flush() { }

        public override int Read(byte[] buffer, int offset, int count)
        {
            CheckDisposed();
            if (_readChunk == null)
            {
                if (_chunks == null)
                {
                    return 0;
                }
                _readChunk = _chunks;
                _readOffset = 0;
            }
            var src = _readChunk.Buffer;
            var length = src.Length;
            if (_readChunk.Next == null)
            {
                length = _writeOffset;
            }
            int num2 = 0;
            while (count > 0)
            {
                if (_readOffset == length)
                {
                    if (_readChunk.Next == null)
                    {
                        return num2;
                    }
                    _readChunk = _readChunk.Next;
                    _readOffset = 0;
                    src = _readChunk.Buffer;
                    length = src.Length;
                    if (_readChunk.Next == null)
                    {
                        length = _writeOffset;
                    }
                }
                int num3 = Math.Min(count, length - _readOffset);
                Buffer.BlockCopy(src, _readOffset, buffer, offset, num3);
                offset += num3;
                count -= num3;
                _readOffset += num3;
                num2 += num3;
            }
            return num2;
        }

        private void CheckDisposed()
        {
            //if (_closed)
            //{
            //    throw new ObjectDisposedException("ChunkedMemoryStream");
            //}
        }

        public override int ReadByte()
        {
            CheckDisposed();
            if (_readChunk == null)
            {
                if (_chunks == null)
                {
                    return 0;
                }
                _readChunk = _chunks;
                _readOffset = 0;
            }
            byte[] buffer = _readChunk.Buffer;
            int length = buffer.Length;
            if (_readChunk.Next == null)
            {
                length = _writeOffset;
            }
            if (_readOffset == length)
            {
                if (_readChunk.Next == null)
                {
                    return -1;
                }
                _readChunk = _readChunk.Next;
                _readOffset = 0;
                buffer = _readChunk.Buffer;
                length = buffer.Length;
                if (_readChunk.Next == null)
                {
                    length = _writeOffset;
                }
            }
            return buffer[_readOffset++];
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            CheckDisposed();
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;

                case SeekOrigin.Current:
                    Position += offset;
                    break;

                case SeekOrigin.End:
                    Position = Length + offset;
                    break;
            }
            return Position;
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public virtual byte[] ToArray()
        {
            int length = (int)Length;
            var buffer = new byte[Length];
            var chunk = _readChunk;
            int num2 = _readOffset;
            _readChunk = _chunks;
            _readOffset = 0;
            Read(buffer, 0, length);
            _readChunk = chunk;
            _readOffset = num2;
            return buffer;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            CheckDisposed();
            if (_chunks == null)
            {
                _chunks = AllocateMemoryChunk();
                _writeChunk = _chunks;
                _writeOffset = 0;
            }
            byte[] dst = _writeChunk.Buffer;
            int length = dst.Length;
            while (count > 0)
            {
                if (_writeOffset == length)
                {
                    _writeChunk.Next = AllocateMemoryChunk();
                    _writeChunk = _writeChunk.Next;
                    _writeOffset = 0;
                    dst = _writeChunk.Buffer;
                    length = dst.Length;
                }
                int num2 = Math.Min(count, length - _writeOffset);
                Buffer.BlockCopy(buffer, offset, dst, _writeOffset, num2);
                offset += num2;
                count -= num2;
                _writeOffset += num2;
            }
        }

        public override void WriteByte(byte value)
        {
            CheckDisposed();
            if (_chunks == null)
            {
                _chunks = AllocateMemoryChunk();
                _writeChunk = _chunks;
                _writeOffset = 0;
            }
            byte[] buffer = _writeChunk.Buffer;
            int length = buffer.Length;
            if (_writeOffset == length)
            {
                _writeChunk.Next = AllocateMemoryChunk();
                _writeChunk = _writeChunk.Next;
                _writeOffset = 0;
                buffer = _writeChunk.Buffer;
                length = buffer.Length;
            }
            buffer[_writeOffset++] = value;
        }

        public virtual void WriteTo(Stream stream)
        {
            CheckDisposed();
            if (stream == null)
            {
                throw new ArgumentNullException("stream");
            }
            if (_readChunk == null)
            {
                if (_chunks == null)
                {
                    return;
                }
                _readChunk = _chunks;
                _readOffset = 0;
            }
            byte[] buffer = _readChunk.Buffer;
            int length = buffer.Length;
            if (_readChunk.Next == null)
            {
                length = _writeOffset;
            }
            while (true)
            {
                if (_readOffset == length)
                {
                    if (_readChunk.Next == null)
                    {
                        return;
                    }
                    _readChunk = _readChunk.Next;
                    _readOffset = 0;
                    buffer = _readChunk.Buffer;
                    length = buffer.Length;
                    if (_readChunk.Next == null)
                    {
                        length = _writeOffset;
                    }
                }
                int count = length - _readOffset;
                stream.Write(buffer, _readOffset, count);
                _readOffset = length;
            }
        }

        public override bool CanRead { get { return true; } }
        public override bool CanSeek { get { return true; } }
        public override bool CanWrite { get { return true; } }

        public override long Length
        {
            get
            {
                MemoryChunk next;
                CheckDisposed();
                int num = 0;
                for (MemoryChunk chunk = _chunks; chunk != null; chunk = next)
                {
                    next = chunk.Next;
                    if (next != null)
                    {
                        num += chunk.Buffer.Length;
                    }
                    else
                    {
                        num += _writeOffset;
                    }
                }
                return (long)num;
            }
        }

        public override long Position
        {
            get
            {
                CheckDisposed();
                if (_readChunk == null)
                {
                    return 0L;
                }
                int num = 0;
                for (MemoryChunk chunk = _chunks; chunk != _readChunk; chunk = chunk.Next)
                {
                    num += chunk.Buffer.Length;
                }
                num += _readOffset;
                return (long)num;
            }
            set
            {
                CheckDisposed();
                if (value < 0L)
                {
                    throw new ArgumentOutOfRangeException("value");
                }
                var chunk = _readChunk;
                int num = _readOffset;
                _readChunk = null;
                _readOffset = 0;
                int num2 = (int)value;
                for (var chunk2 = _chunks; chunk2 != null; chunk2 = chunk2.Next)
                {
                    if ((num2 < chunk2.Buffer.Length) || ((num2 == chunk2.Buffer.Length) && (chunk2.Next == null)))
                    {
                        _readChunk = chunk2;
                        _readOffset = num2;
                        break;
                    }
                    num2 -= chunk2.Buffer.Length;
                }
                if (_readChunk == null)
                {
                    _readChunk = chunk;
                    _readOffset = num;

                    // JPEG writer sets stream position to 0 before it starts to write...
                    if (value != 0)
                    {

                        throw new ArgumentOutOfRangeException("value");
                    }
                }
            }
        }

        private class MemoryChunk
        {
            public byte[] Buffer;
            public MemoryChunk Next;
        }
    }
}