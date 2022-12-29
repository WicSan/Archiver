using Microsoft.Extensions.Logging;
using System.Windows;

namespace ArchivePlanner
{
    /// <summary>
    /// Interaction logic for Archiver.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ILogger<MainWindow> _logger;

        public MainWindow(MainViewModel model, ILogger<MainWindow> logger)
        {
            InitializeComponent();

            DataContext = model;
            _logger = logger;
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var model = (MainViewModel)DataContext;
            await model.LoadPlans();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();

            e.Cancel = true;
        }
    }
}
