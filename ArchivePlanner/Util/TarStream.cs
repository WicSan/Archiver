using SharpCompress.Common;
using SharpCompress.Writers.Tar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArchivePlanner.Util
{
    public class TarStream : Stream
    {
        private readonly TarWriter _writer;

        public TarStream(Stream stream)
        {
            var options = new TarWriterOptions(CompressionType.None, true);
            _writer = new TarWriter(stream, options);
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            //_writer.Write();
        }
    }
}
