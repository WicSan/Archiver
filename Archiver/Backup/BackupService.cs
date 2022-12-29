using ArchivePlanner.Planning;
using ArchivePlanner.Planning.Model;
using ArchivePlanner.Util;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodaTime;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Sockets;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ArchivePlanner.Backup
{
    public class BackupService : BackgroundService
    {
        private readonly IRepository<BackupPlan> _repository;
        private readonly IClock _clock;
        private readonly IProgressService _service;
        private readonly IFtpClientFactory _ftpClientFactory;
        private readonly ILogger<BackupService> _logger;
        private CancellationToken _externalToken;
        private TaskPoolScheduler _scheduler;

        public BackupService(IRepository<BackupPlan> repository, IClock clock, IProgressService service, IFtpClientFactory ftpClientFactory, ILogger<BackupService> logger)
        {
            _repository = repository;
            _clock = clock;
            _service = service;
            _ftpClientFactory = ftpClientFactory;
            _logger = logger;

            var taskFactory = new TaskFactory(new LimitedConcurrencyLevelTaskScheduler(1));
            _scheduler = new TaskPoolScheduler(taskFactory);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _externalToken = stoppingToken;

            Observable.Defer(() =>
            {
                var plans = _repository.GetAllAsync(_externalToken);

                return plans
                    .Select(p => ScheduleBackupJob(p))
                    .ToObservable()
                    .Merge()
                    .SelectMany(async p =>
                    {
                        await ExecutePlanAsync(p);
                        return p;
                    })
                    .ObserveOn(_scheduler);
            })
                .Subscribe(_externalToken);

            return Task.CompletedTask;
        }

        private IObservable<BackupPlan> ScheduleBackupJob(BackupPlan plan)
        {
            return Observable.Return(plan)
                .Merge(_repository.ChangeStream.Where(p => p.Id == plan.Id))
                .Select(p =>
                {
                    return Observable.Create<BackupPlan>(o =>
                    {
                        var now = _clock.GetCurrentInstant().ToLocalDateTime();
                        var next = plan.Schedule.NextExecution(now);
                        var delay = (next - now).ToDuration().ToTimeSpan();

                        _logger.LogInformation("Schedule plan {PlanName} for execution in {NextExecution} ms", plan.Name, next.ToString());

                        var timer = Observable
                            .Timer(TimeSpan.Zero > delay ? TimeSpan.Zero : delay, _scheduler)
                            .Subscribe(_ => o.OnNext(p));

                        return timer;
                    })
                    .Repeat();
                })
                .Switch();
        }

        private async Task ExecutePlanAsync(BackupPlan plan)
        {
            using var client = _ftpClientFactory.CreateFtpClient(plan.Connection);

            _logger.LogInformation("Start backup plan {Name}.", plan.Name);

            try
            {
                await client.ConnectAsync(3, _externalToken);

                var currentNetworkSpeed = MainNetworkInterface.GetSpeedInKB();
                var targetUploadSpeed = Convert.ToInt32(currentNetworkSpeed - currentNetworkSpeed * 0.3);
                var fullName = $"/{plan.FullName}.zip";

                _logger.LogDebug("Plan {Name} started at {StartDateTime} with file name {FullName}", plan.Name, DateTime.Now, fullName);

                using (var uploadStream = await client.OpenWriteAsync(fullName, FluentFTP.FtpDataType.Binary, false, _externalToken))
                using (var limitedStream = new RateLimitedStream(uploadStream, targetUploadSpeed))
                using (var gzipStream = new GZipStream(limitedStream, CompressionLevel.Fastest))
                {
                    var progress = 0.0;
                    var files = GetFilesToBackup(plan).ToList();
                    var totalFiles = files.Count();
                    foreach (var file in files)
                    {
                        if (_externalToken.IsCancellationRequested)
                        {
                            _logger.LogInformation("Backup aborted.");
                            return;
                        }

                        using var fileStream = file.OpenRead();
                        fileStream.CopyTo(gzipStream);

                        progress += 1.0 / totalFiles;
                        _service.ReportProgress(plan.Id, progress);
                    }
                }

                var replay = client.GetReply();
                if (replay.Success)
                {
                    _logger.LogInformation("Backup plan {Name} execution successfull.", plan.Name);

                    plan.Schedule.LastExecution = _clock.GetCurrentInstant().ToLocalDateTime();
                    await _repository.UpsertAsync(plan, _externalToken);
                }
                else
                {
                    _logger.LogError("Failed to upload backup file with error {Error}", replay.ErrorMessage);
                }
            }
            catch (Exception e)
            {
                _logger.LogError("{@Error}", e);
            }
            finally
            {
                plan.ResetProgress();
            }
        }

        private IEnumerable<FileInfo> GetFilesToBackup(BackupPlan plan)
        {
            var options = new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = true
            };

            var singleFiles = plan.FileSystemItems
                .Where(i => i is FileInfo)
                .Cast<FileInfo>();
            var files = plan.FileSystemItems
                .Where(i => i is DirectoryInfo)
                .Cast<DirectoryInfo>()
                .Select(d => Directory.GetFiles(d.FullName, "*", options).Select(f => new FileInfo(f)))
                .SelectMany(l => l)
                .Concat(singleFiles);

            foreach (var file in files)
            {
                yield return file;
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}
