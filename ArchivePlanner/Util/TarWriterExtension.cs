using SharpCompress.Writers.Tar;
using System.IO;

namespace ArchivePlanner.Util
{
    public static class TarWriterExtension
    {
        public static void AddEntry(this TarWriter writer, FileInfo file)
        {
            var stream = file.OpenRead();
            writer.Write(file.FullName, stream, file.LastWriteTime);
        }
    }
}
