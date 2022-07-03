using ArchivePlanner.Planning.Database;
using ArchivePlanner.Planning.Model;
using NodaTime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization.Metadata;
using Xunit;

namespace Archiver.Tests
{
    public class DbTest
    {
        [Fact]
        public void TestJsonDatabase()
        {
            var entropy = System.Text.Encoding.Unicode.GetBytes("jo3ldkKI22!");
            byte[] encryptedData = System.Security.Cryptography.ProtectedData.Protect(
                                    System.Text.Encoding.Unicode.GetBytes("test"),
                                    entropy,
                                    System.Security.Cryptography.DataProtectionScope.CurrentUser);
            var database = new JsonDatabase("test.jdb");
            var plan = new BackupPlan()
            {
                Name = "Local backup",
                DestinationFolder = "/Backup/sandro",
                Schedule = new WeeklyBackupSchedule(new LocalTime(8, 00), new HashSet<IsoDayOfWeek> { IsoDayOfWeek.Monday }) { LastExecution = new LocalDateTime() },
                Connection = new FtpConnection("192.168.1.4", "sandro", Convert.ToBase64String(encryptedData)),
                FileSystemItems = new List<FileSystemInfo>
                {
                    new FileInfo("test")
                },
            };

            database.Upsert(plan);
        }

        [Fact]
        public void TestJsonDatabaseRead()
        {
            var entropy = System.Text.Encoding.Unicode.GetBytes("jo3ldkKI22!");
            byte[] encryptedData = System.Security.Cryptography.ProtectedData.Protect(
                                    System.Text.Encoding.Unicode.GetBytes("test"),
                                    entropy,
                                    System.Security.Cryptography.DataProtectionScope.CurrentUser);
            var database = new JsonDatabase("test.jdb");
            var plan = new BackupPlan()
            {
                Name = "Local backup",
                DestinationFolder = "/Backup/sandro",
                Schedule = new WeeklyBackupSchedule(new LocalTime(8, 00), new HashSet<IsoDayOfWeek> { IsoDayOfWeek.Monday }) { LastExecution = new LocalDateTime() },
                Connection = new FtpConnection("192.168.1.4", "sandro", Convert.ToBase64String(encryptedData)),
                FileSystemItems = new List<FileSystemInfo>
                {
                    new FileInfo("test")
                },
            };

            database.Upsert(plan);

            var plans = database.FindAll<BackupPlan>();
            Assert.NotEmpty(plans);
        }
    }
}
