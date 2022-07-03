using ArchivePlanner.Backup;
using ArchivePlanner.Planning.Model;

namespace ArchivePlanner.Planning
{
    public class BackupPlanOverviewViewModelFactory : IBackupPlanOverviewViewModelFactory
    {
        private readonly IFtpClientFactory _ftpClientFactory;
        private readonly IProgressService _progressService;

        public BackupPlanOverviewViewModelFactory(IFtpClientFactory ftpClientFactory, IProgressService progressService)
        {
            _ftpClientFactory = ftpClientFactory;
            _progressService = progressService;
        }

        public BackupPlanOverviewViewModel CreateModel(BackupPlan plan)
        {
            var model = new BackupPlanOverviewViewModel(_ftpClientFactory, _progressService);
            model.BackupPlan = plan;
            return model;
        }
    }
}
