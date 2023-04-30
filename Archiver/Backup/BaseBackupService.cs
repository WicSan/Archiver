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
        protected IAsyncFtpClient _ftpClient;
        private ILogger<BaseBackupService> _logger;

        public BaseBackupService(BackupPlan plan, IProgressService service, IFtpClientFactory ftpClientFactory, ILogger<BaseBackupService> logger)
        {
            _plan = plan;
            _service = service;
            _ftpClient = ftpClientFactory.CreateFtpClient(_plan.Connection);
            _logger = logger;
        }

        protected BackupPlan Plan => _plan;

        public async Task RestoreAsync(string destination, CancellationToken cancellationToken)
        {
            await _ftpClient.Connect(3, cancellationToken);

            await _ftpClient.DownloadDirectory(destination, Path.Combine(_plan.DestinationFolder, _plan.Name));
        }

        public async Task RestoreFilesAsync(string destination, IEnumerable<string> files, CancellationToken cancellationToken)
        {
            var cfiles = files.ToList();
            
            await _ftpClient.Connect(3, cancellationToken);
            var archiveFiles = (await GetArchiveFiles(cancellationToken)).OrderBy(a => a.Name);

            foreach(var file in archiveFiles)
            {
                using(var stream = new SeekableFtpFileStream(_ftpClient, file.FullName))
                using(var archive = new ZipArchive(stream))
                {
                    var archiveEntries = archive.Entries.Where(e => files.Any(f => f.Equals(e.Name, StringComparison.OrdinalIgnoreCase)));
                    foreach(var archiveEntry in archiveEntries)
                    {
                        archiveEntry.ExtractToFile(Path.Combine(destination, archiveEntry.Name));
                        cfiles.Remove(archiveEntry.Name);
                    }
                }
            }
        }

        protected abstract Task<IEnumerable<FtpListItem>> GetArchiveFiles(CancellationToken cancellationToken);

        public async Task<bool> ExecuteBackupAsync(CancellationToken cancellationToken)
        {
            var isSuccessfull = false;
            try
            {
                _logger.LogInformation("Plan {Name} started at {StartDateTime} with file name {FullName}", _plan.Name, DateTime.Now, _plan.FullName);
                await _ftpClient.Connect(3, cancellationToken);

                var archiveEntries = (await GetArchivedFiles(cancellationToken)).ToList();
                var filesToBackup = GetFilesToBackup(archiveEntries, GetLocalFiles()).ToList();
                if (filesToBackup.Any())
                {
                    isSuccessfull = await BackupFiles(filesToBackup, cancellationToken);
                }
                
                _service.Complete(_plan.Id);
            }
            catch (Exception e)
            {
                _logger.LogError("{@Error}", e);
            }
            finally
            {
                await _ftpClient.Disconnect();
                _plan.ResetProgress();
            }

            return isSuccessfull;
        }

        protected abstract IEnumerable<FileInfo> GetFilesToBackup(IEnumerable<ZipArchiveEntry> archivedFiles, IEnumerable<FileInfo> newFiles);

        public async Task<IEnumerable<ZipArchiveEntry>> GetArchivedFiles(CancellationToken cancellationToken)
        {
            if (!_ftpClient.IsConnected)
            {
                await _ftpClient.Connect();
            }

            var archiveFiles = await GetArchiveFiles(cancellationToken);
            var archivedFiles = archiveFiles.SelectMany(z =>
            {
                try
                {
                    using (var stream = new SeekableFtpFileStream(_ftpClient, z.FullName))
                    using (var zipArchive = new ZipArchive(stream, ZipArchiveMode.Read))
                    {
                        return zipArchive.Entries;
                    }
                }
                catch (Exception)
                {
                    return Enumerable.Empty<ZipArchiveEntry>();
                }
            }).ToList();

            await _ftpClient.Disconnect();

            return archivedFiles;
        }

        private async Task<bool> BackupFiles(List<FileInfo> filesToBackup, CancellationToken cancellationToken)
        {
            var folder = $"/{Path.Combine(_plan.DestinationFolder, _plan.Name)}";
            await _ftpClient.CreateDirectory(folder);

            var currentTime = SystemClock.Instance.GetCurrentInstant().ToString("yyyy-MM-dd-HH-mm-ss", null);
            var prefix = "full";
            if (_plan.InitialBackupExecuted)
            {
                prefix = "diff";
            }
            var fullName = Path.Combine(folder, $"{prefix}_{currentTime}.zip");
            using (var uploadStream = await _ftpClient.OpenWrite(fullName, FtpDataType.Binary, false, cancellationToken))
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
                    _service.ReportProgress(_plan.Id, progress);
                }
            }

            return await IsBackupSuccessfull();
        }

        private IEnumerable<FileInfo> GetLocalFiles()
        {
            var options = new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = true
            };

            var singleFiles = _plan.FileSystemItems
                .Where(i => i is FileInfo)
                .Cast<FileInfo>();
            var files = _plan.FileSystemItems
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

        private async Task<bool> IsBackupSuccessfull()
        {
            var replay = await _ftpClient.GetReply();
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
