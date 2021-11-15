using ArchivePlanner.Planning;
using ArchivePlanner.Planning.Model;
using ArchivePlanner.Util;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SharpCompress.Archives.Tar;
using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ArchivePlanner.Backup
{
    public class ArchiverService : BackgroundService
    {
        private readonly IPlanningRepository _repository;
        private readonly ILogger<ArchiverService> _logger;

        public ArchiverService(IPlanningRepository repository, ILogger<ArchiverService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var plans = _repository.GetAll<BackupPlan>();

            foreach(var plan in plans)
            {
                var tar = CreateTarArchive(plan, stoppingToken);
                CreateGzip(plan, tar);
            }

            return Task.CompletedTask;
        }

        private FileStream CreateTarArchive(BackupPlan plan, CancellationToken cancellationToken)
        {
            var tarTempFileName = Path.GetTempFileName();
            using var tarFileStream = new FileStream(tarTempFileName, FileMode.Create);

            var parallelOptions = new ParallelOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 3,
            };
            var lockObject = new object();

            var executionResult = Parallel.ForEach(plan.GetFilesToBackup(), parallelOptions,
                CreateTemporaryTarOutputStream,
                (file, _, tuple) =>
                {
                    tuple.Archive.AddEntry(file);
                    return tuple;
                },
                (tuple) =>
                {
                    using (var archive = tuple.Archive)
                    {
                        lock (lockObject)
                        {
                            tuple.Archive.SaveToWithoutEndBlocks(tarFileStream);
                        }
                    }

                    File.Delete(tuple.Name);
                });

            return tarFileStream;
        }

        private void CreateGzip(BackupPlan plan, FileStream tarFileStream)
        {
            var gzFileName = $"{plan.Name}_{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")}.tar.gz";
            using var gzFileStream = File.Open(gzFileName, FileMode.Create);
            using var compressionStream = new GZipStream(gzFileStream, CompressionMode.Compress);

            tarFileStream.Seek(0, SeekOrigin.Begin);

            using var archive = TarArchive.Open(tarFileStream);
            archive.SaveTo(compressionStream, new WriterOptions(CompressionType.None));
        }

        private (string Name, TarArchive Archive) CreateTemporaryTarOutputStream()
        {
            var fileName = Path.GetTempFileName();
            var fileStream = new FileStream(fileName, FileMode.Open);
            var archive = TarArchive.Open(fileStream, new ReaderOptions { LeaveStreamOpen = false });

            return (Name: fileName, Archive: archive);
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
