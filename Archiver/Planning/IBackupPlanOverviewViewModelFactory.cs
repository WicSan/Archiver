using ArchivePlanner.Planning.Model;

namespace ArchivePlanner.Planning
{
    public interface IBackupPlanOverviewViewModelFactory
    {
        BackupPlanOverviewViewModel CreateModel(BackupPlan plan);
    }
}