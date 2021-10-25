using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using ArchivePlanner.Planning.Model;
using ArchivePlanner.Util;
using Archiver.Shared;
using FastBackup.Planning;
using LiteDB;
using NodaTime;

namespace ArchivePlanner.Planning
{
    public class BackupPlanViewModel : ViewModelBase
    {
        private string? _selectedDestinationDirectory;
        private FileSystemEntryViewModel? _selectedFolder;
        private readonly Repository _planRepository;
        private BackupPlan _backupPlan = null!;

        public event EventHandler? OnPlanSaved;

        public BackupPlanViewModel(PlanningRepository repository)
        {
            // Get the logical drives
            var drives = DriveInfo.GetDrives().Where(d => d.IsReady);

            // Create the view models from the data
            Drives = new ObservableCollection<FileSystemEntryViewModel>(
                drives.Select(d => new FileSystemEntryViewModel(new DriveInfoWrapper(d), false)));

            BrowseCommand = new RelayCommand(BrowseDirectories);

            SaveCommand = new RelayCommand(SavePlan);
            CancelCommand = new RelayCommand(Cancel);

            _planRepository = repository;
        }

        public BackupPlan BackupPlan
        {
            get { return _backupPlan; }
            set
            {
                _backupPlan = value;

                foreach (var item in _backupPlan.FileSystemItems)
                {
                    DriveInfoWrapper driveInfo;
                    if (item is DriveInfoWrapper drive)
                    {
                        driveInfo = drive;
                        var viewModel = Drives.First(d => d.Info.FullName == driveInfo.FullName);
                        viewModel.IsChecked = true;
                    }
                    else
                    {
                        if (item is DirectoryInfo info)
                        {
                            driveInfo = new DriveInfoWrapper(info.DriveInfo());
                        }
                        else
                        {
                            driveInfo = new DriveInfoWrapper(((FileInfo)item).DriveInfo()!);
                        }

                        var viewModel = Drives.First(d => d.Info.FullName == driveInfo.FullName);
                        if (!viewModel.HasChildren)
                        {
                            viewModel.LoadChildren();
                        }

                        for (var i = 0; i < viewModel.Children.Count; i++)
                        {
                            var child = viewModel.Children[i];
                            if (item.FullName.Contains(child!.Info.FullName))
                            {
                                if (item.FullName == child.Info.FullName)
                                {
                                    child.IsChecked = true;
                                }
                                else
                                {
                                    if (!child.HasChildren)
                                    {
                                        child.LoadChildren();
                                    }
                                    viewModel = child;
                                    i = 0;
                                }
                            }
                        }
                    }
                }
            }
        }

        public ObservableCollection<FileSystemEntryViewModel> Drives { get; set; }

        public string? SelectedDestinationDirectory
        {
            get => _selectedDestinationDirectory;
            set
            {
                if (value != _selectedDestinationDirectory)
                {
                    _selectedDestinationDirectory = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsSaveEnabled));
                }
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

        public bool IsSaveEnabled => SelectedDestinationDirectory is not null;

        public ICommand BrowseCommand { get; set; }

        public ICommand SaveCommand { get; set; }

        public ICommand CancelCommand { get; set; }

        public void SelectTreeViewItem(FileSystemEntryViewModel entry)
        {
            if (entry.Children.Count(i => i is not null) == 0)
            {
                entry.LoadChildren();
            }

            SelectedFolder = entry;
        }

        private void BrowseDirectories()
        {
            var dialog = new FolderBrowserDialog
            {
                SelectedPath = SelectedDestinationDirectory ?? string.Empty
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                SelectedDestinationDirectory = dialog.SelectedPath;
            }
        }

        private void Cancel()
        {
        }

        private void SavePlan()
        {
            var selectedItems = GetSelectedFileSystemEntries(Drives);
            var systemTimeZone = DateTimeZoneProviders.Bcl.GetSystemDefault();

            var plan = new FullBackupPlan()
            {
                Name = "test",
                ExecutionStart = SystemClock.Instance.GetCurrentInstant().InZone(systemTimeZone).TimeOfDay,
                Destination = new DirectoryInfo(SelectedDestinationDirectory!),
                FileSystemItems = selectedItems.Select(f => f.Info).ToList(),
            };

            _planRepository.Upsert(plan);

            OnPlanSaved?.Invoke(this, EventArgs.Empty);
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
    }
}
