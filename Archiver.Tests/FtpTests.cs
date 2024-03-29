﻿using Archiver.Planning;
using Archiver.Planning.Model;
using Archiver.Util;
using FluentFTP;
using System;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Archiver.Tests
{
    public class FtpTests
    {
        private readonly ITestOutputHelper _output;

        public FtpTests(ITestOutputHelper output)
        {
            _output = output;
        }

        /*[Fact]
        public async Task TestFtpClientUpload()
        {
            var fileName = "My Games.tar";
            var token = new CancellationTokenSource().Token;
            var connection = new FtpConnectionDetails("192.168.1.4", "sandro", "");
            var loggerMock = new LoggerMock<FtpClient>(_output);

            using var client = new FtpClientFactory(true, loggerMock).CreateFtpClient(connection);
            await client.ConnectAsync(3, token);

            using var stream = await client.OpenWriteAsync($"Backup\\sandro\\test_{DateTime.Now.ToString("yyyy-MM-dd-HH-mm")}.tar.gz", FtpDataType.Binary, false, token);
            using var limitedStream = new RateLimitedStream(stream, 8000);

            var options = new TarWriterOptions(CompressionType.None, true);
            using (var gzStream = new GZipStream(limitedStream, CompressionMode.Compress, true))
            //using (var writer = new TarWriter(limitedStream, options))
            {
                using var fileStream = File.OpenRead(fileName);

                fileStream.CopyTo(gzStream);
                //fileStream.CopyTo(limitedStream);
                //writer.Write("test", fileStream, null);
                //writer.AddGzipEntry(new FileInfo(fileName));
            }

            client.GetReply();
        }*/

        [Fact]
        public async Task TestConnectAsync()
        {
            var loggerMock = new LoggerMock<FtpClient>(_output);

            using var client = new FtpClientFactory(true, loggerMock).CreateFtpClient(new FtpConnectionDetails { Host="localhost", Username="proftp", Password=new System.Security.SecureString() });
            await Assert.ThrowsAsync<SocketException>(async () => await client.ConnectAsync(3, new CancellationTokenSource().Token));

            Assert.False(client.IsConnected);
        }
    }
}
