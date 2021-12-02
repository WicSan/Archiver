using SharpCompress.Writers.Tar;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace ArchivePlanner.Util
{
    public static class TarWriterExtension
    {
        public static void AddGzipEntry(this TarWriter writer, FileInfo file)
        {
            using var entryStream = file.OpenRead();
            var tempfile = new FileInfo(Path.GetTempFileName());
            using (var tempStream = tempfile.Open(FileMode.Open, FileAccess.ReadWrite))
            {
                using (var gzStream = new GZipStream(tempStream, CompressionMode.Compress, true))
                {
                    entryStream.CopyTo(gzStream);
                }

                tempStream.Seek(0, SeekOrigin.Begin);
                writer.Write($"{file.FullName}.gz", tempStream, file.LastWriteTime, tempStream.Length);
            }

            Task.Run(() => tempfile.Delete());
        }
    }
}
