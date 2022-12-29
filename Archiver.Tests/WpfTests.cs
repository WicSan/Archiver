using Archiver;
using Archiver.Backup;
using Archiver.Planning;
using Archiver.Planning.Model;
using Archiver.Util;
using Moq;
using NodaTime;
using NodaTime.Testing;
using System;
using System.Collections.Generic;
using System.Windows.Automation.Peers;

namespace Archiver.Tests
{
    public class WpfTests
    {
        [STAFact]
        public void ProgressAnimantionOnBackupStart()
        {
            var fakeClock = new FakeClock(Instant.FromDateTimeOffset(new DateTimeOffset(2021, 11, 1, 7, 59, 59, TimeSpan.FromHours(1))));
            var ftpFactoryMock = new Mock<IFtpClientFactory>();
            var factoryMock = new Mock<IBackupPlanOverviewViewModelFactory>();
            var viewModel = new BackupPlanOverviewViewModel(ftpFactoryMock.Object, new ProgressService(), fakeClock, new BackupPlan());
            var repositoryMock = new Mock<IRepository<BackupPlan>>();

            factoryMock.Setup(f => f.CreateModel(It.IsAny<BackupPlan>())).Returns(viewModel);

            var mainViewModel = new MainViewModel(factoryMock.Object, repositoryMock.Object);
            var window = new MainWindow(mainViewModel, new LoggerMock<MainWindow>());
            WindowAutomationPeer windowPeer = new WindowAutomationPeer(window);
            List<AutomationPeer> children = windowPeer.GetChildren();
        }
    }
}
