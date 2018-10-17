using System;
using System.IO;

namespace Freedom.Diagnostics
{
    // Creates a stream of any arbitrary length (int.MaxValue) by default
    // Will return zeros when read from until it's arbitrary length has been reached
    // Can be written to until it reaches it's maximum capacity of 8 exbibytes (8 * 1024 * 1024 TiB)
    public class DiagnosticTestStream : Stream
    {
        private long _length;

        public DiagnosticTestStream()
            : this(int.MaxValue)
        {
        }

        public DiagnosticTestStream(long length)
        {
            _length = length;
        }

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long position;

            switch (origin)
            {
                case SeekOrigin.Begin:
                    position = offset;
                    break;

                case SeekOrigin.Current:
                    position = Position + offset;
                    break;

                case SeekOrigin.End:
                    position = Length + offset;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
            }

            if (position < 0)
                position = 0;

            if (position > Length)
                position = Length;

            Position = position;

            return position;
        }

        public override void SetLength(long value)
        {
            if (Position > value)
                Position = value;

            _length = value;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            long bytesRead = Math.Min(Length - Position, count);

            for (int i = 0; i < bytesRead; i++)
                buffer[offset++] = 0;

            Position += bytesRead;

            return (int) bytesRead;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Position += count;
            _length += count;
        }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Length => _length;

        public override long Position { get; set; }
    }
}
