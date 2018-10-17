using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Freedom.Annotations;

namespace Freedom.IO
{
    public class ProgressReportingStreamDecorator : Stream, INotifyPropertyChanged
    {
        #region Fields

        private const int DefaultRateWindowSize = 5;
        private const long DefaultMinimumTicksBetweenProgressReports = 1L * TimeSpan.TicksPerSecond;

        private readonly Stream _innerStream;
        private readonly IProgress<ProgressReport> _progress;
        private readonly Queue<PositionDataPoint> _positions = new Queue<PositionDataPoint>();
        private long _lastReportedPosition;
        private long _length = -1;
        private int _rateWindowSize = DefaultRateWindowSize;
        private TimeSpan _minimumTimeBetweenProgressReports = new TimeSpan(DefaultMinimumTicksBetweenProgressReports);
        private DateTime _lastProgressReport;

        #endregion

        #region Constructor

        public ProgressReportingStreamDecorator(Stream innerStream)
            : this(innerStream, null)
        {
        }

        public ProgressReportingStreamDecorator(Stream innerStream, IProgress<ProgressReport> progress)
        {
            _innerStream = innerStream;
            _progress = progress;
        }

        #endregion

        #region Progress Reporting

        private void OnPositionChanged(long currentPosition)
        {
            if (_innerStream.CanSeek)
                currentPosition = _innerStream.Position;

            if (_lastReportedPosition == currentPosition) return;

            _lastReportedPosition = currentPosition;

            OnPropertyChanged(nameof(Position));

            if (_progress == null) return;

            ReportProgress(currentPosition);
        }

        private void ReportProgress(long currentPosition)
        {
            PositionDataPoint currentDataPoint = new PositionDataPoint(currentPosition);

            if (_lastProgressReport.Ticks == 0L)
            {
                // Shortcut if this is the first progress report...
                _lastProgressReport = currentDataPoint.DateTime;
                _positions.Enqueue(currentDataPoint);
                _progress?.Report(new ProgressReport(currentPosition, Length));
                return;
            }

            // If not enough time has passed between data points, don't do anything.

            TimeSpan timeSinceLastProgressReport = currentDataPoint.DateTime - _lastProgressReport;

            if (timeSinceLastProgressReport < MinimumTimeBetweenProgressReports)
                return;

            _lastProgressReport = currentDataPoint.DateTime;
            _positions.Enqueue(currentDataPoint);

            // We keep a "window" of data points and average the rate over that window,
            // to smooth out the progress rate reporting a bit.

            PositionDataPoint previousDataPoint = _positions.Count < RateWindowSize
                ? _positions.Peek()
                : _positions.Dequeue();

            long bytesTransfered = currentDataPoint.Position - previousDataPoint.Position;
            double secondsRequired = (currentDataPoint.DateTime - previousDataPoint.DateTime).TotalSeconds;

            if (bytesTransfered == 0L)
            {
                _progress?.Report(new ProgressReport(currentPosition, Length, 0.0));
            }
            else if (secondsRequired < 0.0000001)  // Sanity check so we don't div by zero.
            {
                _progress?.Report(new ProgressReport(currentPosition, Length));
            }
            else
            {
                _progress?.Report(new ProgressReport(currentPosition, Length, bytesTransfered/secondsRequired));
            }
        }

        #endregion

        #region Properties

        public int RateWindowSize
        {
            get { return _rateWindowSize; }
            set
            {
                if (value < 2)
                    throw new ArgumentException("RateWindowSize must be >= 2", nameof(value));

                _rateWindowSize = value;
            }
        }

        public TimeSpan MinimumTimeBetweenProgressReports
        {
            get { return _minimumTimeBetweenProgressReports; }
            set
            {
                if (value.Equals(_minimumTimeBetweenProgressReports)) return;
                _minimumTimeBetweenProgressReports = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Overrides of Stream

        public override void Flush()
        {
            _innerStream.Flush();
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return _innerStream.FlushAsync(cancellationToken);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long result = _innerStream.Seek(offset, origin);

            OnPositionChanged(result);

            return result;
        }

        public override void SetLength(long value)
        {
            _length = value;

            if (_innerStream.CanSeek && _innerStream.CanWrite)
                _innerStream.SetLength(value);

            OnPropertyChanged(nameof(Length));

            if (value < _lastReportedPosition)
                OnPositionChanged(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            int bytesRead = _innerStream.Read(buffer, offset, count);

            OnPositionChanged(_lastReportedPosition + bytesRead);

            return bytesRead;
        }

        public override IAsyncResult BeginRead(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            return _innerStream.BeginRead(buffer, offset, count, callback, state);
        }

        public override int EndRead(IAsyncResult asyncResult)
        {
            int bytesRead = _innerStream.EndRead(asyncResult);

            OnPositionChanged(_lastReportedPosition + bytesRead);

            return bytesRead;
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int bytesRead = await _innerStream.ReadAsync(buffer, offset, count, cancellationToken);

            OnPositionChanged(_lastReportedPosition + bytesRead);

            return bytesRead;
        }

        public override int ReadByte()
        {
            int result = _innerStream.ReadByte();

            OnPositionChanged(_lastReportedPosition + 1);

            return result;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _innerStream.Write(buffer, offset, count);

            OnPositionChanged(_lastReportedPosition + count);
        }

        public override IAsyncResult BeginWrite(byte[] buffer, int offset, int count, AsyncCallback callback, object state)
        {
            IAsyncResult result = _innerStream.BeginWrite(buffer, offset, count, callback, state);

            OnPositionChanged(_lastReportedPosition + count);

            return result;
        }

        public override void EndWrite(IAsyncResult asyncResult)
        {
            _innerStream.EndWrite(asyncResult);
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await _innerStream.WriteAsync(buffer, offset, count, cancellationToken);

            OnPositionChanged(_lastReportedPosition + count);
        }

        public override void WriteByte(byte value)
        {
            _innerStream.WriteByte(value);

            OnPositionChanged(_lastReportedPosition + 1);
        }

        public override bool CanRead => _innerStream.CanRead;

        public override bool CanSeek => _innerStream.CanSeek;

        public override bool CanTimeout => _innerStream.CanTimeout;

        public override bool CanWrite => _innerStream.CanWrite;

        public override long Length => _innerStream.CanSeek ? _innerStream.Length : _length;

        public override long Position
        {
            get { return _innerStream.CanSeek ? _innerStream.Position : _lastReportedPosition; }
            set
            {
                if (_innerStream.Position == value) return;
                _innerStream.Position = value;
                OnPositionChanged(value);
            }
        }

        public override int ReadTimeout
        {
            get { return _innerStream.ReadTimeout; }
            set { _innerStream.ReadTimeout = value; }
        }

        public override int WriteTimeout
        {
            get { return _innerStream.WriteTimeout; }
            set { _innerStream.WriteTimeout = value; }
        }

        public override void Close()
        {
            _innerStream.Close();
        }

        #endregion

        #region Overrides of IDisposable

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _innerStream.Dispose();
            }

            base.Dispose(disposing);
        }

        #endregion

        #region Implementation of INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region PositionDataPoint Private Class

        private struct PositionDataPoint
        {
            internal PositionDataPoint(long position) : this()
            {
                Position = position;
                DateTime = DateTime.UtcNow;
            }

            internal long Position { get; }

            internal DateTime DateTime { get; }
        }

        #endregion
    }
}
