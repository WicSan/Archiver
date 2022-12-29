using Archiver.Backup;
using Archiver.Planning.Model;
using NodaTime;

namespace Archiver.Planning
{
    public class BackupPlanOverviewViewModelFactory : IBackupPlanOverviewViewModelFactory
    {
        private readonly IFtpClientFactory _ftpClientFactory;
        private readonly IProgressService _progressService;
        private readonly IClock _clock;

        public BackupPlanOverviewViewModelFactory(IFtpClientFactory ftpClientFactory, IProgressService progressService, IClock clock)
        {
            _ftpClientFactory = ftpClientFactory;
            _progressService = progressService;
            _clock = clock;
        }

        public BackupPlanOverviewViewModel CreateModel(BackupPlan plan)
        {
            var model = new BackupPlanOverviewViewModel(_ftpClientFactory, _progressService, _clock, plan);
            return model;
        }
    }
}
