using ArchivePlanner.Planning.Model;
using System;

namespace ArchivePlanner.Backup
{
    public interface IProgressService
    {
        IObservable<BackupProgress?> BackupProgress { get; }

        void ReportProgress(Guid id, double progress);
    }
}