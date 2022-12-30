using System;

namespace Archiver.Backup
{
    public class BackupProgress
    {
        public Guid BackupId { get; private set; }

        public double Percentage { get; private set; }

        public BackupProgress(Guid id, double progress)
        {
            BackupId = id;
            Percentage = progress;
        }
    }
}
