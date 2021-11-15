using ArchivePlanner.Util;
using SharpCompress.Archives.GZip;
using SharpCompress.Archives.Tar;
using SharpCompress.Writers.Tar;
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
            using (FileStream zip = new FileStream(@"../../../target/release.gz", FileMode.Create))
            using (GZipStream stream = new GZipStream(zip, CompressionMode.Compress))
            {
                using var file = File.Open(@"C:\Users\sandr\Documents\My Games\Dawn of War 2\Settings\_keydefaults.lua", FileMode.Open);
                file.CopyTo(stream);
            }
        }


        [Fact]
        public void TestCreationTar()
        {
            var options = new TarWriterOptions(SharpCompress.Common.CompressionType.None, false);
            using var tarStream = new FileStream(@"../../../target/release.tar", FileMode.Create);

            using var stream1 = new MemoryStream();
            using (var writer = new TarWriter(stream1, options))
            {
                var file = new FileInfo(@"C:\Users\sandr\Documents\My Games\Dawn of War 2\Settings\_keydefaults.lua");

                using (var entryStream = file.OpenRead())
                {
                    writer.Write(file.FullName, entryStream, file.LastWriteTime, file.Length);
                }
            }
            stream1.Seek(0, SeekOrigin.Begin);
            stream1.CopyTo(tarStream);


            using var stream2 = new MemoryStream();
            using (var writer2 = new TarWriter(stream2, options))
            {
                var file1 = new FileInfo(@"C:\Users\sandr\Documents\They Are Billions\Configuration.txt");

                using (var entryStream = file1.OpenRead())
                {
                    writer2.Write(file1.FullName, entryStream, file1.LastWriteTime, file1.Length);
                }
            }
            stream2.Seek(0, SeekOrigin.Begin);
            stream2.CopyTo(tarStream);
        }

        [Fact]
        public void TestCreationTarWithLibraryBug()
        {
            using var tarStream = new MemoryStream();

            using var stream1 = new MemoryStream();
            using var archive1 = TarArchive.Open(stream1);

            using var file1 = new MemoryStream(Encoding.UTF8.GetBytes("Lorem ipsum dolor sit amet, consetetur sadipscing elitr, sed diam nonumy eirmod tem"));
            archive1.AddEntry("test", file1, file1.Length);
            archive1.SaveToWithoutEndBlocks(tarStream);

            using var stream2 = new MemoryStream();
            using var archive2 = TarArchive.Open(stream1);

            using var file2 = new MemoryStream(Encoding.UTF8.GetBytes("Lorem ipsum dolor sit amet, consetetur sadipscing hhm"));

            archive2.AddEntry("tset2", file2, file2.Length);
            archive2.SaveToWithoutEndBlocks(tarStream);

            var blockSize = 512;
            Assert.Equal(4 * blockSize, tarStream.Length);
        }
    }
}
