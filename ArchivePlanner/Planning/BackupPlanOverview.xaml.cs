using ArchivePlanner.Planning;
using System.Windows;
using System.Windows.Controls;

namespace FastBackup.Planning
{
    /// <summary>
    /// Interaction logic for CreateBackupPlan.xaml
    /// </summary>
    public partial class BackupPlanOverview : UserControl
    {
        public BackupPlanOverview()
        {
        }

        public BackupPlanOverview(BackupPlanViewModel model)
        {
            InitializeComponent();

            DataContext = model;
        }

        private void FolderTreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is FileSystemEntryViewModel item)
            {
                ((BackupPlanViewModel)DataContext).SelectTreeViewItem((FileSystemEntryViewModel)e.NewValue);
            }
        }
    }
}
