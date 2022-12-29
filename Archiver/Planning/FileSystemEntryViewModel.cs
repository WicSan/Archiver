using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using Archiver.Util;
using Archiver.Shared;

namespace Archiver.Planning
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

        public string Type => IsFolder ? "Folder" : "File";

        public string Name => _info.Name;

        public DateTime Modified => _info.LastWriteTime;

        public string Size => IsFolder ? string.Empty : string.Format("{0:N0} KB", ((FileInfo)_info).Length / 1024);

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
                UpdateChildren();
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

        public void RestoreSelected(FileSystemInfo selectedInfo)
        {
            if (selectedInfo.FullName.Contains(Info.FullName))
            {
                if (selectedInfo.FullName == Info.FullName)
                {
                    IsChecked = true;
                }
                else if(IsDrive || IsFolder)
                {
                    if (!HasChildren)
                    {
                        LoadChildren();
                    }

                    if (!HasChildren)
                    {
                        return;
                    }
                    
                    foreach(var child in Children)
                    {
                        child!.RestoreSelected(selectedInfo);
                    }
                }
            }
        }

        private void UpdateChildren()
        {
            foreach (var child in Children.Where(c => c is not null))
            {
                child!.IsChecked = _isChecked;
            }
        }

        private void ClearChildren()
        {
            // Clear items
            Children.Clear();

            // Show the expand arrow if it is not a file
            if (CanExpand)
                Children.Add(null);

            Directories.Refresh();
        }

        private void Expand()
        {
            // Cannot expand a file
            if (!CanExpand || !Children.Any(c => c is null))
                return;

            LoadChildren();
        }
    }
}
