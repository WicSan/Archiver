using System.IO;

namespace ArchivePlanner.Util
{
    public static class DirectoryInfoExtensions
    {
        public static DriveInfo DriveInfo(this DirectoryInfo directory)
        {
            return new DriveInfo(directory.Root.FullName);
        }
    }
}
