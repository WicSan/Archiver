using System.IO;
using System.Windows;
using FastBackup.Plans;
using LiteDB;

namespace FastBackup
{
    /// <summary>
    /// Interaction logic for FastBackup.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            BsonMapper.Global.RegisterType(
                info => info.FullName,
                bson => new DirectoryInfo(bson));

            BsonMapper.Global.RegisterType(
                info => info.FullName,
                bson =>
                    bson.AsString.ToFileSystemEntry());

            this.DataContext = new MainViewModel();
        }
    }
}
