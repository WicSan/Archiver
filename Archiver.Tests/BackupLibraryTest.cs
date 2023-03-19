using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using Xunit;

namespace Archiver.Tests
{
    public class BackupLibraryTest
    {
        [Fact]
        public void TestArchiveCreationZip()
        {
            using (var zipStream = new MemoryStream())
            using (ZipArchive archive = new ZipArchive(zipStream, ZipArchiveMode.Update))
            {
                archive.CreateEntry("test");
                archive.CreateEntryFromFile(@"D:\data\projects\general\Archiver\Archiver.Tests\BackupLibraryTest.cs", "BackupLibraryTest.cs", CompressionLevel.Fastest);
            }
        }

        [Fact]
        public void TestArchiveCreationZip2()
        {
            using var zipStream = File.OpenWrite("test.zip");
            var archive = new ZipArchive(zipStream, ZipArchiveMode.Create);

            var entry = archive.CreateEntry("test", CompressionLevel.NoCompression);

            using var fileStream = new StreamReader(File.Open(@"D:\data\projects\general\Archiver\Archiver.Tests\BackupLibraryTest.cs", FileMode.Open));
            var entryStream = new StreamWriter(entry.Open());
            entryStream.Write(fileStream.ReadToEnd().Substring(0, 40));
            entryStream.Flush();
        }

        [Fact]
        public void TestCreationGZip()
        {
            var proc = Process.GetCurrentProcess();

            using (var zip = new MemoryStream())
            using (GZipStream stream = new GZipStream(zip, CompressionMode.Compress))
            {
                var m1 = GC.GetTotalMemory(false);

                using var file = File.Open(@"D:\downloads\ubuntu-20.04.2.0-desktop-amd64.iso", FileMode.Open);
                file.CopyTo(stream);

                var m2 = GC.GetTotalMemory(false);
                var diff = m2 - m1;
            }
        }


        /*[Fact]
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
        }

        [Fact]
        public void TestCreationGzTar()
        {
            var options = new TarWriterOptions(SharpCompress.Common.CompressionType.None, false);
            using var tarStream = new FileStream(@"C:\Users\sandr\Documents\test.gz.tar", FileMode.Create);

            var files = Directory.EnumerateFiles(@"C:\Users\sandr\Documents\My Games", "*.*", new EnumerationOptions { IgnoreInaccessible = true, RecurseSubdirectories = true });

            using (var writer = new TarWriter(tarStream, options))
            {
                foreach (var file in files.Select(f => new FileInfo(f)))
                {
                    using (var entryStream = file.OpenRead())
                    {
                        var tempfile = new FileInfo(Path.GetTempFileName());
                        using var tempStream = tempfile.OpenWrite();

                        using (var gzStream = new GZipStream(tempStream, CompressionMode.Compress, true))
                        {
                            entryStream.CopyTo(gzStream);
                        }

                        tempStream.Seek(0, SeekOrigin.Begin);
                        writer.Write(file.FullName, tempStream, file.LastWriteTime, tempStream.Length);
                        tempfile.Delete();
                    }
                }
            }
        }


        [Fact]
        public void TestCreationGzTarZipLib()
        {
            var options = new TarWriterOptions(SharpCompress.Common.CompressionType.None, false);
            using var fileStream = new FileStream(@"C:\Users\sandr\Documents\test.gz.tar", FileMode.Create);

            var files = Directory.EnumerateFiles(@"C:\Users\sandr\Documents\My Games", "*.*", new EnumerationOptions { IgnoreInaccessible = true, RecurseSubdirectories = true });

            using (var tarStream = new TarOutputStream(fileStream, Encoding.UTF8))
            {
                foreach (var file in files.Select(f => new FileInfo(f)))
                {
                    using (var entryStream = file.OpenRead())
                    {
                        var entry = TarEntry.CreateEntryFromFile(file.FullName);
                        tarStream.PutNextEntry(entry);

                        using (var gzStream = new GZipStream(tarStream, CompressionMode.Compress, true))
                        {
                            entryStream.CopyTo(gzStream);
                        }
                    }
                }
            }
        }*/

        /*[Fact]
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
        }*/
    }
}
