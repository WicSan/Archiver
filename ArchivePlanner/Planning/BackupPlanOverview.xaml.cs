using System.Windows;
using System.Windows.Controls;

namespace ArchivePlanner.Planning
{
    /// <summary>
    /// Interaction logic for CreateBackupPlan.xaml
    /// </summary>
    public partial class BackupPlanOverview : UserControl
    {
        public BackupPlanOverview()
        {
            InitializeComponent();
        }

        private void FolderTreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is FileSystemEntryViewModel item)
            {
                ((BackupPlanOverviewViewModel)DataContext).SelectTreeViewItem(item);
            }
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            ((BackupPlanOverviewViewModel)DataContext).Password = ((PasswordBox)sender).SecurePassword;
        }

        private void RemoteFolder_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ((BackupPlanOverviewViewModel)DataContext).DestinationDirectory = ((RemoteFolderViewModel)e.NewValue).FullName;
        }
    }
}
