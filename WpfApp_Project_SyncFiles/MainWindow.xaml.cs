using System.Windows;
using WpfApp_Project_SyncFiles.ViewModels;

namespace WpfApp_Project_SyncFiles
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }
    }
}
