using ICSharpCode.SharpZipLib.Tar;
using System.IO;
using System.Threading.Tasks;

namespace ArchiveBot
{
    public static class TarOutputStreamExtension
    {
        public static async Task WriteFileAsync(this TarOutputStream outputStream, FileInfo file)
        {
            outputStream.PutNextEntry(TarEntry.CreateEntryFromFile(file.FullName));

            using var fileStream = file.OpenRead();
            await fileStream.CopyToAsync(outputStream);

            outputStream.CloseEntry();
        }
    }
}
