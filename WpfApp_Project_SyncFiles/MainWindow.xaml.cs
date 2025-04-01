using System.Windows;
using WpfApp_Project_SyncFiles.ViewModels;

namespace WpfApp_Project_SyncFiles
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainWindowViewModel _viewModel;

        public MainWindow()
        {
            InitializeComponent();

            _viewModel = new MainWindowViewModel(Dispatcher);
            DataContext = _viewModel;
            Loaded += async (s, e) => await _viewModel.StartTasksAsync();
        }
    }
}
