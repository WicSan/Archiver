using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ArchivePlanner.Planning.Model
{
    public class FullBackupPlan : BackupPlan
    {
        public override IEnumerable<FileInfo> GetFilesToBackup()
        {
            var options = new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = true
            };

            var singleFiles = FileSystemItems
                .Where(i => i is FileInfo)
                .Cast<FileInfo>();
            var files = FileSystemItems
                .Where(i => i is DirectoryInfo)
                .Cast<DirectoryInfo>()
                .Select(d => Directory.GetFiles(d.FullName, "*", options).Select(f => new FileInfo(f)))
                .SelectMany(l => l)
                .Concat(singleFiles);

            foreach (var file in files)
            {
                yield return file;
            }
        }
    }
}
