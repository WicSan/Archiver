using System.Windows;
using System.Windows.Controls;
using FastBackup.Plans;

namespace FastBackup
{
    /// <summary>
    /// Interaction logic for PlanOverview.xaml
    /// </summary>
    public partial class PlanOverview : Page
    {
        public PlanOverview()
        {
            InitializeComponent();

            this.DataContext = new PlanOverviewViewModel();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            this.NavigationService?.Navigate(new CrupdateBackupPlan());
        }
    }
}
