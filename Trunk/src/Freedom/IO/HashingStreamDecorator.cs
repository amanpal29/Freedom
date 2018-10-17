using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Freedom.Extensions;

namespace Freedom.IO
{
    /// <summary>
    /// Computes the hash of the data read from the innerStream using the specified hashAlgorithm
    /// </summary>
    public class HashingStreamDecorator : Stream
    {
        private readonly Stream _innerStream;
        private readonly HashAlgorithm _hashAlgorithm;

        public HashingStreamDecorator(Stream innerStream, HashAlgorithm hashAlgorithm)
        {
            if (innerStream == null)
                throw new ArgumentNullException(nameof(innerStream));

            if (hashAlgorithm == null)
                throw new ArgumentNullException(nameof(hashAlgorithm));

            _innerStream = innerStream;
            _hashAlgorithm = hashAlgorithm;
        }

        public byte[] Hash => _hashAlgorithm.Hash;

        #region Overrides of Stream

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = _innerStream.Read(buffer, offset, count);

            if (bytesRead > 0)
                _hashAlgorithm.TransformBlock(buffer, offset, bytesRead, null, 0);
            else
                _hashAlgorithm.TransformFinalBlock(buffer, offset, 0);

            return bytesRead;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return ReadAsync(buffer, offset, count).Begin(callback, state);
        }

        public override int EndRead(IAsyncResult ar)
        {
            return ar.End<int>();
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int bytesRead = await _innerStream.ReadAsync(buffer, offset, count, cancellationToken);

            if (bytesRead > 0)
                _hashAlgorithm.TransformBlock(buffer, offset, bytesRead, null, 0);
            else
                _hashAlgorithm.TransformFinalBlock(buffer, offset, 0);

            return bytesRead;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override bool CanRead => _innerStream.CanRead;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length
        {
            get {  throw new NotSupportedException(); }
        }

        public override long Position
        {
            get { return _innerStream.Position; }
            set { throw new NotSupportedException(); }
        }

        #endregion
    }
}
