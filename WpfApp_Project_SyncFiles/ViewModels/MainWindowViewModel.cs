using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
        private string _selectedSkipFoldersListBoxItem;
        private bool _areTextBoxesEnabled;
        private bool _areButtonsEnabled;
        private bool _isProgressBarRunning;

        #region Status Bar Progress
        private long _totalItems;
        private int _progressBarValue;
        private int _completedItems;
        #endregion

        private static string _PcPath;
        private static string _ManualTextBoxExcludedPath;
        private static string _ExternalFolder1Path;
        private static string _ExternalFolder2Path;
        private static string _ExternalFolder3Path;
        private static string _ExternalFolder4Path;

        public static string _userSavedFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}UserSavedFilePaths.txt";
        public static string _userExcludedFilePath = $"{AppDomain.CurrentDomain.BaseDirectory}UserExcludedFilePaths.txt";

        private readonly Dispatcher _dispatcher;
        private TrackTextBoxUpdatesHelper _ttbuh;
        private IErrorCheck _iec;
        private ILoadFavorites _ilf;
        CancellationTokenSource _cts;
        #endregion

        public MainWindowViewModel(Dispatcher dispatcher)
        {
            SavedFoldersListBoxItems = new ObservableCollection<string>();
            SkipFoldersListBoxItems = new ObservableCollection<string>();
            Inlines = new ObservableCollection<Inline>();
            _areTextBoxesEnabled = true;
            _areButtonsEnabled = true;
            UpdateCommandBrowsePcPath = new RelayCommand(execute => Browse("PcPath"));
            UpdateCommandSavePcPath = new RelayCommand(execute => AddOrRemoveSavedFoldersListBoxItem(true));
            UpdateCommandRemovePcPath = new RelayCommand(execute => AddOrRemoveSavedFoldersListBoxItem(false));
            ManualButtonExcludedPcPath = new RelayCommand(execute => AddOrRemoveSkippedFoldersListBoxItem(true));
            AddExcludedPcPath = new RelayCommand(execute => Browse("SkipFolderListBoxItemAdd"));
            RemoveExcludedPcPath = new RelayCommand(execute => AddOrRemoveSkippedFoldersListBoxItem(false));
            UpdateCommandBrowseExternalFolder1 = new RelayCommand(execute => Browse("ExternalFolder1Path"));
            UpdateCommandBrowseExternalFolder2 = new RelayCommand(execute => Browse("ExternalFolder2Path"));
            UpdateCommandBrowseExternalFolder3 = new RelayCommand(execute => Browse("ExternalFolder3Path"));
            UpdateCommandBrowseExternalFolder4 = new RelayCommand(execute => Browse("ExternalFolder4Path"));

            UpdateCommandSyncFiles = new RelayCommand(async execute => await StartTasksAsync());
            UpdateCommandClearLogUI = new RelayCommand(execute => ClearLogUI());
            UpdateCommandCancelSync = new RelayCommand(execute => CancelSync());

            _dispatcher = dispatcher;
            _ttbuh = new();
            _iec = new ErrorCheckHelper();
            _ilf = new LoadFavorites();
            _ilf.LoadUserSelectedListBoxItems(SavedFoldersListBoxItems, _userSavedFilePath);
            _ilf.LoadUserSelectedListBoxItems(SkipFoldersListBoxItems, _userExcludedFilePath);
            _cts = null;
        }

        #region XAML Bindings
        public string SelectedSkipFoldersListBoxItem
        {
            get => _selectedSkipFoldersListBoxItem;
            set
            {
                _selectedSkipFoldersListBoxItem = value;
                OnPropertyChanged(nameof(_selectedSkipFoldersListBoxItem));
            }
        }
        public ObservableCollection<string> SavedFoldersListBoxItems { get; set; }
        public ObservableCollection<string> SkipFoldersListBoxItems { get; set; }
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
        public bool IsProgressBarRunning
        {
            get => _isProgressBarRunning;
            set
            {
                if (_isProgressBarRunning != value)
                {
                    _isProgressBarRunning = value;
                    OnPropertyChanged(nameof(IsProgressBarRunning));
                }
            }
        }
        public int ProgressBarValue
        {
            get
            {
                return _progressBarValue;
            }
            set
            {
                _progressBarValue = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(ProgressBarValue)));
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
        public string ManualTextBoxExcludedPath
        {
            get => _ManualTextBoxExcludedPath;
            set
            {
                _ManualTextBoxExcludedPath = value;
                OnPropertyChanged(nameof(ManualTextBoxExcludedPath));
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
        public RelayCommand UpdateCommandSavePcPath { get; set; }
        public RelayCommand UpdateCommandRemovePcPath { get; set; }
        public RelayCommand ManualButtonExcludedPcPath { get; set; }
        public RelayCommand AddExcludedPcPath { get; set; }
        public RelayCommand RemoveExcludedPcPath { get; set; }
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

        public void AddOrRemoveSavedFoldersListBoxItem(bool add)
        {
            if (add)
            {
                _ilf.AddOrRemoveListBoxItem(true, SavedFoldersListBoxItems, PcPath, _userSavedFilePath, UpdateTextBlockUI);
            }
            else
            {

                _ilf.AddOrRemoveListBoxItem(false, SavedFoldersListBoxItems, PcPath, _userSavedFilePath, UpdateTextBlockUI);
            }
        }

        public void AddOrRemoveSkippedFoldersListBoxItem(bool add)
        {
            if (add)
            {
                _ilf.AddOrRemoveListBoxItem(true, SkipFoldersListBoxItems, ManualTextBoxExcludedPath, _userExcludedFilePath, UpdateTextBlockUI);
                ManualTextBoxExcludedPath = string.Empty;
            }
            else
            {
                
                _ilf.AddOrRemoveListBoxItem(false, SkipFoldersListBoxItems, SelectedSkipFoldersListBoxItem, _userExcludedFilePath, UpdateTextBlockUI);
            }
        }

        public void Browse(string selectedTextBox)
        {
            FileDialogView childView = new();
            FileDialogViewModel childModel = new(childView.Close);
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
                    case "ExternalFolder1Path":
                        ExternalFolder1Path = path;
                        break;
                    case "ExternalFolder2Path":
                        ExternalFolder2Path = path;
                        break;
                    case "ExternalFolder3Path":
                        ExternalFolder3Path = path;
                        break;
                    case "ExternalFolder4Path":
                        ExternalFolder4Path = path;
                        break;
                    case "SkipFolderListBoxItemAdd":
                        _ilf.AddOrRemoveListBoxItem(true, SkipFoldersListBoxItems, path, _userExcludedFilePath, UpdateTextBlockUI);
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
                    FlipProgressBarUI(true);

                    UpdateTextBlockUI("Now starting on syncing your files to the external folder(s) selected.", Brushes.Black);

                    ConcurrentQueue<string> pcLogMessages = new();
                    HelperFunctions hf = new(_cts.Token, pcLogMessages);
                    hf.SetStartingDirectory(PcPath);
                    hf.SetUpdateTextBlockOnUI(UpdateTextBlockUI);

                    ConcurrentBag<string> ConcurrentSkipFoldersBag = _ilf.CreateNewSkipFoldersBag(hf.ShortenedPath(PcPath), SkipFoldersListBoxItems);

                    await Task.Run(async () =>
                    {
                        List<string> allSelectedPcFolders = hf.GetAllDirectories(PcPath, ConcurrentSkipFoldersBag);
                        ConcurrentDictionary<string, FileInfoHolderModel> allSelectedPcFiles = hf.GetAllFiles(allSelectedPcFolders);
                        ConcurrentDictionary<string, FileInfoHolderModel> allSeclectedPcFilesForTasks = new(allSelectedPcFiles);
                        
                        _totalItems = hf.CalculateTotalFileSize(allSelectedPcFiles);

                        List<Task> tasks = new();

                        foreach (string folderTextBoxKey in _ttbuh.ExternalFoldersSelected.Keys)
                        {
                            string externalFolder = _ttbuh.ExternalFoldersSelected[folderTextBoxKey];
                            string pcFolderFromTextBox = PcPath;

                            tasks.Add(
                              Task.Run(() =>
                              {
                                  SyncFilesFromPcToExternalDriveController _main = new(_cts.Token);
                                  _main.SetUpdateTextBlockOnUI(UpdateTextBlockUI);
                                  _main.SetConcurrentSkipFoldersBag(ConcurrentSkipFoldersBag);
                                  _main.SetAllSortedFilesFromPcPath(allSeclectedPcFilesForTasks);
                                  _main.SetPaths(pcFolderFromTextBox, externalFolder);
                                  bool completed = _main.SyncFiles();
                                  UpdateTextBlockUI($"Writing Log file to \"{externalFolder}\".", Brushes.Blue);
                                  _main.WriteLogFile(pcLogMessages);
                                  UpdateTextBlockUI($"Completed writing Log file to \"{externalFolder}\".", Brushes.Blue);

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
                    });

                    FlipTextBoxesUI(true);
                    FlipButtonsUI(true);
                    FlipProgressBarUI(false);
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

        public void FlipProgressBarUI(bool isEnabled)
        {
            _ = _dispatcher.BeginInvoke(() =>
            {
                IsProgressBarRunning = isEnabled;
            });
        }

        private void UpdateTextBlockUI(string newMessage, SolidColorBrush textColor)
        {
            try
            {
                _ = _dispatcher.BeginInvoke(new Action(() =>
                  {
                      string text = $"{newMessage}{Environment.NewLine}{Environment.NewLine}";
                      Run run = new(text) { Foreground = textColor };
                      Inlines.Add(run);
                  }));
            }
            catch (Exception ex)
            {
                UpdateTextBlockUI(ex.Message, Brushes.Red);
            }
        }

        public void IncrementProgress(int CompletedItems)
        {
            try
            {
                _ = _dispatcher.BeginInvoke(new Action(() =>
                {
                    _completedItems += CompletedItems;

                    if (_completedItems == _totalItems)
                    {
                        ProgressBarValue = 100;
                    }
                    else
                    {
                        float completedItemsFloat = _completedItems;
                        float totalItemsFloat = _totalItems;
                        ProgressBarValue = (int)((completedItemsFloat / totalItemsFloat) * 100);
                    }
                }));
            }
            catch (Exception ex)
            {
                UpdateTextBlockUI(ex.Message, Brushes.Red);
            }
        }
    }
}
