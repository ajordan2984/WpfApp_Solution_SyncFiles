using System;
using System.Collections.Concurrent;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WpfApp_Project_SyncFiles.Commands;
using WpfApp_Project_SyncFiles.Views;

namespace WpfApp_Project_SyncFiles.ViewModels
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };

        public MainWindowViewModel(Dispatcher dispatcher)
        {
            UpdateCommandBrowsePcPath = new RelayCommand(execute => Browse("PcPath"));
            UpdateCommandBrowseExternalDrive1 = new RelayCommand(execute => Browse("ExternalDrive1Path"));
            UpdateCommandBrowseExternalDrive2 = new RelayCommand(execute => Browse("ExternalDrive2Path"));
            UpdateCommandBrowseExternalDrive3 = new RelayCommand(execute => Browse("ExternalDrive3Path"));
            UpdateCommandBrowseExternalDrive4 = new RelayCommand(execute => Browse("ExternalDrive4Path"));
            _dispatcher = dispatcher;
        }

        #region Private Members
        private static string _PcPath = null;
        private static string _ExternalDrive1Path = null;
        private static string _ExternalDrive2Path = null;
        private static string _ExternalDrive3Path = null;
        private static string _ExternalDrive4Path = null;
        private string _statusText = null;
        private readonly Dispatcher _dispatcher;
        private readonly ConcurrentQueue<string> _messageQueue = new();
        private StringBuilder _textBuilder = new();
        #endregion


        #region Public Properties
        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(StatusText)));
            }
        }
        public string PcPath
        {
            get
            {
                return _PcPath;
            }
            set
            {
                _PcPath = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(PcPath)));
            }
        }
        public string ExternalDrive1Path
        {
            get
            {
                return _ExternalDrive1Path;
            }
            set
            {
                _ExternalDrive1Path = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(ExternalDrive1Path)));
            }
        }
        public string ExternalDrive2Path
        {
            get
            {
                return _ExternalDrive2Path;
            }
            set
            {
                _ExternalDrive2Path = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(ExternalDrive2Path)));
            }
        }
        public string ExternalDrive3Path
        {
            get
            {
                return _ExternalDrive3Path;
            }
            set
            {
                _ExternalDrive3Path = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(ExternalDrive3Path)));
            }
        }
        public string ExternalDrive4Path
        {
            get
            {
                return _ExternalDrive4Path;
            }
            set
            {
                _ExternalDrive4Path = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(ExternalDrive4Path)));
            }
        }
        #endregion


        #region Button Clicks
        public RelayCommand UpdateCommandBrowsePcPath { get; set; }
        public RelayCommand UpdateCommandBrowseExternalDrive1 { get; set; }
        public RelayCommand UpdateCommandBrowseExternalDrive2 { get; set; }
        public RelayCommand UpdateCommandBrowseExternalDrive3 { get; set; }
        public RelayCommand UpdateCommandBrowseExternalDrive4 { get; set; }
        #endregion

        #region Command Executions
        public void Browse(string selectedTextBox)
        {
            var childView = new FileDialogView();
            var childModel = new FileDialogViewModel(childView.Close);
            childView.DataContext = childModel;
            childView.ShowDialog();

            if (childModel.FileDialogTree.SelectedItem != null)
            {
                string path = childModel.FileDialogTree.SelectedItem.FullPath;

                switch (selectedTextBox)
                {
                    case "PcPath":
                        PcPath = path;
                        break;
                    case "ExternalDrive1Path":
                        ExternalDrive1Path = path;
                        break;
                    case "ExternalDrive2Path":
                        ExternalDrive2Path = path;
                        break;
                    case "ExternalDrive3Path":
                        ExternalDrive3Path = path;
                        break;
                    case "ExternalDrive4Path":
                        ExternalDrive4Path = path;
                        break;
                    default:
                        MessageBox.Show("An error occured in the Browse function.", "Alert");
                        break;
                }
            }
        }
        #endregion

        public async Task StartTasksAsync()
        {
            var tasks = new Task[100];

            for (int i = 0; i < 100; i++)
            {
                int taskId = i + 1;
                tasks[i] = Task.Run(async () =>
                {
                    for (int j = 1; j <= 5; j++) // Each task updates 5 times
                    {
                        await Task.Delay(new Random().Next(100, 500)); // Simulate random work
                        string message = $"Task {taskId} - Update {j}" + Environment.NewLine;

                        _messageQueue.Enqueue(message); // Thread-safe queue
                        UpdateUI();
                    }
                });
            }

            await Task.WhenAll(tasks);
        }

        private void UpdateUI()
        {
            _dispatcher.Invoke(() =>
            {
                if (_messageQueue.TryDequeue(out string newMessage))
                {
                    _textBuilder.AppendLine(DateTime.Now.ToString() + " | " + newMessage + Environment.NewLine + Environment.NewLine); // Append the text
                    StatusText = _textBuilder.ToString();
                }
            });
        }
    }
}
