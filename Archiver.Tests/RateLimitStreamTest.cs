using ArchivePlanner.Util;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Archiver.Tests
{
    public class RateLimitStreamTest
    {
        private readonly ITestOutputHelper _output;

        public RateLimitStreamTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task TestRateLimitWhenWriting()
        {
            using var mStream = new MemoryStream();
            var stopwatch = new Stopwatch();

            var limitStream = new RateLimitedStream(mStream, 1000);

            var range = Enumerable.Range(0, 1000);
            var buffer = new byte[50000];

            Random rnd = new Random();
            rnd.NextBytes(buffer);

            stopwatch.Start();
            foreach (var r in range)
            {
                await limitStream.WriteAsync(buffer, 0, buffer.Length);
            }
            stopwatch.Stop();
            var limitedRate = buffer.Length * range.Count() / ((stopwatch.ElapsedMilliseconds + 0.00001) / 1000) / 1024;

            mStream.Seek(0, SeekOrigin.Begin);
            stopwatch.Restart();
            foreach (var r in range)
            {
                await mStream.WriteAsync(buffer, 0, buffer.Length);
            }
            stopwatch.Stop();
            var rate = buffer.Length * range.Count() / ((stopwatch.ElapsedMilliseconds + 0.00001) / 1000) / 1024;

            _output.WriteLine("Limited rate {0}", limitedRate);
            _output.WriteLine("Rate {0}", rate);
            Assert.True(rate > limitedRate);
        }
    }
}
