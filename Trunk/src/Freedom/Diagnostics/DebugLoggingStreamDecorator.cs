using System;
using System.IO;
using System.Reflection;
using log4net;

namespace Freedom.Diagnostics
{
    public class DebugLoggingStreamDecorator : Stream
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Stream _innerStream;

        public DebugLoggingStreamDecorator(Stream innerStream)
        {
            Log.DebugFormat("Created logging stream for {0}.", _innerStream);

            _innerStream = innerStream;

            try
            {
                Log.DebugFormat("Inner stream length is {0}", _innerStream.Length);
            }
            catch (NotSupportedException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
        }

        #region Overrides of Stream

        public override void Flush()
        {
            Log.Debug("Flushing stream.");

            _innerStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long result = _innerStream.Seek(offset, origin);

            Log.DebugFormat("Seeking to offset {0} from {1} resulted in new position of {2}.", offset, origin, result);

            return result;
        }

        public override void SetLength(long value)
        {
            Log.DebugFormat("Setting Length to {0}", value);
            
            _innerStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = _innerStream.Read(buffer, offset, count);

            Log.DebugFormat("Read from stream. bytes requested = {0}, bytes read = {1}, new position = {2}", count, bytesRead, _innerStream.Position);

            return bytesRead;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Log.DebugFormat("Wrote {0} bytes to stream.", count);
        }

        public override bool CanRead => _innerStream.CanRead;

        public override bool CanSeek => _innerStream.CanSeek;

        public override bool CanWrite => _innerStream.CanWrite;

        public override long Length => _innerStream.Length;

        public override long Position
        {
            get { return _innerStream.Position; }
            set
            {
                _innerStream.Position = value;

                Log.DebugFormat("Set stream position to {0} (target was {1})", _innerStream.Position, value);
            }
        }

        #endregion
    }
}
