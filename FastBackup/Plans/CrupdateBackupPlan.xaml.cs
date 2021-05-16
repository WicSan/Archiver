using System.Windows;

namespace FastBackup.Plans
{
    /// <summary>
    /// Interaction logic for CreateBackupPlan.xaml
    /// </summary>
    public partial class CrupdateBackupPlan
    {
        public CrupdateBackupPlan()
        {
            InitializeComponent();
        }

        private void FolderTreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is FileSystemEntryViewModel item)
            {
                ((CrupdatePlanViewModel)DataContext).SelectTreeViewItem((FileSystemEntryViewModel)e.NewValue);
            }
        }
    }
}
