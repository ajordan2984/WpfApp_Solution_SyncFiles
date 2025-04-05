using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using WpfApp_Project_SyncFiles.Commands;
using WpfApp_Project_SyncFiles.Controllers;
using WpfApp_Project_SyncFiles.Helpers;
using WpfApp_Project_SyncFiles.Interfaces;
using WpfApp_Project_SyncFiles.Models;
using WpfApp_Project_SyncFiles.Views;

namespace WpfApp_Project_SyncFiles.ViewModels
{
    class MainWindowViewModel : INotifyPropertyChanged
    {
        public MainWindowViewModel(Dispatcher dispatcher)
        {
            UpdateCommandBrowsePcPath = new RelayCommand(execute => Browse("0"));
            UpdateCommandBrowseExternalFolder1 = new RelayCommand(execute => Browse("1"));
            UpdateCommandBrowseExternalFolder2 = new RelayCommand(execute => Browse("2"));
            UpdateCommandBrowseExternalFolder3 = new RelayCommand(execute => Browse("3"));
            UpdateCommandBrowseExternalFolder4 = new RelayCommand(execute => Browse("4"));
            UpdateCommandSyncFiles = new RelayCommand(async execute => await StartTasksAsync());
            UpdateCommandClearLogUI = new RelayCommand(execute => ClearLogUI());
            TextColor = new SolidColorBrush(Colors.Black);
            AreButtonsEnabled = true;
            _dispatcher = dispatcher;
        }

        #region Private Members
        private bool _areButtonsEnabled;
        private SolidColorBrush _textColor;
        private string _statusText = null;
        private static string _PcPath = null;
        private static string _ExternalFolder1Path = null;
        private static string _ExternalFolder2Path = null;
        private static string _ExternalFolder3Path = null;
        private static string _ExternalFolder4Path = null;
        private readonly Dispatcher _dispatcher;

        private StringBuilder _textBuilder = new();
        private Dictionary<string, string> _ExternalFoldersSelected = new();
        private IErrorCheck _iec = new ErrorCheckHelper();
        #endregion


        #region XAML Bindings
        public bool AreButtonsEnabled
        {
            get { return _areButtonsEnabled; }
            set
            {
                if (_areButtonsEnabled != value)
                {
                    _areButtonsEnabled = value;
                    OnPropertyChanged(nameof(AreButtonsEnabled));
                }
            }
        }
        public SolidColorBrush TextColor
        {
            get { return _textColor; }
            set
            {
                if (_textColor != value)
                {
                    _textColor = value;
                    OnPropertyChanged(nameof(TextColor));
                }
            }
        }
        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                OnPropertyChanged(nameof(StatusText));
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
                OnPropertyChanged(nameof(PcPath));
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
                OnPropertyChanged(nameof(ExternalFolder1Path));
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
                OnPropertyChanged(nameof(ExternalFolder2Path));
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
                OnPropertyChanged(nameof(ExternalFolder3Path));
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
                OnPropertyChanged(nameof(ExternalFolder4Path));
            }
        }

        // Button Clicks
        public RelayCommand UpdateCommandBrowsePcPath { get; set; }
        public RelayCommand UpdateCommandBrowseExternalFolder1 { get; set; }
        public RelayCommand UpdateCommandBrowseExternalFolder2 { get; set; }
        public RelayCommand UpdateCommandBrowseExternalFolder3 { get; set; }
        public RelayCommand UpdateCommandBrowseExternalFolder4 { get; set; }
        public RelayCommand UpdateCommandSyncFiles { get; set; }
        public RelayCommand UpdateCommandClearLogUI { get; set; }
        #endregion

        #region Command Executions
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

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

        private void ClearLogUI()
        {
            _dispatcher.Invoke(() =>
            {
                _textBuilder.Clear();
                StatusText = _textBuilder.ToString();
            });
        }

        public async Task StartTasksAsync()
        {
            try
            {
                HasErrorModel hem = _iec.CheckPaths(PcPath, _ExternalFoldersSelected);

                if (hem.HasError)
                {
                    UpdateTextBlockUI(hem.ErrorMessage);
                }
                else
                {
                    FlipButtonsUI(false);

                    UpdateTextBlockUI("Now starting on syncing your files to the external folder(s) selected.");

                    var gafafh = new GetAllFilesAndFoldersHelper(UpdateTextBlockUI);
                    var pcFiles = gafafh.GetAllFiles(gafafh.GetAllDirectories(PcPath));
                    var dictionary = new ConcurrentDictionary<string, FileInfoHolderModel>(pcFiles);
                    var tasks = new List<Task>();

                    foreach (var folderTextBoxKey in _ExternalFoldersSelected.Keys)
                    {
                        string externalFolder = _ExternalFoldersSelected[folderTextBoxKey];
                        string pcFolderFromTextBox = PcPath;

                        tasks.Add(
                          Task.Run(() =>
                          {
                              var _main = new SyncFilesFromPcToExternalDriveController(UpdateTextBlockUI);
                              _main.SetAllSortedFilesFromPcPath(dictionary);
                              _main.SetPaths(pcFolderFromTextBox, externalFolder);
                              _main.SyncFiles();
                          }));
                    }

                    await Task.WhenAll(tasks);
                    FlipButtonsUI(true);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        #endregion

        public void FlipButtonsUI(bool isEnabled)
        {
            _dispatcher.Invoke(() =>
            {
                AreButtonsEnabled = isEnabled;
            });
        }

        public void ChangeTextColor()
        {
            TextColor = TextColor == System.Windows.Media.Brushes.Red ? System.Windows.Media.Brushes.Blue : System.Windows.Media.Brushes.Red;
        }

        private void UpdateTextBlockUI(string newMessage)
        {
            try
            {
                _dispatcher.BeginInvoke(new Action(() => {
                    _textBuilder.AppendLine(DateTime.Now.ToString() + " | " + newMessage + Environment.NewLine + Environment.NewLine);
                    StatusText = _textBuilder.ToString();
                }));
            }
            catch (Exception ex)
            {
                UpdateTextBlockUI(ex.Message);
            }
        }

        private void AddSelectedExternalFolder(string TextBoxName, string TextBoxPath)
        {
            if (string.IsNullOrEmpty(TextBoxPath))
            {
                if (_ExternalFoldersSelected.ContainsKey(TextBoxName))
                {
                    _ExternalFoldersSelected.Remove(TextBoxName);
                }
            }
            else
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
}
