using FluentFTP;
using System.Collections.Generic;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace Archiver.Backup
{
    public interface IBackupService
    {
        Task<bool> ExecuteBackupAsync(CancellationToken cancellationToken);

        Task RestoreAsync(string destination, CancellationToken cancellationToken);

        Task RestoreFilesAsync(string destination, IEnumerable<string> files, CancellationToken cancellationToken);

        Task<IEnumerable<ZipArchiveEntry>> GetArchivedFiles(CancellationToken cancellationToken);
    }
}
