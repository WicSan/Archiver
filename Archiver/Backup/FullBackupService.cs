using Archiver.Planning;
using Archiver.Planning.Model;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Archiver.Backup
{
    public class FullBackupService : BaseBackupService
    {
        public FullBackupService(BackupPlan plan, IProgressService service, IFtpClientFactory ftpClientFactory, ILogger<BaseBackupService> logger)
            : base(plan, service, ftpClientFactory, logger)
        {
        }

        protected override IEnumerable<FileInfo> GetFilesToBackup(IEnumerable<ZipArchiveEntry> archivedFiles, IEnumerable<FileInfo> newFiles)
        {
            return newFiles;
        }
    }
}
