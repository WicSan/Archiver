using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Security;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Input;
using ArchivePlanner.Backup;
using ArchivePlanner.Planning.Model;
using ArchivePlanner.Util;
using Archiver.Shared;
using FluentFTP;
using NodaTime;

namespace ArchivePlanner.Planning
{
    public class BackupPlanOverviewViewModel : ViewModelBase, IDisposable
    {
        private FileSystemEntryViewModel? _selectedFolder;
        private BackupPlan _backupPlan = null!;
        private BackupPlan _originalBackupPlan = null!;
        private IFtpClientFactory _ftpFactory;
        private double _progress;
        private readonly IProgressService _progressService;
        private readonly IDisposable _progressSubscription;

        public event EventHandler<BackupPlan>? OnSavePlan;
        public event EventHandler? OnCancel;

        public BackupPlanOverviewViewModel(IFtpClientFactory ftpClientFactory, IProgressService progressService)
        {
            LoadOverview();

            _ftpFactory = ftpClientFactory;
            _progressService = progressService;
            _progressSubscription = _progressService.BackupProgress
                .Where(p => p?.Id == _backupPlan?.Id)
                .Subscribe(p => Progress = p?.Progress ?? -1);

            RemoteFolders = new ObservableCollection<RemoteFolderViewModel>();

            BrowseCommand = new RelayCommand(BrowseDirectories);
            RefreshCommand = new AsyncCommand(RefreshRemoteFolders);

            SaveCommand = new RelayCommand(SavePlan);
            CancelCommand = new RelayCommand(Cancel);
            CheckConnectionCommand = new AsyncCommand(CheckConnection);
            ChangeTypeCommand = new RelayCommand<Type>(ChangeScheduleType);
            BackupPlan = new BackupPlan();
        }

        public bool IsSaveEnabled => DestinationDirectory is not null;

        public ICommand BrowseCommand { get; set; }

        public ICommand SaveCommand { get; set; }

        public ICommand CancelCommand { get; set; }

        public ICommand ChangeTypeCommand { get; set; }

        public IAsyncCommand CheckConnectionCommand { get; set; }

        public IAsyncCommand RefreshCommand { get; set; }

        public ObservableCollection<FileSystemEntryViewModel> Drives { get; set; } = null!;

        public ObservableCollection<RemoteFolderViewModel> RemoteFolders { get; set; } = null!;

