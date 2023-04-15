using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using Archiver.Planning.Model;
using Archiver.Util;
using Archiver.Shared;
using FluentFTP;
using NodaTime;
using Archiver.Backup;
using Archiver.Backup.Model;
using System.Threading;
using System.Windows.Threading;

namespace Archiver.Planning
{
    public class BackupPlanOverviewViewModel : ViewModelBase, IDisposable
    {
        private FileSystemEntryViewModel? _selectedFolder;
        private BackupPlan _backupPlan = null!;
        private BackupPlan _originalBackupPlan = null!;
        private IFtpClientFactory _ftpFactory;
        private readonly BackupServiceFactory _backupServiceFactory;
        private BackupProgress? _progress;
        private readonly IProgressService _progressService;
        private readonly IClock _clock;
        private readonly IDisposable _progressSubscription;
        private Dispatcher _dispatcher;
        private string? _restoreDestination;

        public event EventHandler<BackupPlan>? OnSavePlan;
        public event EventHandler? OnCancel;

        public BackupPlanOverviewViewModel(IFtpClientFactory ftpClientFactory, BackupServiceFactory backupServiceFactory, IProgressService progressService, IClock clock, BackupPlan? plan)
        {
            _ftpFactory = ftpClientFactory;
            _backupServiceFactory = backupServiceFactory;
            _progressService = progressService;
            _clock = clock;
            _dispatcher = Dispatcher.CurrentDispatcher;

            RemoteFolders = new ObservableCollection<RemoteFolderViewModel>();
            ArchivedItems = new ObservableCollection<ArchivedItemViewModel>();

            BrowseCommand = new RelayCommand(BrowseDirectories);
            RefreshCommand = new AsyncCommand(RefreshRemoteFolders);

            SaveCommand = new RelayCommand(SavePlan);
            CancelCommand = new RelayCommand(Cancel);
            CheckConnectionCommand = new AsyncCommand(CheckConnection);
            ChangeTypeCommand = new RelayCommand<Type>(ChangeScheduleType);
            RestoreCommand = new AsyncCommand(RestorePlan);
            RestoreAllCommand = new AsyncCommand(RestoreAllPlan);
            SelectDestinationCommand = new RelayCommand(SelectDestination);

            BackupPlan = plan ?? new BackupPlan();
            _progressSubscription = _progressService.BackupProgress
                .Where(p => p?.BackupId == _backupPlan!.Id)
                .Subscribe(p => Progress = p);
        }

        public bool IsSaveEnabled => DestinationDirectory is not null;

        public ICommand BrowseCommand { get; set; }

        public ICommand SaveCommand { get; set; }

        public ICommand CancelCommand { get; set; }

        public ICommand ChangeTypeCommand { get; set; }

        public IAsyncCommand CheckConnectionCommand { get; set; }

        public IAsyncCommand RefreshCommand { get; set; }

        public IAsyncCommand RestoreCommand { get; set; }

        public IAsyncCommand RestoreAllCommand { get; set; }

        public ICommand SelectDestinationCommand { get; set; }

