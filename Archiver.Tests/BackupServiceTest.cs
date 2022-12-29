using ArchivePlanner.Backup;
using ArchivePlanner.Planning;
using ArchivePlanner.Planning.Model;
using ArchivePlanner.Util;
using FluentFTP;
using Microsoft.Extensions.Logging;
using Moq;
using NodaTime;
using NodaTime.Testing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Archiver.Tests
{
    public class BackupServiceTest
    {
        private readonly ITestOutputHelper _output;

        public BackupServiceTest(ITestOutputHelper output)
        {
            _output = output;
        }

        [Fact]
        public async Task TestBackupService()
        {
            // Monday
            var fakeClock = new FakeClock(Instant.FromDateTimeOffset(new DateTimeOffset(2021, 11, 1, 7, 59, 59, TimeSpan.FromHours(1))));

            // use dotnet memory (jetbrains)
            var plan = new BackupPlan()
            {
                Name = "test",
                DestinationFolder = "Backup/sandro",
                Connection = new FtpConnection("192.168.1.4", "sandro",
                    ""),
                Schedule = new WeeklyBackupSchedule(new LocalTime(8, 0), new List<IsoDayOfWeek> { IsoDayOfWeek.Monday }),
                FileSystemItems = new List<FileSystemInfo>
                {
                    new DirectoryInfo(@"C:\Users\sandr\Documents\They Are Billions\Saves"),
                },
            };

            var repositoryMock = new Mock<IRepository<BackupPlan>>();
            repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).Returns(GetBackupPlans(new List<BackupPlan> { plan }));
            repositoryMock.Setup(r => r.ChangeStream).Returns(Observable.Empty<BackupPlan>());

            var updateObserver = new BehaviorSubject<Unit>(Unit.Default);
            repositoryMock.Setup(r => r.UpsertAsync(It.IsAny<BackupPlan>(), It.IsAny<CancellationToken>())).Callback(() => updateObserver.OnCompleted());
            var loggerMock = new LoggerMock<BackupService>(_output);

            var subject = new BackupService(
                repositoryMock.Object,
                fakeClock,
                new Mock<IProgressService>().Object,
                new FtpClientFactory(false, new Mock<ILogger<FtpClient>>().Object),
                loggerMock);

            await subject.StartAsync(new CancellationTokenSource().Token);

            updateObserver.Timeout(TimeSpan.FromMinutes(20)).Wait();
        }

        [Fact]
        public async Task TestBackupServicePerformance()
        {
            /*Process process = new Process();
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "cd ../syswow64 && lodctr /r";
            process.StartInfo = startInfo;
            process.Start();*/

            PerformanceCounter cpuCounter;

            // use dotnet memory (jetbrains)
            var plan = new BackupPlan()
            {
                Name = "test",
                DestinationFolder = "",
                Schedule = new WeeklyBackupSchedule(new LocalTime(8, 00), new List<IsoDayOfWeek> { IsoDayOfWeek.Monday }),
                FileSystemItems = new List<FileSystemInfo>
                {
                    new DirectoryInfo(@"C:\Users\sandr\Documents\My Games\Age of Empires 3"),
                },
            };

            var repositoryMock = new Mock<IRepository<BackupPlan>>();
            repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).Returns(() => new List<BackupPlan> { plan });
            var fakeClock = new FakeClock(Instant.FromDateTimeOffset(new DateTimeOffset(2021, 11, 1, 7, 59, 59, TimeSpan.FromHours(1))));
            var subject = new BackupService(
                repositoryMock.Object, fakeClock,
                new Mock<ProgressService>().Object,
                new FtpClientFactory(false, new Mock<ILogger<FtpClient>>().Object),
                new Mock<ILogger<BackupService>>().Object);
            CancellationTokenSource source = new CancellationTokenSource();

            await subject.StartAsync(source.Token);

            cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            _output.WriteLine($"{cpuCounter.NextValue()} %");
            Thread.Sleep(2000);
        }

        [Fact]
        public void TestBackupServiceNicCheck()
        {
            var nics = NetworkInterface
                .GetAllNetworkInterfaces()
                .Where(nic => nic.NetworkInterfaceType != NetworkInterfaceType.Loopback
                      && nic.NetworkInterfaceType != NetworkInterfaceType.Tunnel
                      && nic.OperationalStatus == OperationalStatus.Up);
            var ipProps = nics.Select(nic =>
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"root/CIMV2",
                            string.Format("SELECT * FROM Win32_NetworkAdapter WHERE GUID='{0}'", nic.Id));
                return searcher.Get();
            });
            var speedsInMB = nics.Select(nic => Math.Round(nic.Speed / (1024.0 * 1024.0 * 8.0), 2));
        }

        [Fact]
        public void TestTimerScheduling()
        {
            Enumerable.Range(1, 2)
                .Select(i =>
                {
                    return Schedule(i);
                })
                .Merge()
                .SubscribeOn(NewThreadScheduler.Default)
                .Subscribe(v =>
                {
                    _output.WriteLine($"Start {v} {DateTime.Now}");
                    _output.WriteLine($"Stop {v}");
                });

            Thread.Sleep(10000);
        }

        private IObservable<int> Schedule(int i)
        {
            return Observable.Create<int>(o =>
            {
                var timer = new System.Timers.Timer(i * 900);
                timer.Elapsed += (_, _) =>
                {
                    o.OnNext(i);

                    timer.Interval = i * 900;
                    timer.Start();
                };

                timer.AutoReset = false;
                timer.Start();
                return timer;
            });
        }

        private async IAsyncEnumerable<BackupPlan> GetBackupPlans(IEnumerable<BackupPlan> plans)
        {
            foreach(BackupPlan plan in plans)
            {
                yield return plan;
            }

            await Task.CompletedTask;
        }
    }
}
