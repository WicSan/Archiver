using ArchivePlanner.Planning.Model;
using ArchivePlanner.Util;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ArchivePlanner.Backup
{
    public class MockBackupService : BackgroundService
    {
        private readonly IRepository<BackupPlan> _repository;
        private readonly IProgressService _service;
        private readonly ILogger<BackupService> _logger;

        public MockBackupService(IRepository<BackupPlan> repository, IProgressService service, ILogger<BackupService> logger)
        {
            _repository = repository;
            _service = service;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var plan = await _repository.GetAllAsync().FirstOrDefaultAsync();
            var progress = 0;

            if(plan is not null)
            {
                Observable.Interval(new TimeSpan(0, 0, 2))
                    .Subscribe(_ =>
                    {
                        _logger.LogInformation("Progress report");
                        _service.ReportProgress(plan.Id, ++progress);
                    });
            }
        }
    }
}
