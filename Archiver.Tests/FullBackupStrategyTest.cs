using ArchivePlanner.Planning.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace Archiver.Tests
{
    public class FullBackupStrategyTest
    {
        [Fact]
        public void WhenStrategyIsExecuted_ItemsAreMoved()
        {
            var plan = new FullBackupPlan()
            {
                Name = "test",
                Destination = new DirectoryInfo(Directory.GetCurrentDirectory()),
                FileSystemItems = new List<FileSystemInfo>
                {
                    new DirectoryInfo(@"C:\Users\sandr\Documents"),
                    new DirectoryInfo(@"D:\Downloads"),
                },
            };

            var files = plan.GetFilesToBackup().ToList();

            Assert.NotEmpty(files);
        }
    }
}
