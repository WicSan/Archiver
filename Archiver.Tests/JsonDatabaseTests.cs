using Archiver.Planning.Database;
using Archiver.Planning.Model;
using Microsoft.Extensions.Options;
using Moq;
using NodaTime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Serialization;
using Xunit;

namespace Archiver.Tests
{
    public class JsonDatabaseTests
    {
        [Fact]
        public void TestJsonDatabaseRead()
        {
            var converters = new List<JsonConverter>()
            {
                new LocalTimeConverter(),
                new LocalDateTimeConverter(),
                new FileSystemInfoConverter(),
                new BackupScheduleConverter(),
            };
            var optionsMock = new Mock<IOptions<JsonDatabaseOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new JsonDatabaseOptions() { FileName = "test" });

            var subject = new JsonDatabase(optionsMock.Object, converters);


            var plan = new BackupPlan()
            {
                Name = "Local backup",
                DestinationFolder = "/Backup/sandro",
                Schedule = new WeeklyBackupSchedule(new LocalTime(8, 00), new HashSet<IsoDayOfWeek> { IsoDayOfWeek.Monday }) { LastExecution = new LocalDateTime() },
                Connection = new FtpConnection("192.168.1.4", "sandro", GetEncryptedBase64EncodedString("test")),
                FileSystemItems = new List<FileSystemInfo>
                {
                    new FileInfo("test")
                },
            };

            subject.Upsert(plan);

            var plans = subject.FindAll<BackupPlan>();
            Assert.NotEmpty(plans);
        }

        [Fact]
        public void TestEntityUpsert()
        {
            var converters = new List<JsonConverter>()
            {
                new LocalTimeConverter(),
                new LocalDateTimeConverter(),
                new FileSystemInfoConverter(),
                new BackupScheduleConverter(),
            };
            var optionsMock = new Mock<IOptions<JsonDatabaseOptions>>();
            optionsMock.Setup(o => o.Value).Returns(new JsonDatabaseOptions() { FileName = "test" });

            var subject = new JsonDatabase(optionsMock.Object, converters);

            var plan = new BackupPlan()
            {
                Id = Guid.NewGuid(),
                Name = "test",
                DestinationFolder = "Backup/sandro",
                Connection = new FtpConnection("192.168.1.4", "sandro", GetEncryptedBase64EncodedString("test")),
                Schedule = new WeeklyBackupSchedule(new LocalTime(8, 0), new List<IsoDayOfWeek> { IsoDayOfWeek.Monday }),
                FileSystemItems = new List<FileSystemInfo>
                {
                    new DirectoryInfo(@"C:\Users\sandr\Documents\They Are Billions\Saves"),
                },
            };
            subject.Upsert(plan);

            plan.Id = Guid.NewGuid();
            subject.Upsert(plan);

            subject.Upsert(plan);

            var savedEntities = subject.FindAll<BackupPlan>();

            Assert.Equal(2, savedEntities.Count());
        }

        private string GetEncryptedBase64EncodedString(string value)
        {
            var entropy = System.Text.Encoding.Unicode.GetBytes("jo3ldkKI22!");
            var encryptedData = System.Security.Cryptography.ProtectedData.Protect(
                                    System.Text.Encoding.Unicode.GetBytes(value),
                                    entropy,
                                    System.Security.Cryptography.DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(encryptedData);
        }
    }
}
