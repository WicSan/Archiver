using FluentFTP;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Archiver.Backup
{
    public class SeekableFtpFileStream : Stream, IDisposable
    {
        private long _length;

        // Current position, used to move cursor to the right position
        private long _position;
        // Pointing to the current reading position on the stream
        private long _cursor;

        private readonly IAsyncFtpClient _ftpClient;
        private readonly string _fullName;

        private Stream? _stream;

        public SeekableFtpFileStream(IAsyncFtpClient client, string fullName)
        {
            _ftpClient = client;
            _fullName = fullName;
            _stream = null;
            _cursor = 0;
            _position = 0;
            _length = -1;
        }

        /// <summary>
        /// Close, dispose and nullify _stream and _response
        /// </summary>
        private void CloseStreamingConnection()
        {
            // Clean up stream if exists
            if (_stream != null)
            {
                _stream.Close();
                _stream.Dispose();
                _stream = null;

                try
                {
                    var re = Task.Run(() => _ftpClient.GetReply()).GetAwaiter().GetResult();
                }
                catch (TimeoutException)
                {
                }
            }
        }

        /// <summary>
        /// If position is smaller than length, stream can read.
        /// </summary>
        public override bool CanRead
        {
            get
            {
                return Position < Length;
            }
        }

        /// <summary>
        /// Always true
        /// </summary>
        public override bool CanSeek
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Always false
        /// </summary>
        public override bool CanWrite
        {
            get
            {
                return true;
            }
        }

        /// <summary>
        /// Gets length of file
        /// </summary>
        public override long Length
        {
            get
            {
                InitializeLength();
                return _length;
            }
        }


        /// <summary>
        /// Gets stream position
        /// </summary>
        public override long Position
        {
            get
            {
                return _position;
            }

            set
            {
                _position = value;
            }
        }

        /// <summary>
        /// Always throws NotSupportedException
        /// </summary>
        public override void Flush()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Reads from stream to buffer
        /// </summary>
        /// <param name="buffer">Byte[] to contain read bytes</param>
        /// <param name="offset">Offset from position</param>
        /// <param name="count">Bytes to read</param>
        /// <returns>Bytes read count</returns>
        public override int Read(byte[] buffer, int offset, int count)
        {
            InitializeLength();
            if (_cursor > _position || _stream == null)
            {
                CloseStreamingConnection();
                _stream = Task.Run(async () => await _ftpClient.OpenRead(_fullName, FtpDataType.Binary, _position, Length)).GetAwaiter().GetResult();

                _cursor = _position;
            }
            else if (_cursor < _position)
            {
                for (; _cursor < _position; _cursor++)
                {
                    // Skip bytes
                    _stream.ReadByte();
                }
            }

            int bytesRead = _stream.Read(buffer, offset, count);

            _cursor += bytesRead;
            _position = _cursor;

            return bytesRead;
        }

        /// <summary>
        /// Seeks for a position on the stream
        /// </summary>
        /// <exception cref="ArgumentException">Thrown on seeking before or after stream</exception>
        /// <param name="offset">Offset from origin</param>
        /// <param name="origin">Seeking from here</param>
        /// <returns>Position on stream</returns>
        public override long Seek(long offset, SeekOrigin origin)
        {
            InitializeLength();
            switch (origin)
            {
                case SeekOrigin.Begin:
                    if (offset < 0)
                        throw new ArgumentException("Cannot seek before beginning.");
                    if (offset > _length)
                        throw new ArgumentException("Cannot seek after ending.");
                    // Set position to offset
                    _position = offset;
                    break;
                case SeekOrigin.End:
                    if (offset > 0)
                        throw new ArgumentException("Cannot seek after ending.");
                    if (offset < _length * -1)
                        throw new ArgumentException("Cannot seek before beginning.");
                    // Add offset to file _length
                    _position = _length + offset;
                    break;
                default:
                    if (_position + offset < 0)
                        throw new ArgumentException("Cannot seek before beginning.");
                    if (_position + offset > _length)
                        throw new ArgumentException("Cannot seek after ending.");
                    // Add offset to current _position
                    _position = _position + offset;
                    break;
            }

            return _position;
        }

        private void InitializeLength()
        {
            if(_length == -1)
            {
                _length = Task.Run(() => _ftpClient.GetFileSize(_fullName)).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Always throws NotSupportedException
        /// </summary>
        /// <param name="value"></param>
        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Always throws NotSupportedException
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="count"></param>
        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Closes connection to FTP
        /// </summary>
        public new void Dispose()
        {
            CloseStreamingConnection();
        }
    }
}
