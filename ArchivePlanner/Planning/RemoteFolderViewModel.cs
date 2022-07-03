using FluentFTP;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ArchivePlanner.Planning
{
    public class RemoteFolderViewModel
    {
        private readonly FtpListItem _item;
        private readonly IFtpClientFactory _clientFactory;
        private readonly BackupPlanOverviewViewModel _parentViewModel;
        private bool _isExpanded;

        public RemoteFolderViewModel(FtpListItem item, IFtpClientFactory factory, BackupPlanOverviewViewModel viewModel)
        {
            _item = item;
            _clientFactory = factory;
            _parentViewModel = viewModel;
            Children.Add(null);
        }

        public bool IsSelected { get; set; }

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;

                if (value)
                {
                    if (Children.Contains(null))
                    {
                        Children.Remove(null);
                    }
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    LoadChildrenAsync();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            }
        }

        public ObservableCollection<RemoteFolderViewModel?> Children { get; set; } = new ObservableCollection<RemoteFolderViewModel?>();

        public string Name => _item.Name;

        public string FullName => _item.FullName;

        private async Task LoadChildrenAsync()
        {
            using var client = _clientFactory.CreateFtpClient(_parentViewModel.BackupPlan.Connection);
            await client.ConnectAsync();
            foreach (var item in await client.GetListingAsync(_item.FullName))
            {
                if (item.Type == FtpFileSystemObjectType.Directory)
                {
                    Children.Add(new RemoteFolderViewModel(item, _clientFactory, _parentViewModel));
                }
            }
        }
    }
}
