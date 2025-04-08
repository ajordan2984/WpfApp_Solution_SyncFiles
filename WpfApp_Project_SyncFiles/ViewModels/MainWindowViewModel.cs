﻿using System;
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
        #region Private Members
        private bool _areTextBoxesEnabled;
        private bool _areButtonsEnabled;
        private SolidColorBrush _textColor;
        private string _statusText;
        private static string _PcPath;
        private static string _ExternalFolder1Path;
        private static string _ExternalFolder2Path;
        private static string _ExternalFolder3Path;
        private static string _ExternalFolder4Path;

        private readonly Dispatcher _dispatcher;
        private TrackTextBoxUpdatesHelper _ttbuh;
        private StringBuilder _textBuilder;
        private IErrorCheck _iec;
        #endregion

        public MainWindowViewModel(Dispatcher dispatcher)
        {
            _areTextBoxesEnabled = true;
            _areButtonsEnabled = true;
            _textColor = new SolidColorBrush(Colors.Black);
            UpdateCommandBrowsePcPath = new RelayCommand(execute => Browse("0"));
            UpdateCommandBrowseExternalFolder1 = new RelayCommand(execute => Browse("1"));
            UpdateCommandBrowseExternalFolder2 = new RelayCommand(execute => Browse("2"));
            UpdateCommandBrowseExternalFolder3 = new RelayCommand(execute => Browse("3"));
            UpdateCommandBrowseExternalFolder4 = new RelayCommand(execute => Browse("4"));
            UpdateCommandSyncFiles = new RelayCommand(async execute => await StartTasksAsync());
            UpdateCommandClearLogUI = new RelayCommand(execute => ClearLogUI());
            
            _dispatcher = dispatcher;
            _ttbuh = new();
            _textBuilder = new();
            _iec = new ErrorCheckHelper();
        }

        #region XAML Bindings
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
                HasErrorModel hem = _iec.CheckPaths(PcPath, _ttbuh.ExternalFoldersSelected);

                if (hem.HasError)
                {
                    UpdateTextBlockUI(hem.ErrorMessage);
                }
                else
                {
                    FlipTextBoxesUI(false);
                    FlipButtonsUI(false);

                    UpdateTextBlockUI("Now starting on syncing your files to the external folder(s) selected.");

                    GetAllFilesAndFoldersHelper gafafh = new(UpdateTextBlockUI);
                    SortedDictionary<string, FileInfoHolderModel> pcFiles = gafafh.GetAllFiles(gafafh.GetAllDirectories(PcPath));
                    ConcurrentDictionary<string, FileInfoHolderModel> dictionary = new(pcFiles);
                    List<Task> tasks = new();

                    foreach (string folderTextBoxKey in _ttbuh.ExternalFoldersSelected.Keys)
                    {
                        string externalFolder = _ttbuh.ExternalFoldersSelected[folderTextBoxKey];
                        string pcFolderFromTextBox = PcPath;

                        tasks.Add(
                          Task.Run(() =>
                          {
                              SyncFilesFromPcToExternalDriveController _main = new(UpdateTextBlockUI);
                              _main.SetAllSortedFilesFromPcPath(dictionary);
                              _main.SetPaths(pcFolderFromTextBox, externalFolder);
                              _main.SyncFiles();
                              UpdateTextBlockUI($"Completed syncing your files to \"{externalFolder}\".");
                          }));
                    }

                    await Task.WhenAll(tasks);
                    UpdateTextBlockUI($"Completed syncing all your files to the external folders selected.");

                    FlipTextBoxesUI(true);
                    FlipButtonsUI(true);
                }
            }
            catch (Exception ex)
            {
                _ = MessageBox.Show(ex.Message);
            }
        }
        #endregion

        public void FlipTextBoxesUI(bool isEnabled)
        {
            _dispatcher.BeginInvoke(() =>
            {
                AreTextBoxesEnabled = isEnabled;
            });
        }
        
        public void FlipButtonsUI(bool isEnabled)
        {
            _dispatcher.BeginInvoke(() =>
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
                    _textBuilder.AppendLine(DateTime.Now.ToString() + " | " + newMessage + Environment.NewLine);
                    StatusText = _textBuilder.ToString();
                }));
            }
            catch (Exception ex)
            {
                UpdateTextBlockUI(ex.Message);
            }
        }
    }
}
