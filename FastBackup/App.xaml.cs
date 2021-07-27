using FastBackup.Plans;
using FastBackup.Util;
using LiteDB;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
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
            BsonMapper.Global.RegisterType(
                info => info.FullName,
                bson => new DirectoryInfo(bson));

            BsonMapper.Global.RegisterType(
                info => info.FullName,
                bson =>
                    bson.AsString.ToFileSystemEntry());

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

            services.Configure<LiteDbOptions>((s) => s.DbName = "backup");
            services.AddSingleton<IRepository, Repository>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }
    }
}
