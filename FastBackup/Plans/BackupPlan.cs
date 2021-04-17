using System;
using System.Collections.Generic;
using System.IO;

namespace FastBackup.Plans
{
    public class BackupPlan
    {
        public string Name { get; set; } = null!;

        public DateTime ExecutionStart { get; set; }

        public DirectoryInfo Destination { get; set; } = null!;

        public ICollection<FileSystemInfo> FileSystemItems { get; set; } = new List<FileSystemInfo>();
    }
}
