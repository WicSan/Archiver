using System;

namespace ArchivePlanner.Backup
{
    public class BackupProgress
    {
        public Guid BackupId { get; private set; }

        public double Progress { get; private set; }

        public BackupProgress(Guid id, double progress)
        {
            BackupId = id;
            Progress = progress;
        }
    }
}
