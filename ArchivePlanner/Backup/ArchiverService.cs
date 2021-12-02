using ArchivePlanner.Planning;
using ArchivePlanner.Planning.Model;
using ArchivePlanner.Util;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodaTime;
using SharpCompress.Common;
using SharpCompress.Writers.Tar;
using System;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;

namespace ArchivePlanner.Backup
{
    public class ArchiverService : BackgroundService
    {
        private System.Timers.Timer? _timer;
        private readonly IPlanningRepository _repository;
        private readonly IClock _clock;
        private readonly ILogger<ArchiverService> _logger;

        public ArchiverService(IPlanningRepository repository, IClock clock, ILogger<ArchiverService> logger)
        {
            _repository = repository;
            _clock = clock;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var plans = _repository.GetAll<BackupPlan>();

            foreach (var plan in plans)
            {
                ScheduleJob(plan, stoppingToken);
            }

            return Task.CompletedTask;
        }

        protected void ScheduleJob(BackupPlan plan, CancellationToken cancellationToken)
        {
            var now = _clock.GetCurrentInstant().ToLocalDateTime();
            var next = plan.CalculateNextExecution(now);
            if (next is not null)
            {
                var delayInMs = (next.Value - now).TotalMilliseconds;
                delayInMs = delayInMs < 0 ? 0 : delayInMs;

                _timer = new System.Timers.Timer(delayInMs);
                _timer.Elapsed += async (sender, args) =>
                {
                    _timer.Dispose();
                    _timer = null!;

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        await ExecutePlanAsync(plan, cancellationToken);
                    }

                    if (!cancellationToken.IsCancellationRequested)
                    {
                        ScheduleJob(plan, cancellationToken);
                    }
                };
                _timer.Start();
            }
        }

        protected Task<BackupPlan> ExecutePlanAsync(BackupPlan plan, CancellationToken stoppingToken)
        {
            var options = new TarWriterOptions(CompressionType.None, false);

            var certificate = X509Certificate.CreateFromCertFile("ftp.crt");
            var credentials = new NetworkCredential("sandro", "");
            var server = new FtpConnection(new Uri("ftp://192.168.1.4"), certificate, credentials);

            _logger.LogInformation("Start backup plans.");

            var backupFileName = $"{plan.UniqueName}.gz.tar";
            using var uploadStream = server.OpenUploadStream($"Backup/sandro/{backupFileName}");
            using var writer = new TarWriter(uploadStream, options);

            _logger.LogDebug("Plan {Name} started at {StartDateTime} with file name {FileName}", plan.Name, DateTime.Now, backupFileName);

            if(stoppingToken.IsCancellationRequested)
            {
                return (Task<BackupPlan>)Task.FromCanceled(stoppingToken);
            }

            foreach (var file in plan.GetFilesToBackup())
            {
                writer.AddGzipEntry(file);
            }

            _logger.LogInformation("Backup plans execution successfull.");

            plan.LastExecution = _clock.GetCurrentInstant().ToLocalDateTime();
            return Task.FromResult(plan);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);

            _timer?.Stop();
        }

        public override void Dispose()
        {
            base.Dispose();

            _timer?.Dispose();
        }
    }
}
