using SharpCompress.Archives.Tar;
using SharpCompress.Writers.Tar;
using System.IO;

namespace ArchivePlanner.Util
{
    public static class TarArchiveExtension
    {
        public static void SaveToWithoutEndBlocks(this TarArchive archive, Stream stream)
        {
            var options = new TarWriterOptions(SharpCompress.Common.CompressionType.None, false);
            archive.SaveTo(stream, options);

            // library has a bug and overwrites attributes
            var endOfTarSize = 2 * 512;
            stream.Seek(-endOfTarSize, SeekOrigin.End);
            stream.SetLength(stream.Length - endOfTarSize);
        }

        public static void AddEntry(this TarArchive archive, FileInfo file)
        {
            var stream = file.OpenRead();
            archive.AddEntry(file.FullName, stream, file.Length, file.LastWriteTime);
        }
    }
}
