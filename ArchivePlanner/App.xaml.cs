using ArchivePlanner;
using ArchivePlanner.Backup;
using ArchivePlanner.Planning;
using ArchivePlanner.Util;
using FastBackup.Planning;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using System.Windows;

namespace FastBackup
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;

        public App()
        {
            ServiceCollection services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(ServiceCollection services)
        {
            services.AddSingleton<MainWindow>();
            services.AddSingleton<BackupPlanOverview>();

            services.AddSingleton<MainViewModel>();
            services.AddSingleton<BackupPlanViewModel>();

            services.AddConfiguredLiteDb();
            services.AddSingleton<PlanningRepository>();

            services.AddHostedService<ArchiverService>();

            services.AddSingleton<IClock, SystemClock>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }
    }
}
