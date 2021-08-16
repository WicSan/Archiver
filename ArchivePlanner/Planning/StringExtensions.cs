using System.IO;

namespace FastBackup.Planning
{
    public static class StringExtensions
    {
        public static FileSystemInfo ToFileSystemEntry(this string path)
        {
            FileSystemInfo fileSystemEntry;
            if (File.GetAttributes(path).HasFlag(FileAttributes.Directory))
            {
                fileSystemEntry = new DirectoryInfo(path);
            }
            else
            {
                fileSystemEntry = new FileInfo(path);
            }

            return fileSystemEntry;
        }
    }
}
