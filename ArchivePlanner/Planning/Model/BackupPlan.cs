using NodaTime;
using System;
using System.Collections.Generic;
using System.IO;

namespace ArchivePlanner.Planning.Model
{
    public abstract class BackupPlan
    {
        public string Name { get; set; } = null!;

        public DirectoryInfo Destination { get; set; } = null!;

        public ICollection<FileSystemInfo> FileSystemItems { get; set; } = new List<FileSystemInfo>();

        public LocalTime ExecutionStart { get; set; }

        public DayOfWeek[] Interval { get; set; } = Array.Empty<DayOfWeek>();

        public string UniqueName
        {
            get
            {
                var timestamp = SystemClock.Instance.GetCurrentInstant().ToString("yyyy-MM-dd-HH-mm-ss", null);
                return $"{Name}_{timestamp}";
            }
        }

        public abstract IEnumerable<FileInfo> GetFilesToBackup();
    }
}
