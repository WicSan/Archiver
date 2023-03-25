using Archiver.Planning.Model;
using Archiver.Util;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Archiver.Backup
{
    public class MockBackupService : BackgroundService
    {
        private readonly IRepository<BackupPlan> _repository;
        private readonly IProgressService _service;
        private readonly ILogger<BackupScheduler> _logger;

        public MockBackupService(IRepository<BackupPlan> repository, IProgressService service, ILogger<BackupScheduler> logger)
        {
            _repository = repository;
            _service = service;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var plan = await _repository.GetAllAsync().FirstOrDefaultAsync();

            if (plan is not null)
            {
                Observable.Timer(new TimeSpan(0, 0, 6))
                    .Subscribe(_ =>
                    {
                        _logger.LogInformation("Progress report");
                        _service.ReportProgress(plan.Id, 0.1);

                        Thread.Sleep(2000);

                        _service.ReportProgress(plan.Id, 1);

                        Thread.Sleep(10000);

                        _service.ReportProgress(plan.Id, ProgressService.TaskCompleted);
                    });
            }
        }
    }
}
