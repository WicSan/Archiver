using Archiver.Planning;
using Archiver.Planning.Model;
using Archiver.Util;
using FluentFTP;
using Microsoft.Extensions.Logging;
using NodaTime;
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
        public FullBackupService(
            BackupPlan plan, 
            IProgressService service,
            IFtpClientFactory ftpClientFactory,
            IRepository<BackupPlan> repository,
            IClock clock,
            ILogger<BaseBackupService> logger)
            : base(plan, service, ftpClientFactory, repository, clock, logger)
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
