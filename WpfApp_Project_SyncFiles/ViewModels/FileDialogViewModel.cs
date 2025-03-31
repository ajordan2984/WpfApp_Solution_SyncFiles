using System;
using System.Windows;
using System.Windows.Input;
using WpfApp_Project_SyncFiles.Commands;
using WpfApp_Project_SyncFiles.Interfaces;
using WpfApp_Project_SyncFiles.Models;

namespace WpfApp_Project_SyncFiles.ViewModels
{
    public class FileDialogViewModel
    {
        #region Constructors
        public FileDialogViewModel(Action windowClose)
        {
            FileDialogTree = new TreeServiceModel();
            UpdateCommandCloseWindow = new ButtonCommands(windowClose);
            UpdateCommandSelectPicture = new ButtonCommands(() =>
            {
                if (FileDialogTree.SelectedItem != null && FileDialogTree.SelectedItem.GetType() == typeof(FolderNodeModel))
                {
                    windowClose();
                }
                else
                {
                    MessageBox.Show("Please select a folder", "Alert");
                }
            });
        }
        #endregion

        #region Button Clicks (Commands)
        public ICommand UpdateCommandCloseWindow { get; private set; }
        public ICommand UpdateCommandSelectPicture { get; private set; }
        #endregion

        #region Public Members
        public ITreeServiceModel FileDialogTree { get; set; }
        #endregion
    }
}
