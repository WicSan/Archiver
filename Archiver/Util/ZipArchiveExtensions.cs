using NodaTime;
using System.IO;
using System.IO.Compression;
using static NodaTime.Extensions.DateTimeExtensions;

namespace Archiver.Util
{
    public static class ZipArchiveExtensions
    {
        public static void AddEntry(this ZipArchive archive, FileInfo file)
        {
            var entry = archive.CreateEntry(file.FullName, CompressionLevel.Fastest);
            var offset = file.LastWriteTime.ToLocalDateTime() - file.LastWriteTimeUtc.ToLocalDateTime();
            entry.LastWriteTime = file.LastWriteTime
                .ToLocalDateTime()
                .InZoneStrictly(DateTimeZone.ForOffset(Offset.FromHours((int)offset.Hours)))
                .ToDateTimeOffset();

            using (var entryStream = entry.Open())
            using (var fileStream = file.OpenRead())
            {
                fileStream.CopyTo(entryStream);
            }
        }
    }
}
