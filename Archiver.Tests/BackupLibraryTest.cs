using ICSharpCode.SharpZipLib.Tar;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace FastBackup.Tests
{
    public class BackupLibraryTest
    {
        [Fact]
        public void TestArchiveCreationZip()
        {
            using (FileStream zipToOpen = new FileStream(@"../../../target/release.zip", FileMode.OpenOrCreate))
            using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
            {
                archive.CreateEntryFromFile(@"D:\data\projects\general\FastBackup\FastBackup.Tests\BackupLibraryTest.cs", "BackupLibraryTest.cs", CompressionLevel.Fastest);
            }
        }

        [Fact]
        public void TestCreationGZip()
        {
            using (FileStream zip = new FileStream(@"../../../target/release.gz", FileMode.OpenOrCreate))
            using (GZipStream stream = new GZipStream(zip, CompressionMode.Compress))
            {
                using var file = File.Open(@"D:\data\projects\general\FastBackup\FastBackup.Tests\BackupLibraryTest.cs", FileMode.Open);
                file.CopyTo(stream);
            }
        }


        [Fact]
        public void TestCreationTar()
        {
            using var fileStream = File.Open(@"D:\data\projects\general\FastBackup\FastBackup.Tests\BackupLibraryTest.cs", FileMode.Open);
            using var tarStream = new FileStream(@"../../../target/release.tar", FileMode.OpenOrCreate);
            using var s = new TarOutputStream(tarStream, Encoding.ASCII);
            var entry = TarEntry.CreateTarEntry(@"\D\data\projects\general\FastBackup\FastBackup.Tests\BackupLibraryTest.cs");
            entry.Size = fileStream.Length;

            s.PutNextEntry(entry);
            fileStream.CopyTo(s);
            s.CloseEntry();
        }

        [Fact]
        public void TestArchiveCreationTar()
        {
            using var tarStream = new FileStream(@"../../../target/release.gz.tar", FileMode.OpenOrCreate);
            using var s = new TarOutputStream(tarStream, Encoding.UTF8);

            var d = @"C:\Users\sandr\Documents";
            var d2 = @"C:\Users\sandr\Downloads";

            var options = new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = true
            };
            var zippedFiles = Directory.EnumerateFiles(d, "*", options)
                .Concat(Directory.EnumerateFiles(d2, "*", options))
                .AsParallel()
                .WithDegreeOfParallelism(5)
                .Select(CreateGzippedEntry);

            foreach (var zippedFile in zippedFiles)
            {
                s.PutNextEntry(zippedFile.Key);
                s.Write(zippedFile.Value, 0, zippedFile.Value.Length);
                s.CloseEntry();
            }
        }


        private KeyValuePair<TarEntry, byte[]> CreateGzippedEntry(string path)
        {
            MemoryStream compressedFileStream = new MemoryStream();
            using (var compressionStream = new GZipStream(compressedFileStream, CompressionMode.Compress, true))
            {
                using var file = File.OpenRead(path);
                file.CopyTo(compressionStream);
            }
            compressedFileStream.Position = 0;

            var entry = TarEntry.CreateTarEntry($"{path.Replace(Path.VolumeSeparatorChar.ToString(), string.Empty)}.gz");
            entry.Size = compressedFileStream.Length;

            var mem = new byte[compressedFileStream.Length];
            compressedFileStream.Read(mem);
            return new KeyValuePair<TarEntry, byte[]>(entry, mem.ToArray());
        }
    }
}
