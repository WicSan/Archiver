using Archiver.Planning;
using Archiver.Planning.Model;
using Microsoft.Extensions.Logging;
using NodaTime;
using System;

namespace Archiver.Backup
{
    public class BackupServiceFactory
    {
        private IProgressService _service;
        private IFtpClientFactory _ftpClientFactory;
        private ILogger<BaseBackupService> _logger;

        public BackupServiceFactory(IProgressService service, IFtpClientFactory ftpClientFactory, ILogger<BaseBackupService> logger)
        {
            _service = service;
            _ftpClientFactory = ftpClientFactory;
            _logger = logger;
        }

        public IBackupService CreateService(BackupPlan plan)
        {
            switch (plan.BackupType)
            {
                case BackupType.Differential:
                    return new DifferantialBackupService(plan, _service, _ftpClientFactory, _logger);
                case BackupType.Full:
                    return new FullBackupService(plan, _service, _ftpClientFactory, _logger);
                default:
                    throw new NotSupportedException();
            }
        }
    }
}
