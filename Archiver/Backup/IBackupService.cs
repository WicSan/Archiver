using System.Threading;
using System.Threading.Tasks;

namespace Archiver.Backup
{
    public interface IBackupService
    {
        Task<bool> ExecuteAsync(CancellationToken cancellationToken);
    }
}
