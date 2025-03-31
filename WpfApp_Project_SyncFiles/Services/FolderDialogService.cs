using WpfApp_Project_SyncFiles.Interfaces;
using Microsoft.Win32;
using System.IO;

namespace WpfApp_Project_SyncFiles.Services
{ 
    public class FolderDialogService : IFolderDialogService
    {
        private string _selectedFolder = null;


        public string ShowFolderDialog()
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Title = "Select a folder",
                CheckFileExists = false,
                CheckPathExists = true,
                FileName = "Select Folder"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedFolder = Path.GetDirectoryName(openFileDialog.FileName);
            }
            return _selectedFolder;
        }
    }
}
