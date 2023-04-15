using Archiver.Planning;
using Archiver.Planning.Model;
using FluentFTP;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Archiver.Backup
{
    public class FullBackupService : BaseBackupService
    {
        public FullBackupService(BackupPlan plan, IProgressService service, IFtpClientFactory ftpClientFactory, ILogger<BaseBackupService> logger)
            : base(plan, service, ftpClientFactory, logger)
        {
        }

        protected override async Task<IEnumerable<FtpListItem>> GetArchiveFiles(CancellationToken cancellationToken)
        {
            var zipFiles = await _ftpClient.GetListing($"/{Plan.DestinationFolder}/{Plan.Name}", cancellationToken);

            return zipFiles
                .OrderByDescending(f => f.Modified)
                .Take(1);
        }

        protected override IEnumerable<FileInfo> GetFilesToBackup(IEnumerable<ZipArchiveEntry> archivedFiles, IEnumerable<FileInfo> newFiles)
        {
            return newFiles;
        }
    }
}
