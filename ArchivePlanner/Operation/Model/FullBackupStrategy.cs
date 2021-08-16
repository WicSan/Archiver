using FastBackup.Planning.Model;
using ICSharpCode.SharpZipLib.Tar;
using NodaTime;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace FastBackup.Operation.Model
{
    public class FullBackupStrategy : BackupStrategy
    {
        public void Backup(BackupPlan plan)
        {
            var timestamp = SystemClock.Instance.GetCurrentInstant().ToString("yyyy-MM-dd-HH-mm-ss", null);
            using var tarFileStream = new FileStream(Path.Combine(plan.Destination.FullName, $"{plan.Name}_{timestamp}.gz.tar"), FileMode.OpenOrCreate);

            using var s = new TarOutputStream(tarFileStream, Encoding.UTF8);

            var options = new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = true
            };

            foreach (var item in plan.FileSystemItems)
            {
                if (item is DirectoryInfo)
                {
                    var tarEntries = Directory.EnumerateFiles(item.FullName, "*", options)
                        .AsParallel()
                        .WithDegreeOfParallelism(5)
                        .Select(CreateGzippedEntry);
                    foreach (var entry in tarEntries)
                    {
                        s.PutNextEntry(entry.Entry);
                        s.Write(entry.Content, 0, entry.Content.Length);
                        s.CloseEntry();
                    }
                }
                else
                {
                    var entry = CreateGzippedEntry(item.FullName);
                    s.PutNextEntry(entry.Entry);
                    s.Write(entry.Content, 0, entry.Content.Length);
                    s.CloseEntry();
                }
            }
        }

        private (TarEntry Entry, byte[] Content) CreateGzippedEntry(string path)
        {
            MemoryStream compressedFileStream = new MemoryStream();
            using (var compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress, true))
            {
                using var file = File.OpenRead(path);
                file.CopyTo(compressionStream);
            }
            compressedFileStream.Position = 0;

            var entry = TarEntry.CreateTarEntry($"{path.Replace(Path.VolumeSeparatorChar.ToString(), string.Empty)}.gz");
            entry.Size = compressedFileStream.Length;

            var mem = new byte[compressedFileStream.Length];
            compressedFileStream.Read(mem);
            return (entry, mem.ToArray());
        }
    }
}
