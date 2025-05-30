using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using WpfApp_Project_SyncFiles.Interfaces;
using WpfApp_Project_SyncFiles.Models;

namespace WpfApp_Project_SyncFiles.Helpers
{
    public class ErrorCheckHelper : IErrorCheck
    {
        public ErrorCheckHelper()
        {
        }

        public void AddOrRemoveListBoxItem(bool add, ObservableCollection<string> ListBoxItems, string folder)
        {
            if (add)
            {
                ListBoxItems.Remove(folder);
                ListBoxItems.Add(folder);
            }
            else
            {
                if (ListBoxItems != null && !string.IsNullOrEmpty(folder))
                {
                    ListBoxItems.Remove(folder);
                }
            }
        }
        
        HasErrorModel IErrorCheck.CheckPaths(string pcFolderPath, Dictionary<string, string> textBoxes)
        {
            if (string.IsNullOrEmpty(pcFolderPath))
            {
                return new HasErrorModel(true, "Error: The PC folder cannot be empty. Please Try again.");
            }

            if (!Directory.Exists(pcFolderPath))
            {
                return new HasErrorModel(true, $"Error: Sorry the folder on your PC: \"{pcFolderPath}\" does not exist. Please try again.");
            }

            if (textBoxes.Count == 0)
            {
                return new HasErrorModel(true, $"Error: You must have one external folder path selected. Please try again.");
            }
            
            foreach (string textBoxName in textBoxes.Keys)
            {
                string externalFolderPath = textBoxes[textBoxName];

                if (externalFolderPath == pcFolderPath)
                {
                    return new HasErrorModel(true, "Error: The PC folder and External folder cannot be the same. Please Try again.");
                }

                if (!Directory.Exists(externalFolderPath))
                {
                    return new HasErrorModel(true, $"Error: Sorry the folder on your external drive: \"{externalFolderPath}\" does not exist. Please try again.");
                }

                string shortPcFolderPath = Path.GetFileName(pcFolderPath);
                string shortExternalFolderPath = Path.GetFileName(externalFolderPath);

                if (shortPcFolderPath != shortExternalFolderPath)
                {
                    return new HasErrorModel(true, $"Error: Sorry the end of your PC folder: \"{pcFolderPath}\" does not match the end of the folder \"{externalFolderPath}\" on your external drive. Please try again.");
                }
            }
            
            return new HasErrorModel(false, null);
        }
    }
}
