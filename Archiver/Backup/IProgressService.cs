﻿using Archiver.Backup.Model;
using Archiver.Planning.Model;
using System;

namespace Archiver.Backup
{
    public interface IProgressService
    {
        IObservable<BackupProgress?> BackupProgress { get; }

        void ReportProgress(Guid id, double progress);

        void Complete(Guid id);
    }
}