        public string? RestoreDestination 
        {
            get
            {
                return _restoreDestination;
            }
            set
            {
                _restoreDestination = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<FileSystemEntryViewModel> Drives { get; set; } = null!;

        public ObservableCollection<RemoteFolderViewModel> RemoteFolders { get; set; }

        public ObservableCollection<ArchivedItemViewModel> ArchivedItems { get; set; }

        public BackupProgress? Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ProgressPercentage));
            }
        }

        public double ProgressPercentage => _progress?.Percentage ?? 0;

        public LocalDateTime NextExecution
        {
            get
            {
                var now = _clock.GetCurrentInstant().ToLocalDateTime();
                return _backupPlan.Schedule.NextExecution(now);
            }
        }

        public int SelectedSchedulePeriod
        {
            get
            {
                switch (_backupPlan.Schedule)
                {
                    case DailyBackupSchedule:
                        return 0;
                    case WeeklyBackupSchedule:
                        return 1;
                    default:
                        return -1;
                }
            }
            set
            {
                switch (value)
                {
                    case 0:
                        BackupPlan.Schedule = new DailyBackupSchedule();
                        break;
                    case 1:
                        BackupPlan.Schedule = new DailyBackupSchedule();
                        break;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsWeeklySelected));
                OnPropertyChanged(nameof(IsMondayChecked));
                OnPropertyChanged(nameof(IsTuesdayChecked));
                OnPropertyChanged(nameof(IsWednesdayChecked));
                OnPropertyChanged(nameof(IsThursdayChecked));
                OnPropertyChanged(nameof(IsFridayChecked));
                OnPropertyChanged(nameof(IsSaturdayChecked));
                OnPropertyChanged(nameof(IsSundayChecked));
            }
        }

        public int SelectedScheduleType
        {
            get
            {
                return (int)_backupPlan.BackupType;
            }
            set
            {
                _backupPlan.BackupType = (BackupType)value;
                OnPropertyChanged();
            }
        }

        public bool IsWeeklySelected => BackupPlan.Schedule is WeeklyBackupSchedule;

        public bool IsMondayChecked
        {
            get => _backupPlan.Schedule is WeeklyBackupSchedule plan && plan.ExecutionDays.Contains(IsoDayOfWeek.Monday);
            set
            {
                var executionDays = ((WeeklyBackupSchedule)_backupPlan.Schedule).ExecutionDays;
                if (value)
                {
                    executionDays.Add(IsoDayOfWeek.Monday);
                }
                else if (executionDays.Count > 1)
                {
                    executionDays.Remove(IsoDayOfWeek.Monday);
                }
                OnPropertyChanged(nameof(NextExecution));
                OnPropertyChanged();
            }
        }

        public bool IsTuesdayChecked
        {
            get => _backupPlan.Schedule is WeeklyBackupSchedule plan && plan.ExecutionDays.Contains(IsoDayOfWeek.Tuesday);
            set
            {
                var executionDays = ((WeeklyBackupSchedule)_backupPlan.Schedule).ExecutionDays;
                if (value)
                {
                    executionDays.Add(IsoDayOfWeek.Tuesday);
                }
                else if (executionDays.Count > 1)
                {
                    executionDays.Remove(IsoDayOfWeek.Tuesday);
                }
                OnPropertyChanged(nameof(NextExecution));
                OnPropertyChanged();
            }
        }

        public bool IsWednesdayChecked
        {
            get => _backupPlan.Schedule is WeeklyBackupSchedule plan && plan.ExecutionDays.Contains(IsoDayOfWeek.Wednesday);
            set
            {
                var executionDays = ((WeeklyBackupSchedule)_backupPlan.Schedule).ExecutionDays;
                if (value)
                {
                    executionDays.Add(IsoDayOfWeek.Wednesday);
                }
                else if (executionDays.Count > 1)
                {
                    executionDays.Remove(IsoDayOfWeek.Wednesday);
                }
                OnPropertyChanged(nameof(NextExecution));
                OnPropertyChanged();
            }
        }

        public bool IsThursdayChecked
        {
            get => _backupPlan.Schedule is WeeklyBackupSchedule plan && plan.ExecutionDays.Contains(IsoDayOfWeek.Thursday);
            set
            {
                var executionDays = ((WeeklyBackupSchedule)_backupPlan.Schedule).ExecutionDays;
                if (value)
                {
                    executionDays.Add(IsoDayOfWeek.Thursday);
                }
                else if (executionDays.Count > 1)
                {
                    executionDays.Remove(IsoDayOfWeek.Thursday);
                }
                OnPropertyChanged(nameof(NextExecution));
                OnPropertyChanged();
            }
        }

        public bool IsFridayChecked
        {
            get => _backupPlan.Schedule is WeeklyBackupSchedule plan && plan.ExecutionDays.Contains(IsoDayOfWeek.Friday);
            set
            {
                var executionDays = ((WeeklyBackupSchedule)_backupPlan.Schedule).ExecutionDays;
                if (value)
                {
                    executionDays.Add(IsoDayOfWeek.Friday);
                }
                else if (executionDays.Count > 1)
                {
                    executionDays.Remove(IsoDayOfWeek.Friday);
                }
                OnPropertyChanged(nameof(NextExecution));
                OnPropertyChanged();
            }
        }

        public bool IsSaturdayChecked
        {
            get => _backupPlan.Schedule is WeeklyBackupSchedule plan && plan.ExecutionDays.Contains(IsoDayOfWeek.Saturday);
            set
            {
                var executionDays = ((WeeklyBackupSchedule)_backupPlan.Schedule).ExecutionDays;
                if (value)
                {
                    executionDays.Add(IsoDayOfWeek.Saturday);
                }
                else if (executionDays.Count > 1)
                {
                    executionDays.Remove(IsoDayOfWeek.Saturday);
                }
                OnPropertyChanged(nameof(NextExecution));
                OnPropertyChanged();
            }
        }

        public bool IsSundayChecked
        {
            get => _backupPlan.Schedule is WeeklyBackupSchedule plan && plan.ExecutionDays.Contains(IsoDayOfWeek.Sunday);
            set
            {
                var executionDays = ((WeeklyBackupSchedule)_backupPlan.Schedule).ExecutionDays;
                if (value)
                {
                    executionDays.Add(IsoDayOfWeek.Sunday);
                }
                else if (executionDays.Count > 1)
                {
                    executionDays.Remove(IsoDayOfWeek.Sunday);
                }
                OnPropertyChanged(nameof(NextExecution));
                OnPropertyChanged();
            }
        }

        public string? DestinationDirectory
        {
            get => _backupPlan.DestinationFolder;
            set
            {
                _backupPlan.DestinationFolder = value!;

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsSaveEnabled));
            }
        }

        public FileSystemEntryViewModel? SelectedFolder
        {
            get => _selectedFolder;
            set
            {
                _selectedFolder = value;

                OnPropertyChanged();
                OnPropertyChanged(nameof(SelectedFolderChildren));
            }
        }

        public IEnumerable<FileSystemEntryViewModel?> SelectedFolderChildren => SelectedFolder?.Children.ToList() ?? new List<FileSystemEntryViewModel?>();

        public void SelectTreeViewItem(FileSystemEntryViewModel entry)
        {
            if (entry.Children.Count(i => i is not null) == 0)
            {
                entry.LoadChildren();
            }

            SelectedFolder = entry;
        }

        public BackupPlan BackupPlan
        {
            get { return _backupPlan; }
            private set
            {
                _originalBackupPlan = value;
                _backupPlan = (BackupPlan)value.Clone();
                _backupPlan.Id = value.Id;
                _backupPlan.Schedule.LastExecution = value.Schedule.LastExecution;

                OnPropertyChanged();
                OnPropertyChanged(nameof(IsWeeklySelected));
                OnPropertyChanged(nameof(DestinationDirectory));
                OnPropertyChanged(nameof(IsSaveEnabled));
                OnPropertyChanged(nameof(IsMondayChecked));
                OnPropertyChanged(nameof(IsTuesdayChecked));
                OnPropertyChanged(nameof(IsWednesdayChecked));
                OnPropertyChanged(nameof(IsThursdayChecked));
                OnPropertyChanged(nameof(IsFridayChecked));
                OnPropertyChanged(nameof(IsSaturdayChecked));
                OnPropertyChanged(nameof(IsSundayChecked));

                LoadOverview();
            }
        }

        private void LoadOverview()
        {
            if(Drives is null)
            {
                // Get the logical drives
                var drives = DriveInfo.GetDrives().Where(d => d.IsReady);

                Drives = new ObservableCollection<FileSystemEntryViewModel>(
                    drives.Select(d => new FileSystemEntryViewModel(new DriveInfoWrapper(d), false)));
            }
            else
            {
                ResetFolderListing();
            }

            foreach (var item in _backupPlan.FileSystemItems)
            {
                foreach (var drive in Drives)
                {
                    drive.RestoreSelected(item);
                }
            }

            OnPropertyChanged(nameof(Drives));

            if (BackupPlan.DestinationFolder is not null)
            {
                var service = _backupServiceFactory.CreateService(BackupPlan);
                service.GetArchivedFiles(new CancellationTokenSource().Token).ContinueWith(f =>
                {
                    foreach(var entry in f.Result.Select(e => new ArchivedItemViewModel(e)))
                    {
                        _dispatcher.Invoke(() => ArchivedItems.Add(entry));
                    }
                    OnPropertyChanged(nameof(ArchivedItems));
                });
            }
        }

        private async Task RefreshRemoteFolders()
        {
            try
            {
                using var client = _ftpFactory.CreateFtpClient(BackupPlan.Connection);

                await client.Connect();
                foreach (var item in await client.GetListing())
                {
                    if (item.Type == FtpObjectType.Directory && RemoteFolders.All(r => r.FullName != item.FullName))
                    {
                        RemoteFolders.Add(new RemoteFolderViewModel(item, _ftpFactory, this));
                    }
                }

            }
            catch (InvalidOperationException)
            {
                System.Windows.MessageBox.Show("Enter connection information");
            }
        }

        private async Task CheckConnection()
        {
            try
            {
                using var client = _ftpFactory.CreateFtpClient(BackupPlan.Connection);
                client.Config.ConnectTimeout = 5;
                await client.Connect();
                if (client.IsConnected)
                {
                    System.Windows.MessageBox.Show("Successfully connected");
                }
                else
                {
                    System.Windows.MessageBox.Show("Connection failed");
                }
            }
            //TODO  || e is FtpAuthenticationException
            catch (Exception e) when (e is InvalidOperationException)
            {
                System.Windows.MessageBox.Show("Enter correct connection information");
            }
        }

        private void ResetFolderListing()
        {
            foreach (var drive in Drives)
            {
                drive.IsChecked = false;
                drive.IsExpanded = false;
            }
        }

        private void BrowseDirectories()
        {
            var dialog = new FolderBrowserDialog
            {
                SelectedPath = DestinationDirectory ?? string.Empty
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                DestinationDirectory = dialog.SelectedPath;
            }
        }

        private void Cancel()
        {
            OnCancel?.Invoke(this, EventArgs.Empty);
        }

        private void SavePlan()
        {
            var selectedItems = GetSelectedFileSystemEntries(Drives);
            _backupPlan.FileSystemItems = selectedItems.Select(f => f.Info).ToList();

            _backupPlan.Id = _originalBackupPlan.Id;
            OnSavePlan?.Invoke(this, _backupPlan);
            OnPropertyChanged(nameof(NextExecution));
        }

        private async Task RestorePlan()
        {
            DestinationGuard();

            var service = _backupServiceFactory.CreateService(BackupPlan);
            await service.RestoreFilesAsync(RestoreDestination!, ArchivedItems.Where(i => i.IsChecked).Select(i => i.Name), new CancellationTokenSource().Token);
        }

        private async Task RestoreAllPlan()
        {
            DestinationGuard();

            var service = _backupServiceFactory.CreateService(BackupPlan);
            await service.RestoreAsync(RestoreDestination!, new CancellationTokenSource().Token);
        }

        private void DestinationGuard()
        {
            if(RestoreDestination == null)
            {
                MessageBox.Show("Please select a destination folder");
            }
        }

        private void SelectDestination()
        {
            var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if(result == DialogResult.OK)
            {
                RestoreDestination = dialog.SelectedPath;
            }
        }

        private void ChangeScheduleType(Type type)
        {

        }

        private IEnumerable<FileSystemEntryViewModel> GetSelectedFileSystemEntries(IEnumerable<FileSystemEntryViewModel?> entries)
        {
            List<FileSystemEntryViewModel> selectedEntries = new();

            foreach (var entry in entries)
            {
                if (entry!.IsChecked is null)
                {
                    selectedEntries.AddRange(GetSelectedFileSystemEntries(entry.Children.Where(c => c is not null)));
                }
                else if (entry!.IsChecked == true)
                {
                    selectedEntries.Add(entry);
                }
            }

            return selectedEntries;
        }

        public void Dispose()
        {
            _progressSubscription.Dispose();
        }
    }
}
