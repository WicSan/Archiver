using ArchivePlanner;
using ArchivePlanner.Backup;
using ArchivePlanner.Planning;
using ArchivePlanner.Planning.Model;
using ArchivePlanner.Util;
using Moq;
using System.Collections.Generic;
using System.Windows.Automation.Peers;

namespace Archiver.Tests
{
    public class WpfTests
    {
        [STAFact]
        public void ProgressAnimantionOnBackupStart()
        {
            var ftpFactoryMock = new Mock<IFtpClientFactory>();
            var factoryMock = new Mock<IBackupPlanOverviewViewModelFactory>();
            var viewModel = new BackupPlanOverviewViewModel(ftpFactoryMock.Object, new ProgressService());
            var repositoryMock = new Mock<IRepository<BackupPlan>>();

            factoryMock.Setup(f => f.CreateModel(It.IsAny<BackupPlan>())).Returns(viewModel);

            var mainViewModel = new MainViewModel(factoryMock.Object, repositoryMock.Object);
            var window = new MainWindow(mainViewModel, new LoggerMock<MainWindow>());
            WindowAutomationPeer windowPeer = new WindowAutomationPeer(window);
            List<AutomationPeer> children = windowPeer.GetChildren();
        }
    }
}
