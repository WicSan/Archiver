using ArchivePlanner.Backup;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Xunit;

namespace Archiver.Tests
{
    public class FtpTest
    {
        [Fact]
        public void TestFTPConnectionRead()
        {
            // Get the object used to communicate with the server.
            var certificate = X509Certificate.CreateFromCertFile("ftp.crt");
            var credentials = new NetworkCredential("sandro", "Q8nR`ccw;x");
            var server = new FtpConnection(new Uri("ftp://192.168.1.4"), certificate, credentials);

            using var stream = server.OpenUploadStream("/Backup/sandro/test_2021-10-31-18-21-54.gz");
            using var gzipstream = new GZipStream(stream, CompressionMode.Compress);
            using var file = File.Open(@"D:\downloads\ubuntu-20.04.2.0-desktop-amd64.iso", FileMode.Open);

            file.CopyTo(gzipstream);
        }

        [Fact]
        public void TestFTPUpload()
        {
            var certificate = X509Certificate.CreateFromCertFile("ftp.crt");
            var credentials = new NetworkCredential("sandro", "Q8nR`ccw;x");
            var server = new FtpConnection(new Uri("ftp://192.168.1.4"), certificate, credentials);

            server.UploadFileToFolder("/Backup/sandro", new FileInfo("test_2021-10-31-18-21-54.tar.gz"));
        }
    }
}
