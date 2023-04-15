using Archiver.Planning;
using Archiver.Planning.Model;
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
    public class DifferantialBackupService : BaseBackupService
    {
        public DifferantialBackupService(BackupPlan plan, IProgressService service, IFtpClientFactory ftpClientFactory, ILogger<BaseBackupService> logger) 
            : base(plan, service, ftpClientFactory, logger)
        {

        }

        protected override async Task<IEnumerable<FtpListItem>> GetArchiveFiles(CancellationToken cancellationToken)
        {
            var zipFiles = await _ftpClient.GetListing($"/{Plan.DestinationFolder}/{Plan.Name}", cancellationToken);
            if (!zipFiles.Any(z => z.Name.Contains("full")))
            {
                if (zipFiles.Any())
                {
                    throw new Exception("Missing full backup file");
                }
            }

            return zipFiles.ToList();
        }

        protected override IEnumerable<FileInfo> GetFilesToBackup(IEnumerable<ZipArchiveEntry> archivedFiles, IEnumerable<FileInfo> newFiles)
        {
            if (!archivedFiles.Any())
            {
                return newFiles;
            }

            var archivedDict = archivedFiles
                .GroupBy(f => Path.GetFullPath(f.FullName).GetHashCode())
                .Select(g => g.OrderBy(e => e.LastWriteTime).Last())
                .ToDictionary(x => Path.GetFullPath(x.FullName).GetHashCode());

            return newFiles
                .Where(f => 
                {
                    if (!archivedDict.TryGetValue(f.FullName.GetHashCode(), out var archivedFile))
                        return true;

                    var archiveTime = new LocalDateTime(archivedFile.LastWriteTime.Year,
                                                        archivedFile.LastWriteTime.Month,
                                                        archivedFile.LastWriteTime.Day,
                                                        archivedFile.LastWriteTime.Hour,
                                                        archivedFile.LastWriteTime.Minute);
                    var fileTime = new LocalDateTime(f.LastWriteTime.Year,
                                                     f.LastWriteTime.Month,
                                                     f.LastWriteTime.Day,
                                                     f.LastWriteTime.Hour,
                                                     f.LastWriteTime.Minute);
                    return fileTime > archiveTime;
                });
        }
    }
}
