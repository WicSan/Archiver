using System.IO;
using System.IO.Compression;

namespace Archiver.Planning
{
    public class ArchivedItemViewModel
    {
        public ArchivedItemViewModel(ZipArchiveEntry entry)
        {
            Name = entry.Name;
            SourcePath = Path.GetDirectoryName(entry.FullName)!;
        }

        public string Name { get; set; }

        public string SourcePath { get; set; }

        public bool IsChecked { get; set; }
    }
}
