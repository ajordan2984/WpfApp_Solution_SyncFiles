using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
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
        #region Private Members
        private bool _areTextBoxesEnabled;
        private bool _areButtonsEnabled;
        private static string _PcPath;
        private static string _ExternalFolder1Path;
        private static string _ExternalFolder2Path;
        private static string _ExternalFolder3Path;
        private static string _ExternalFolder4Path;

        private readonly Dispatcher _dispatcher;
        private TrackTextBoxUpdatesHelper _ttbuh;
        private IErrorCheck _iec;
        CancellationTokenSource _cts;
        #endregion

        public MainWindowViewModel(Dispatcher dispatcher)
        {
            Inlines = new ObservableCollection<Inline>();
            _areTextBoxesEnabled = true;
            _areButtonsEnabled = true;
            UpdateCommandBrowsePcPath = new RelayCommand(execute => Browse("0"));
            UpdateCommandBrowseExternalFolder1 = new RelayCommand(execute => Browse("1"));
            UpdateCommandBrowseExternalFolder2 = new RelayCommand(execute => Browse("2"));
            UpdateCommandBrowseExternalFolder3 = new RelayCommand(execute => Browse("3"));
            UpdateCommandBrowseExternalFolder4 = new RelayCommand(execute => Browse("4"));
            UpdateCommandSyncFiles = new RelayCommand(async execute => await StartTasksAsync());
            UpdateCommandClearLogUI = new RelayCommand(execute => ClearLogUI());
            UpdateCommandCancelSync = new RelayCommand(execute => CancelSync());

            _dispatcher = dispatcher;
            _ttbuh = new();
            _iec = new ErrorCheckHelper();
            _cts = null;
        }

        #region XAML Bindings
        public ObservableCollection<Inline> Inlines { get; }
        public bool AreTextBoxesEnabled
        {
            get => _areTextBoxesEnabled;
            set
            {
                if (_areTextBoxesEnabled != value)
                {
                    _areTextBoxesEnabled = value;
                    OnPropertyChanged(nameof(AreTextBoxesEnabled));
                }
            }
        }
        public bool AreButtonsEnabled
        {
            get => _areButtonsEnabled;
            set
            {
                if (_areButtonsEnabled != value)
                {
                    _areButtonsEnabled = value;
                    OnPropertyChanged(nameof(AreButtonsEnabled));
                }
            }
        }
        public string PcPath
        {
            get => _PcPath;
            set
            {
                _PcPath = value;
                OnPropertyChanged(nameof(PcPath));
            }
        }
        public string ExternalFolder1Path
        {
            get => _ExternalFolder1Path;
            set
            {
                _ExternalFolder1Path = value;
                _ttbuh.AddSelectedExternalFolder("ExternalFolder1Path", value);
                OnPropertyChanged(nameof(ExternalFolder1Path));
            }
        }
        public string ExternalFolder2Path
        {
            get => _ExternalFolder2Path;
            set
            {
                _ExternalFolder2Path = value;
                _ttbuh.AddSelectedExternalFolder("ExternalFolder2Path", value);
                OnPropertyChanged(nameof(ExternalFolder2Path));
            }
        }
        public string ExternalFolder3Path
        {
            get => _ExternalFolder3Path;
            set
            {
                _ExternalFolder3Path = value;
                _ttbuh.AddSelectedExternalFolder("ExternalFolder3Path", value);
                OnPropertyChanged(nameof(ExternalFolder3Path));
            }
        }
        public string ExternalFolder4Path
        {
            get => _ExternalFolder4Path;
            set
            {
                _ExternalFolder4Path = value;
                _ttbuh.AddSelectedExternalFolder("ExternalFolder4Path", value);
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
        public RelayCommand UpdateCommandCancelSync { get; set; }
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

        public async Task StartTasksAsync()
        {
            try
            {
                HasErrorModel hem = _iec.CheckPaths(PcPath, _ttbuh.ExternalFoldersSelected);

                if (hem.HasError)
                {
                    UpdateTextBlockUI(hem.ErrorMessage, Brushes.Red);
                }
                else
                {
                    _cts = new CancellationTokenSource();
                    FlipTextBoxesUI(false);
                    FlipButtonsUI(false);

                    UpdateTextBlockUI("Now starting on syncing your files to the external folder(s) selected.", Brushes.Black);

                    GetAllFilesAndFoldersHelper gafafh = new(UpdateTextBlockUI, _cts.Token);

                    List<string> allSelectedPcFolders = gafafh.GetAllDirectories(PcPath);
                    SortedDictionary<string, FileInfoHolderModel> allSelectedPcFiles = gafafh.GetAllFiles(allSelectedPcFolders);
                    ConcurrentDictionary<string, FileInfoHolderModel> allSeclectedPcFilesForTasks = new(allSelectedPcFiles);
                    List<Task> tasks = new();

                    foreach (string folderTextBoxKey in _ttbuh.ExternalFoldersSelected.Keys)
                    {
                        string externalFolder = _ttbuh.ExternalFoldersSelected[folderTextBoxKey];
                        string pcFolderFromTextBox = PcPath;

                        tasks.Add(
                          Task.Run(() =>
                          {
                              SyncFilesFromPcToExternalDriveController _main = new(UpdateTextBlockUI, _cts.Token);
                              _main.SetAllSortedFilesFromPcPath(allSeclectedPcFilesForTasks);
                              _main.SetPaths(pcFolderFromTextBox, externalFolder);
                              bool completed = _main.SyncFiles();

                              if (completed)
                              {
                                  UpdateTextBlockUI($"Completed syncing your files to \"{externalFolder}\".", Brushes.Black);
                              }
                              else
                              {
                                  UpdateTextBlockUI($"Syncing your files to \"{externalFolder}\" was Canceled by the user.", Brushes.Red);
                                  _main.RemoveUpdateChangesFile();
                              }
                          }));
                    }

                    await Task.WhenAll(tasks);

                    FlipTextBoxesUI(true);
                    FlipButtonsUI(true);
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show(ex.Message);
            }
        }

        private void ClearLogUI()
        {
            _dispatcher.Invoke(() =>
            {
                Inlines.Clear();
            });
        }

        private void CancelSync()
        {
            try
            {
                if (_cts == null)
                {
                    return;
                }

                if (_cts != null)
                {
                    _cts.Cancel();
                }
            }
            catch (Exception ex)
            {
                UpdateTextBlockUI(ex.Message, Brushes.Red);
            }
        }
        #endregion

        public void FlipTextBoxesUI(bool isEnabled)
        {
            _ = _dispatcher.BeginInvoke(() =>
              {
                  AreTextBoxesEnabled = isEnabled;
              });
        }

        public void FlipButtonsUI(bool isEnabled)
        {
            _ = _dispatcher.BeginInvoke(() =>
              {
                  AreButtonsEnabled = isEnabled;
              });
        }

        private void UpdateTextBlockUI(string newMessage, SolidColorBrush textColor)
        {
            try
            {
                _ = _dispatcher.BeginInvoke(new Action(() =>
                  {
                      string text = DateTime.Now.ToString() + " | " + newMessage + Environment.NewLine + Environment.NewLine;
                      Run run = new(text) { Foreground = textColor };
                      Inlines.Add(run);
                  }));
            }
            catch (Exception ex)
            {
                UpdateTextBlockUI(ex.Message, Brushes.Red);
            }
        }
    }
}
