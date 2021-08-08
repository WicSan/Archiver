using FastBackup.Operation.Model;
using FastBackup.Planning.Model;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace FastBackup.Tests
{
    public class FullBackupStrategyTest
    {
        [Fact]
        public void WhenStrategyIsExecuted_ItemsAreMoved()
        {
            var plan = new WeeklyBackupPlan()
            {
                Name = "test",
                Destination = new DirectoryInfo(Directory.GetCurrentDirectory()),
                FileSystemItems = new List<FileSystemInfo>
                {
                    new DirectoryInfo(@"C:\Users\sandr\Documents"),
                    new DirectoryInfo(@"C:\Users\sandr\Downloads"),
                },
            };

            var strategy = new FullBackupStrategy();
            strategy.Backup(plan);
        }
    }
}
