using Archiver.Planning.Model;
using System;

namespace Archiver.Backup
{
    public interface IProgressService
    {
        IObservable<BackupProgress?> BackupProgress { get; }

        void ReportProgress(Guid id, double progress);
    }
}