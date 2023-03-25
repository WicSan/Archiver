using Archiver;
using Archiver.Planning;
using Archiver.Util;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NodaTime;
using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows;
using Serilog;
using Microsoft.Extensions.Logging;
using FluentFTP;
using Archiver.Planning.Model;
using Archiver.Planning.Database;
using System.Text.Json.Serialization;
using Archiver.Backup;

namespace Archiver
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private System.Windows.Forms.NotifyIcon _notifyIcon;
        private IHost _host;
        private Microsoft.Extensions.Logging.ILogger _logger;

        public App()
        {
            _notifyIcon = new System.Windows.Forms.NotifyIcon();
            _notifyIcon.Click += NotifyIcon_Click;

            _host = Host.CreateDefaultBuilder()
                .ConfigureServices(services =>
                {
                    ConfigureServices(services);
                })
                .UseSerilog((context, services, loggerConfiguration) => loggerConfiguration
                    .Enrich.FromLogContext()
                    .WriteTo.File("logs/archiver.log"))
                .Build();

            _logger = _host.Services.GetRequiredService<ILogger<App>>();

            var menu = new System.Windows.Forms.ContextMenuStrip();
            var item = new System.Windows.Forms.ToolStripMenuItem("Quit");
            item.Click += NotifyIconMenuQuit_Click;
            menu.Items.Add(item);
            _notifyIcon.ContextMenuStrip = menu;
            _notifyIcon.Text = "No backup in progess";
        }

        private void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<MainWindow>();
            services.AddSingleton<BackupPlanOverview>();

            services.AddSingleton<MainViewModel>();
            services.AddSingleton<IBackupPlanOverviewViewModelFactory, BackupPlanOverviewViewModelFactory>();

            services.AddTransient<JsonConverter, LocalTimeConverter>();
            services.AddTransient<JsonConverter, LocalDateTimeConverter>();
            services.AddTransient<JsonConverter, FileSystemInfoConverter>();
            services.AddTransient<JsonConverter, BackupScheduleConverter>();

            services.AddOptions<JsonDatabaseOptions>().Configure(o => o.FileName = "archiver");
            services.AddSingleton<JsonDatabase>();

            services.AddSingleton<IRepository<BackupPlan>, PlanningRepository>();
            services.AddTransient<BackupServiceFactory>();
            services.AddHostedService<BackupScheduler>();

            services.AddSingleton<IClock>(c => SystemClock.Instance);
            services.AddSingleton(_notifyIcon);

            services.AddSingleton<IProgressService, ProgressService>();

            services.AddTransient<IFtpClientFactory>((services) =>
            {
                var logger = services.GetRequiredService<ILogger<FtpClient>>();
                return new FtpClientFactory(true, logger);
            });
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            ConfigureTaskBarIcon();

            _logger.LogInformation("Archiver started...");

            await _host.RunAsync();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _logger.LogInformation("Archiver shutingdown...");

            base.OnExit(e);
        }

        private void ConfigureTaskBarIcon()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using Stream iconStream = assembly.GetManifestResourceStream("Archiver.Resources.Icon.ico")!;

            _notifyIcon.Icon = new Icon(iconStream);
            _notifyIcon.Visible = true;
        }

        private void NotifyIcon_Click(object? sender, EventArgs e)
        {
            var clickArgs = (System.Windows.Forms.MouseEventArgs)e;
            if (clickArgs.Button != System.Windows.Forms.MouseButtons.Right)
            {
                var window = _host.Services.GetRequiredService<MainWindow>();
                if(window.Visibility != Visibility.Visible)
                {
                    window.Show();
                }
            }
        }

        private void NotifyIconMenuQuit_Click(object? sender, EventArgs e)
        {
            Shutdown();
        }
    }
}
