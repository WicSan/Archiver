using System.IO;

namespace Archiver.Util
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
