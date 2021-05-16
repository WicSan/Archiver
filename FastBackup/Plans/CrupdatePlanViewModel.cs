using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Input;
using FastBackup.Util;
using LiteDB;

namespace FastBackup.Plans
{
    public class CrupdatePlanViewModel : ViewModelBase, INavigatebleViewModel
    {
        private string? _selectedDestinationDirectory;
        private FileSystemEntryViewModel? _selectedFolder;
        private readonly Repository _planRepository;
        private readonly NavigationService _navigationService;

        public event EventHandler? OnPlanSaved;

        public CrupdatePlanViewModel(NavigationService navigationService)
        {
            // Get the logical drives
            var drives = DriveInfo.GetDrives().Where(d => d.IsReady);

            // Create the view models from the data
            Drives = new ObservableCollection<FileSystemEntryViewModel>(
                drives.Select(d => new FileSystemEntryViewModel(new DriveInfoWrapper(d), false)));

            BrowseCommand = new RelayCommand(BrowseDirectories);

            SaveCommand = new RelayCommand(SavePlan);
            CancelCommand = new RelayCommand(Cancel);

            _planRepository = new Repository();
            _navigationService = navigationService;
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

        public ObservableCollection<FileSystemEntryViewModel?> SelectedFolderChildren =>
            SelectedFolder?.Children ?? new ObservableCollection<FileSystemEntryViewModel?>();

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
            _navigationService.Navigate(typeof(PlanOverviewViewModel));
        }

        private void SavePlan()
        {
            var selectedItems = GetSelectedFileSystemEntries(Drives);

            var plan = new BackupPlan()
            {
                Name = "test",
                ExecutionStart = DateTime.Now,
                Destination = new DirectoryInfo(SelectedDestinationDirectory!),
                FileSystemItems = selectedItems.Select(f => f.Info).ToList(),
            };

            _planRepository.GetCollection<BackupPlan>().Upsert(plan);

            OnPlanSaved?.Invoke(this, EventArgs.Empty);

            _navigationService.Navigate(typeof(PlanOverviewViewModel));
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

        public void NavigateTo(object? param)
        {
            if(param is not null)
            {
                _planRepository.GetCollection<BackupPlan>().Find(p => p.Name == (string)param);
            }
        }
    }
}
