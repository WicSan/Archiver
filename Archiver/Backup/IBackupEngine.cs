using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace Archiver.Backup
{
    public interface IBackupEngine
    {
        IEnumerable<FileInfo> GetFilesToBackup(IEnumerable<ZipArchiveEntry> previousFiles, IEnumerable<FileInfo> newFiles);
    }
}
