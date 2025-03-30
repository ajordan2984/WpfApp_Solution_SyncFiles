using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WpfApp_Project_SyncFiles.Interfaces;

namespace WpfApp_Project_SyncFiles.Services
{ 
    public class FolderDialogService : IFolderDialogService
    {
        public string ShowFolderDialog(string initialPath)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.SelectedPath = initialPath;
                dialog.Description = "Select a folder";
                dialog.ShowNewFolderButton = true;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    return dialog.SelectedPath;
                }
            }
            return null; // Return null if no folder is selected
        }
    }
}
