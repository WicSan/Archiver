using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using FastBackup.Planning.Model;
using FastBackup.Util;

namespace FastBackup.Planning
{
    public class FileSystemEntryViewModel : ViewModelBase
    {
        private readonly FileSystemInfo _info;
        private bool _isChecked;
        private bool _isExpanded;

        public bool IsFolder => _info is DirectoryInfo;

        public bool IsDrive => _info is DriveInfoWrapper;

        public bool IsFile => _info is FileInfo;

        public string ImageName => IsDrive ? "drive" : (IsFolder ? (IsExpanded ? "folder-open" : "folder-closed") : "file");

        public string Name => _info.Name;

        public DateTime Modified => _info.LastWriteTime;

        public long Size => IsFile ? ((FileInfo)_info).Length : 0;

        public FileSystemInfo Info => _info;

        public ObservableCollection<FileSystemEntryViewModel?> Children { get; set; }

        public ICollectionView Directories { get; }

        public bool CanExpand => !IsFile;

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;

                if (value)
                {
                    Expand();
                }
            }
        }

        public bool? IsChecked
        {
            get
            {
                if (Children.Any(c => c is not null && (c.IsChecked ?? true) != _isChecked))
                {
                    return null;
                }

                return _isChecked;
            }
            set
            {
                _isChecked = value ?? true;
                UpdateChildren(this);
                OnPropertyChanged();
            }
        }

        public bool HasChildren => Children.Count(i => i is not null) > 0;

        /// <summary>
        /// Default constructor
        /// </summary>
        public FileSystemEntryViewModel(FileSystemInfo info, bool isChecked)
        {
            _info = info;
            _isChecked = isChecked;

            Children = new();

            var source = CollectionViewSource.GetDefaultView(Children);
            source.Filter = p => ((FileSystemEntryViewModel?)p)?.IsFolder ?? true;
            Directories = source;

            // Setup the children as needed
            ClearChildren();
        }

        public void LoadChildren()
        {
            Children.Clear();

            if (!CanExpand)
            {
                return;
            }

            // Find all children
            var fileSystemEntries = Directory
                .EnumerateFileSystemEntries(_info.FullName, "*", new EnumerationOptions {IgnoreInaccessible = true});
            foreach (var fileSystemEntryPath in fileSystemEntries)
            {
                var item = new FileSystemEntryViewModel(fileSystemEntryPath.ToFileSystemEntry(), IsChecked ?? true);
                item.PropertyChanged += (_, _) => OnPropertyChanged(nameof(IsChecked)); 
                Children.Add(item);
            }

            Directories.Refresh();
        }

        private void UpdateChildren(FileSystemEntryViewModel entry)
        {
            foreach (var child in entry.Children.Where(c => c is not null))
            {
                child!.IsChecked = _isChecked;
            }
        }

        private void ClearChildren()
        {
            // Clear items
            Children.Clear();

            // Show the expand arrow if we are not a file
            if (CanExpand)
                Children.Add(null);

            Directories.Refresh();
        }

        private void Expand()
        {
            // We cannot expand a file
            if (!CanExpand || !Children.Any(c => c is null))
                return;

            LoadChildren();
        }
    }
}