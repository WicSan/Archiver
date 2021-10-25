using Archiver.Shared.Operation;
using ICSharpCode.SharpZipLib.Tar;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ArchiveBot
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var plan = new FullBackupPlan()
            {
                Name = "test",
                Destination = new DirectoryInfo(Directory.GetCurrentDirectory()),
                FileSystemItems = new List<FileSystemInfo>
                {
                    new DirectoryInfo(@"C:\Users\sandr\Documents\They Are Billions"),
                },
            };
            using var tarFileStream = new FileStream(Path.Combine($"{plan.UniqueName}.tar"), FileMode.OpenOrCreate);
            using var tar = new TarOutputStream(tarFileStream, Encoding.UTF8);

            foreach (var file in plan.GetFilesToBackup())
            {
                await tar.WriteFileAsync(file);
            }
        }

        /*private (TarEntry Entry, byte[] Content) CreateGzippedEntry(string path)
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
        }*/
    }
}
