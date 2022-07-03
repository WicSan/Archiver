using System;
using System.Reactive.Subjects;

namespace ArchivePlanner.Backup
{
    public class ProgressService : IProgressService
    {
        private readonly BehaviorSubject<BackupProgress?> _progressStream = new (null);

        public ProgressService()
        {
        }

        public IObservable<BackupProgress?> BackupProgress => _progressStream;

        public void ReportProgress(Guid id, double progress)
        {
            var prog = new BackupProgress(id, progress);
            _progressStream.OnNext(prog);
        }
    }
}
