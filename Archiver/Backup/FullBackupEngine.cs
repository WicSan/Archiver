using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Archiver.Backup
{
    public class FullBackupEngine : IBackupEngine
    {
        public FullBackupEngine()
        {
        }

        public IEnumerable<FileInfo> GetFilesToBackup(IEnumerable<ZipArchiveEntry> archivedFiles, IEnumerable<FileInfo> newFiles)
        {
            return newFiles;
        }
    }
}
