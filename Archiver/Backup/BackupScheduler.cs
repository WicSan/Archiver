using Archiver.Planning.Model;
using Archiver.Util;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NodaTime;
using System;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Archiver.Backup
{
    public class BackupScheduler : BackgroundService
    {
        private readonly IRepository<BackupPlan> _repository;
        private readonly BackupServiceFactory _factory;
        private readonly IClock _clock;
        private readonly ILogger<BackupScheduler> _logger;
        private CancellationToken _externalToken;
        private TaskPoolScheduler _scheduler;

        public BackupScheduler(IRepository<BackupPlan> repository, IClock clock, BackupServiceFactory factory, ILogger<BackupScheduler> logger)
        {
            _repository = repository;
            _clock = clock;
            _factory = factory;
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
                        var service = _factory.CreateService(p);
                        var isSuccessfull = await service.ExecuteAsync(_externalToken);

                        if (isSuccessfull)
                        {
                            p.Schedule.LastExecution = _clock.GetCurrentInstant().ToLocalDateTime();
                            await _repository.UpsertAsync(p, _externalToken);
                        }
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
