using FastBackup.Planning.Model;

namespace FastBackup.Operation.Model
{
    public interface BackupStrategy
    {
        public void Backup(BackupPlan plan);
    }
}
