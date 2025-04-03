using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using WpfApp_Project_SyncFiles.Commands;
using WpfApp_Project_SyncFiles.Helpers;
using WpfApp_Project_SyncFiles.Interfaces;
using WpfApp_Project_SyncFiles.Models;
using WpfApp_Project_SyncFiles.Views;

namespace WpfApp_Project_SyncFiles.ViewModels
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };
      
        public MainWindowViewModel(Dispatcher dispatcher)
        {
            UpdateCommandBrowsePcPath = new RelayCommand(execute => Browse("0"));
            UpdateCommandBrowseExternalFolder1 = new RelayCommand(execute => Browse("1"));
            UpdateCommandBrowseExternalFolder2 = new RelayCommand(execute => Browse("2"));
            UpdateCommandBrowseExternalFolder3 = new RelayCommand(execute => Browse("3"));
            UpdateCommandBrowseExternalFolder4 = new RelayCommand(execute => Browse("4"));
            UpdateCommandSyncFiles = new RelayCommand(execute => SyncFiles());
            UpdateCommandClearLogUI = new RelayCommand(execute => ClearLogUI());

            _dispatcher = dispatcher;
        }

        #region Private Members
        private static string _PcPath = null;
        private static string _ExternalFolder1Path = null;
        private static string _ExternalFolder2Path = null;
        private static string _ExternalFolder3Path = null;
        private static string _ExternalFolder4Path = null;
        private string _statusText = null;
        private readonly Dispatcher _dispatcher;
        
        private readonly ConcurrentQueue<string> _messageQueue = new();
        private StringBuilder _textBuilder = new();
        private Dictionary<string, string> _ExternalFoldersSelected = new();
        private IErrorCheck _iec = new ErrorCheckHelper();
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
        public string ExternalFolder1Path
        {
            get
            {
                return _ExternalFolder1Path;
            }
            set
            {
                _ExternalFolder1Path = value;
                AddSelectedExternalFolder("ExternalFolder1Path", value);
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(ExternalFolder1Path)));
            }
        }
        public string ExternalFolder2Path
        {
            get
            {
                return _ExternalFolder2Path;
            }
            set
            {
                _ExternalFolder2Path = value;
                AddSelectedExternalFolder("ExternalFolder2Path", value);
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(ExternalFolder2Path)));
            }
        }
        public string ExternalFolder3Path
        {
            get
            {
                return _ExternalFolder3Path;
            }
            set
            {
                _ExternalFolder3Path = value;
                AddSelectedExternalFolder("ExternalFolder3Path", value);
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(ExternalFolder3Path)));
            }
        }
        public string ExternalFolder4Path
        {
            get
            {
                return _ExternalFolder4Path;
            }
            set
            {
                _ExternalFolder4Path = value;
                AddSelectedExternalFolder("ExternalFolder4Path", value);
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(ExternalFolder4Path)));
            }
        }
        #endregion


        #region Button Clicks
        public RelayCommand UpdateCommandBrowsePcPath { get; set; }
        public RelayCommand UpdateCommandBrowseExternalFolder1 { get; set; }
        public RelayCommand UpdateCommandBrowseExternalFolder2 { get; set; }
        public RelayCommand UpdateCommandBrowseExternalFolder3 { get; set; }
        public RelayCommand UpdateCommandBrowseExternalFolder4 { get; set; }
        public RelayCommand UpdateCommandSyncFiles { get; set; }
        public RelayCommand UpdateCommandClearLogUI { get; set; }
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
                    case "0":
                        PcPath = path;
                        break;
                    case "1":
                        ExternalFolder1Path = path;
                        break;
                    case "2":
                        ExternalFolder2Path = path;
                        break;
                    case "3":
                        ExternalFolder3Path = path;
                        break;
                    case "4":
                        ExternalFolder4Path = path;
                        break;
                    default:
                        MessageBox.Show("An error occured in the Browse function.", "Alert");
                        break;
                }
            }
        }

        public void SyncFiles()
        {
            try
            {
                HasErrorModel hem = _iec.CheckPaths(PcPath, _ExternalFoldersSelected);

                if (hem.HasError)
                {
                    MessageBox.Show(hem.ErrorMessage);
                }
                else
                {
                    MessageBox.Show("No Errors!");
                }
            }
            catch (Exception ex)
            {
                _messageQueue.Enqueue(ex.Message); // Thread-safe queue
                UpdateUI();
            }
        }

        private void ClearLogUI()
        {
            _dispatcher.Invoke(() =>
            {
                _textBuilder.Clear();
                StatusText = _textBuilder.ToString();
            });
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
                        string message = $"Task {taskId} - Update {j}";

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

        private void AddSelectedExternalFolder(string TextBoxName, string TextBoxPath)
        {
            if (_ExternalFoldersSelected.ContainsKey(TextBoxName))
            {
                _ExternalFoldersSelected[TextBoxName] = TextBoxPath.Trim();
            }
            else
            {
                _ExternalFoldersSelected.Add(TextBoxName, TextBoxPath.Trim());
            }
        }
    }
}
