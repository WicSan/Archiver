using Archiver.Planning;
using Archiver.Planning.Model;
using Archiver.Util;
using FluentFTP;
using Microsoft.Extensions.Logging;
using NodaTime;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Archiver.Backup
{
    public abstract class BaseBackupService : IBackupService
    {
        private readonly BackupPlan _plan;
        private IProgressService _service;
        private IFtpClientFactory _ftpClientFactory;
        private ILogger<BaseBackupService> _logger;

        public BaseBackupService(BackupPlan plan, IProgressService service, IFtpClientFactory ftpClientFactory, ILogger<BaseBackupService> logger)
        {
            _plan = plan;
            _service = service;
            _ftpClientFactory = ftpClientFactory;
            _logger = logger;
        }

        public async Task<bool> ExecuteAsync(CancellationToken cancellationToken)
        {
            var isSuccessfull = false;
            try
            {
                using var client = _ftpClientFactory.CreateFtpClient(_plan.Connection);

                _logger.LogInformation("Plan {Name} started at {StartDateTime} with file name {FullName}", _plan.Name, DateTime.Now, _plan.FullName);

                await client.ConnectAsync(3, cancellationToken);

                var archiveEntries = (await GetBackedUpFiles(client)).ToList();
                isSuccessfull = await BackupFiles(client, _plan, archiveEntries, cancellationToken);

                _service.Complete(_plan.Id);
            }
            catch (Exception e)
            {
                _logger.LogError("{@Error}", e);
            }
            finally
            {
                _plan.ResetProgress();
            }

            return isSuccessfull;
        }

        protected abstract IEnumerable<FileInfo> GetFilesToBackup(IEnumerable<ZipArchiveEntry> archivedFiles, IEnumerable<FileInfo> newFiles);

        private async Task<IEnumerable<ZipArchiveEntry>> GetBackedUpFiles(IFtpClient client)
        {
            var zipFiles = await client.GetListingAsync($"/{_plan.DestinationFolder}/{_plan.Name}");
            if (!zipFiles.Any(z => z.Name.Contains("full")))
            {
                if (zipFiles.Any())
                {
                    _logger.LogError("Missing full backup file");
                }

                return Enumerable.Empty<ZipArchiveEntry>();
            }

            return zipFiles.SelectMany(z =>
            {
                try
                {
                    using (var stream = new SeekableFtpFileStream(client, z.FullName))
                    using (var zipArchive = new ZipArchive(stream, ZipArchiveMode.Read))
                    {
                        return zipArchive.Entries;
                    }
                }
                catch (Exception)
                {
                    return Enumerable.Empty<ZipArchiveEntry>();
                }
            });
        }

        private async Task<bool> BackupFiles(IFtpClient client, BackupPlan plan, List<ZipArchiveEntry> archiveEntries, CancellationToken cancellationToken)
        {
            var filesToBackup = GetFilesToBackup(archiveEntries, GetLocalFiles(plan)).ToList();
            if (!filesToBackup.Any())
            {
                return true;
            }

            var folder = $"/{Path.Combine(plan.DestinationFolder, plan.Name)}";
            await client.CreateDirectoryAsync(folder);

            var currentTime = SystemClock.Instance.GetCurrentInstant().ToString("yyyy-MM-dd-HH-mm-ss", null);
            var prefix = "full";
            if (archiveEntries.Any())
            {
                prefix = "diff";
            }
            var fullName = Path.Combine(folder, $"{prefix}_{currentTime}.zip");
            using (var uploadStream = await client.OpenWriteAsync(fullName, FtpDataType.Binary, false, cancellationToken))
            using (var limitedStream = new RateLimitedStream(uploadStream, 80))
            using (var archive = new ZipArchive(limitedStream, ZipArchiveMode.Create, false))
            {
                var progress = 0.0;
                var totalFiles = filesToBackup.Count();
                foreach (var file in filesToBackup)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        _logger.LogInformation("Backup aborted.");
                        return false;
                    }

                    archive.AddEntry(file);

                    progress += 1.0 / totalFiles;
                    _service.ReportProgress(plan.Id, progress);
                }
            }

            return IsBackupSuccessfull(client);
        }

        private IEnumerable<FileInfo> GetLocalFiles(BackupPlan plan)
        {
            var options = new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = true
            };

            var singleFiles = plan.FileSystemItems
                .Where(i => i is FileInfo)
                .Cast<FileInfo>();
            var files = plan.FileSystemItems
                .Where(i => i is DirectoryInfo)
                .Cast<DirectoryInfo>()
                .Select(d => Directory.GetFiles(d.FullName, "*", options).Select(f => new FileInfo(f)))
                .SelectMany(l => l)
                .Concat(singleFiles);

            foreach (var file in files)
            {
                yield return file;
            }
        }

        private bool IsBackupSuccessfull(IFtpClient client)
        {
            var replay = client.GetReply();
            if (replay.Success)
            {
                _logger.LogInformation("Backup plan {Name} execution successfull.", _plan.Name);
                return true;
            }
            else
            {
                _logger.LogError("Failed to upload backup file with error {Error}", replay.ErrorMessage);
                return false;
            }
        }
    }
}
