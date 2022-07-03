using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace ArchivePlanner.Util
{
    public class RateLimitedStream : Stream, IDisposable
    {
        private readonly int _rate;
        private readonly Stopwatch _watch;

        private long _writtenBytes;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="rate">Rate in kB/s</param>
        public RateLimitedStream(Stream stream, int rate)
        {
            BaseStream = stream;
            _rate = rate;
            _watch = new Stopwatch();
        }

        public int Rate => _rate;

        public Stream BaseStream { get; set; }

        public override bool CanRead => BaseStream.CanRead;

        public override bool CanSeek => BaseStream.CanSeek;

        public override bool CanWrite => BaseStream.CanWrite;

        public override long Length => BaseStream.Length;

        public override long Position { get => BaseStream.Position; set => BaseStream.Position = value; }

        public override void Flush()
        {
            BaseStream.Flush();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return BaseStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return BaseStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            BaseStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            BaseStream.Write(buffer, offset, count);

            if (!_watch.IsRunning)
            {
                _watch.Start();
            }

            Interlocked.Add(ref _writtenBytes, count);

            var targetTimeSpan = TimeSpan.FromSeconds((double)_writtenBytes / (_rate * 1024));
            var actualTimeSpan = _watch.Elapsed;
            var delayInS = targetTimeSpan - actualTimeSpan;

            if (delayInS > TimeSpan.Zero)
            {
                Thread.Sleep(Convert.ToInt32(delayInS.TotalMilliseconds));
            }
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await base.WriteAsync(buffer, offset, count, cancellationToken);
        }
    }
}
