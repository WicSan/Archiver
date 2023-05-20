using Archiver.Planning;
using Archiver.Planning.Model;
using Archiver.Util;
using Microsoft.Extensions.Logging;
using NodaTime;
using System;

namespace Archiver.Backup
{
    public class BackupServiceFactory
    {
        private IProgressService _service;
        private IFtpClientFactory _ftpClientFactory;
        private readonly IRepository<BackupPlan> _repository;
        private readonly IClock _clock;
        private ILogger<BaseBackupService> _logger;

        public BackupServiceFactory(IProgressService service, IFtpClientFactory ftpClientFactory, IRepository<BackupPlan> repository, IClock clock, ILogger<BaseBackupService> logger)
        {
            _service = service;
            _ftpClientFactory = ftpClientFactory;
            _repository = repository;
            _clock = clock;
            _logger = logger;
        }

        public IBackupService CreateService(BackupPlan plan)
        {
            switch (plan.BackupType)
            {
                case BackupType.Differential:
                    return new DifferantialBackupService(plan, _service, _ftpClientFactory, _repository, _clock, _logger);
                case BackupType.Full:
                    return new FullBackupService(plan, _service, _ftpClientFactory, _repository, _clock, _logger);
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
