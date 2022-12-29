using Archiver.Planning.Model;

namespace Archiver.Planning
{
    public interface IBackupPlanOverviewViewModelFactory
    {
        BackupPlanOverviewViewModel CreateModel(BackupPlan plan);
    }
}