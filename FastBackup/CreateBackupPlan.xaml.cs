using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FastBackup.Plans;

namespace FastBackup
{
    /// <summary>
    /// Interaction logic for CreateBackupPlan.xaml
    /// </summary>
    public partial class CreateBackupPlan : Page
    {
        public CreateBackupPlan()
        {
            InitializeComponent();

            var model = new CreatePlanViewModel();
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
                ((CreatePlanViewModel)DataContext).SelectTreeViewItem((FileSystemEntryViewModel)e.NewValue);
            }
        }
    }
}
