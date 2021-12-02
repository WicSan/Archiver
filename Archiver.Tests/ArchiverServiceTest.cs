using ArchivePlanner;
using ArchivePlanner.Backup;
using ArchivePlanner.Planning;
using ArchivePlanner.Planning.Model;
using LiteDB;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using NodaTime.Testing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Archiver.Tests
{
    public class ArchiverServiceTest
    {
        [Fact]
        public async Task TestArchiverService()
        {
            // use dotnet memory (jetbrains)
            var plan = new FullBackupPlan()
            {
                Name = "test",
                Destination = new DirectoryInfo(Directory.GetCurrentDirectory()),
                ExecutionDays = new[] { IsoDayOfWeek.Monday },
                ExecutionTime = new LocalTime(8, 00),
                FileSystemItems = new List<FileSystemInfo>
                {
                    new DirectoryInfo(@"C:\Users\sandr\Documents\My Games\Age of Empires 3"),
                },
            };

            var repositoryMock = new Mock<IPlanningRepository>();
            repositoryMock.Setup(r => r.GetAll<BackupPlan>()).Returns(() => new List<BackupPlan> { plan });
            var fakeClock = new FakeClock(Instant.FromDateTimeOffset(new DateTimeOffset(2021, 11, 1, 7, 59, 59, TimeSpan.FromHours(1))));
            var subject = new ArchiverService(repositoryMock.Object, fakeClock, new Mock<ILogger<ArchiverService>>().Object);
            CancellationTokenSource source = new CancellationTokenSource();

            await subject.StartAsync(source.Token);

            Thread.Sleep(2000);
        }
    }
}