        public double Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                OnPropertyChanged();
            }
        }

        public bool IsDailySelected
        {
            get
            {
                return _backupPlan.Schedule is DailyBackupSchedule;
            }
        }

        public bool IsWeeklySelected
        {
            get
            {
                return _backupPlan.Schedule is WeeklyBackupSchedule;
            }
        }

        public bool IsMondayChecked
        {
            get => _backupPlan.Schedule is WeeklyBackupSchedule plan && plan.ExecutionDays.Contains(IsoDayOfWeek.Monday);
            set
            {
                if (value)
                {
                    ((WeeklyBackupSchedule)_backupPlan.Schedule).ExecutionDays.Add(IsoDayOfWeek.Monday);
                }
                else
                {
                    ((WeeklyBackupSchedule)_backupPlan.Schedule).ExecutionDays.Remove(IsoDayOfWeek.Monday);
                }
                OnPropertyChanged();
            }
        }

        public bool IsTuesdayChecked
        {
            get => _backupPlan.Schedule is WeeklyBackupSchedule plan && plan.ExecutionDays.Contains(IsoDayOfWeek.Tuesday);
            set
            {
                if (value)
                {
                    ((WeeklyBackupSchedule)_backupPlan.Schedule).ExecutionDays.Add(IsoDayOfWeek.Tuesday);
                }
                else
                {
                    ((WeeklyBackupSchedule)_backupPlan.Schedule).ExecutionDays.Remove(IsoDayOfWeek.Tuesday);
                }
                OnPropertyChanged();
            }
        }

        public bool IsWednesdayChecked
        {
            get => _backupPlan.Schedule is WeeklyBackupSchedule plan && plan.ExecutionDays.Contains(IsoDayOfWeek.Wednesday);
            set
            {
                if (value)
                {
                    ((WeeklyBackupSchedule)_backupPlan.Schedule).ExecutionDays.Add(IsoDayOfWeek.Wednesday);
                }
                else
                {
                    ((WeeklyBackupSchedule)_backupPlan.Schedule).ExecutionDays.Remove(IsoDayOfWeek.Wednesday);
                }
                OnPropertyChanged();
            }
        }

        public bool IsThursdayChecked
        {
            get => _backupPlan.Schedule is WeeklyBackupSchedule plan && plan.ExecutionDays.Contains(IsoDayOfWeek.Thursday);
            set
            {
                if (value)
                {
                    ((WeeklyBackupSchedule)_backupPlan.Schedule).ExecutionDays.Add(IsoDayOfWeek.Thursday);
                }
                else
                {
                    ((WeeklyBackupSchedule)_backupPlan.Schedule).ExecutionDays.Remove(IsoDayOfWeek.Thursday);
                }
                OnPropertyChanged();
            }
        }

        public bool IsFridayChecked
        {
            get => _backupPlan.Schedule is WeeklyBackupSchedule plan && plan.ExecutionDays.Contains(IsoDayOfWeek.Friday);
            set
            {
                if (value)
                {
                    ((WeeklyBackupSchedule)_backupPlan.Schedule).ExecutionDays.Add(IsoDayOfWeek.Friday);
                }
                else
                {
                    ((WeeklyBackupSchedule)_backupPlan.Schedule).ExecutionDays.Remove(IsoDayOfWeek.Friday);
                }
                OnPropertyChanged();
            }
        }

        public bool IsSaturdayChecked
        {
            get => _backupPlan.Schedule is WeeklyBackupSchedule plan && plan.ExecutionDays.Contains(IsoDayOfWeek.Saturday);
            set
            {
                if (value)
                {
                    ((WeeklyBackupSchedule)_backupPlan.Schedule).ExecutionDays.Add(IsoDayOfWeek.Saturday);
                }
                else
                {
                    ((WeeklyBackupSchedule)_backupPlan.Schedule).ExecutionDays.Remove(IsoDayOfWeek.Saturday);
                }
                OnPropertyChanged();
            }
        }

        public bool IsSundayChecked
        {
            get => _backupPlan.Schedule is WeeklyBackupSchedule plan && plan.ExecutionDays.Contains(IsoDayOfWeek.Sunday);
            set
            {
                if (value)
                {
                    ((WeeklyBackupSchedule)_backupPlan.Schedule).ExecutionDays.Add(IsoDayOfWeek.Sunday);
                }
                else
                {
                    ((WeeklyBackupSchedule)_backupPlan.Schedule).ExecutionDays.Remove(IsoDayOfWeek.Sunday);
                }
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

        public BackupPlan BackupPlan
        {
            get { return _backupPlan; }
            set
            {
                _originalBackupPlan = value;
                _backupPlan = (BackupPlan)value.Clone();

                OnPropertyChanged();
                OnPropertyChanged(nameof(DestinationDirectory));
                OnPropertyChanged(nameof(IsSaveEnabled));
                OnPropertyChanged(nameof(IsDailySelected));
                OnPropertyChanged(nameof(IsWeeklySelected));
                OnPropertyChanged(nameof(IsMondayChecked));
                OnPropertyChanged(nameof(IsTuesdayChecked));
                OnPropertyChanged(nameof(IsWednesdayChecked));
                OnPropertyChanged(nameof(IsThursdayChecked));
                OnPropertyChanged(nameof(IsFridayChecked));
                OnPropertyChanged(nameof(IsSaturdayChecked));
                OnPropertyChanged(nameof(IsSundayChecked));

                ResetFolderListing();

                foreach (var item in _backupPlan.FileSystemItems)
                {
                    foreach (var drive in Drives)
                    {
                        drive.RestoreSelected(item);
                    }
                }
                OnPropertyChanged(nameof(Drives));
            }
        }

        public void SelectTreeViewItem(FileSystemEntryViewModel entry)
        {
            if (entry.Children.Count(i => i is not null) == 0)
            {
                entry.LoadChildren();
            }

            SelectedFolder = entry;
        }

        private void LoadOverview()
        {
            // Get the logical drives
            var drives = DriveInfo.GetDrives().Where(d => d.IsReady);

            Drives = new ObservableCollection<FileSystemEntryViewModel>(
                drives.Select(d => new FileSystemEntryViewModel(new DriveInfoWrapper(d), false)));
        }

        private async Task RefreshRemoteFolders()
        {
            try
            {
                using var client = _ftpFactory.CreateFtpClient(BackupPlan.Connection);

                await client.ConnectAsync();
                foreach (var item in await client.GetListingAsync())
                {
                    if (item.Type == FtpFileSystemObjectType.Directory && RemoteFolders.All(r => r.FullName != item.FullName))
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
                client.ConnectTimeout = 5;
                await client.ConnectAsync();
                if (client.IsConnected)
                {
                    System.Windows.MessageBox.Show("Successfully connected");
                }
                else
                {
                    System.Windows.MessageBox.Show("Connection failed");
                }
            }
            catch (Exception e) when (e is InvalidOperationException || e is FtpAuthenticationException)
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
        }

        private void ChangeScheduleType(Type type)
        {
            BackupPlan.Schedule = (BackupSchedule)Activator.CreateInstance(type, BackupPlan.Schedule)!;
            OnPropertyChanged(nameof(IsDailySelected));
            OnPropertyChanged(nameof(IsWeeklySelected));
            OnPropertyChanged(nameof(IsMondayChecked));
            OnPropertyChanged(nameof(IsTuesdayChecked));
            OnPropertyChanged(nameof(IsWednesdayChecked));
            OnPropertyChanged(nameof(IsThursdayChecked));
            OnPropertyChanged(nameof(IsFridayChecked));
            OnPropertyChanged(nameof(IsSaturdayChecked));
            OnPropertyChanged(nameof(IsSundayChecked));
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
