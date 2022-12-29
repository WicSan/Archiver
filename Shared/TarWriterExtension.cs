using System.IO;
using System.IO.Compression;

namespace ArchivePlanner.Util
{
    public static class TarWriterExtension
    {
        /*public static void AddGzipEntry(this TarWriter writer, FileInfo file)
        {
            using var fileStream = file.OpenRead();
            var tempfile = new FileInfo("upload.tmp");
            using (var tempStream = tempfile.Open(FileMode.Create, FileAccess.ReadWrite))
            {
                using (var gzStream = new GZipStream(tempStream, CompressionMode.Compress, true))
                {
                    fileStream.CopyTo(gzStream);
                }

                tempStream.Seek(0, SeekOrigin.Begin);
                writer.Write($"{file.FullName}.gz", tempStream, file.LastWriteTime, tempStream.Length);
            }

            tempfile.Delete();
        }*/
    }
}
