using ArchivePlanner;
using ArchivePlanner.Backup;
using ArchivePlanner.Planning;
using ArchivePlanner.Planning.Model;
using LiteDB;
using Microsoft.Extensions.Logging;
using Moq;
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
            var plan = new FullBackupPlan()
            {
                Name = "test",
                Destination = new DirectoryInfo(Directory.GetCurrentDirectory()),
                FileSystemItems = new List<FileSystemInfo>
                {
                    new DirectoryInfo(@"C:\Users\sandr\Documents\My Games\Dawn of War 2\Logfiles"),
                },
            };

            var repositoryMock = new Mock<IPlanningRepository>();
            repositoryMock.Setup(r => r.GetAll<BackupPlan>()).Returns(() => new List<BackupPlan> { plan });
            var subject = new ArchiverService(repositoryMock.Object, new Mock<ILogger<ArchiverService>>().Object);
            CancellationTokenSource source = new CancellationTokenSource();

            await subject.StartAsync(source.Token);
        }
    }
}
