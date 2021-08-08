using FastBackup.Operation;
using FastBackup.Operation.Model;
using System.Collections.Generic;
using System.IO;

namespace FastBackup.Planning.Model
{
    public class BackupPlan
    {
        public string Name { get; set; } = null!;

        public BackupStrategy BackupStrategy { get; set; } = new FullBackupStrategy();

        public DirectoryInfo Destination { get; set; } = null!;

        public ICollection<FileSystemInfo> FileSystemItems { get; set; } = new List<FileSystemInfo>();

        public void Run()
        {
            BackupStrategy.Backup(this);
        }
    }
}
