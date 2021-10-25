using System.IO;

namespace ArchivePlanner.Util
{
    public static class FileInfoExtension
    {
        public static DriveInfo? DriveInfo(this FileInfo file)
        {
            if (file.Directory is null)
            {
                return null;
            }

            return new DriveInfo(file.Directory.Root.FullName);
        }
    }
}
