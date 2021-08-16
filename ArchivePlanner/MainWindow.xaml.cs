using System.IO;
using System.Windows;
using FastBackup.Planning;
using LiteDB;

namespace FastBackup
{
    /// <summary>
    /// Interaction logic for FastBackup.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow(MainViewModel model)
        {
            InitializeComponent();

            this.DataContext = model;
        }
    }
}
