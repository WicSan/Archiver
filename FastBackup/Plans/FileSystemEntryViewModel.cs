using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using FastBackup.Util;

namespace FastBackup.Plans
{
    public class FileSystemEntryViewModel : ViewModelBase
    {
        private readonly FileSystemInfo _info;
        private bool _isChecked;
        private bool _isExpanded;

        public FileSystemEntryType Type 
        {
            get
            {
                if (_info is DirectoryInfo)
                {
                    return FileSystemEntryType.Folder;
                }

                if (_info is DriveInfoWrapper)
                {
                    return FileSystemEntryType.Drive;
                }

                return FileSystemEntryType.File;
            }
        }

        public string ImageName => Type == FileSystemEntryType.Drive ? "drive" : (Type == FileSystemEntryType.File ? "file" : (IsExpanded ? "folder-open" : "folder-closed"));

        public string Name => _info.Name;

        public DateTime Modified => _info.LastWriteTime;

        public long Size
        {
            get
            {
                if (_info is FileInfo info)
                {
                    return info.Length;
                }

                return 0;
            }
        }

        public FileSystemInfo Info => _info;

        public ObservableCollection<FileSystemEntryViewModel?> Children { get; set; } = new();

        public ObservableCollection<FileSystemEntryViewModel?> Directories { get; set; } = new();

        public bool CanExpand => Type != FileSystemEntryType.File;

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

            Children.CollectionChanged += (_, args) =>
            {
                switch(args.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        foreach (var newItem in args.NewItems!)
                        {
                            if(newItem is null || ((FileSystemEntryViewModel?)newItem)?.Type == FileSystemEntryType.Folder)
                            {
                                Directories.Add(newItem as FileSystemEntryViewModel);
                            }
                        }

                        break;

                    case NotifyCollectionChangedAction.Remove:
                        foreach (var oldItem in args.OldItems!)
                        {
                            Directories.Remove(oldItem as FileSystemEntryViewModel);
                        }

                        break;

                    case NotifyCollectionChangedAction.Reset:
                        Directories.Clear();
                        break;
                }
            };
                

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
        }

        private void Expand()
        {
            // We cannot expand a file
            if (!CanExpand)
                return;

            LoadChildren();
        }
    }
}
