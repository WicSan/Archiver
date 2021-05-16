using System.Windows;
using System.Windows.Controls;
using FastBackup.Plans;

namespace FastBackup
{
    /// <summary>
    /// Interaction logic for CreateBackupPlan.xaml
    /// </summary>
    public partial class CrupdateBackupPlan : Page
    {
        public CrupdateBackupPlan()
        {
            InitializeComponent();

            var model = new CrupdatePlanViewModel();
            DataContext = model;

            model.OnPlanSaved += (_, _) => NavigationService?.GoBack();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService?.GoBack();
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
