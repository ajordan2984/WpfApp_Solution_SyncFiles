using System.Collections.Generic;
using System.IO;
using WpfApp_Project_SyncFiles.Interfaces;
using WpfApp_Project_SyncFiles.Models;

namespace WpfApp_Project_SyncFiles.Helpers
{
    public class ErrorCheck : IErrorCheck
    {
        public ErrorCheck()
        {
        }
        
        HasErrorModel IErrorCheck.CheckPaths(string pcFolderPath, List<TextBoxModel> textBoxPaths)
        {
            if (string.IsNullOrEmpty(pcFolderPath))
            {
                return new HasErrorModel(true, "Error: The PC path cannot be empty. Please Try again.");
            }

            if (!Directory.Exists(pcFolderPath))
            {
                return new HasErrorModel(true, $"Error: Sorry the path on your PC: \"{pcFolderPath}\" does not exist. Please try again.");
            }

            foreach (TextBoxModel tbm in textBoxPaths)
            {
                if (tbm.TextBoxText == pcFolderPath)
                {
                    return new HasErrorModel(true, "Error: The PC path and External Path cannot be the same. Please Try again.");
                }

                string error = StartCheck(pcFolderPath, tbm);

                if (!string.IsNullOrEmpty(error))
                {
                    return new HasErrorModel(true, error);
                }
            }
            
            return new HasErrorModel(false, null);
        }

        private string StartCheck(string pcFolderPath, TextBoxModel tbm)
        {
            if (!Directory.Exists(tbm.TextBoxText))
            {
                return $"Error: Sorry the path to your external drive: \"{tbm.TextBoxText}\" does not exist. Please try again.";
            }

            if (tbm.TextBoxName == "ExternalFolder1Path" && string.IsNullOrEmpty(tbm.TextBoxText))
            {
                return "Error: Sorry you must have a valid path in the \"External Folder 1\" textbox. Please try again.";
            }

            if (
                (tbm.TextBoxName == "ExternalFolder2Path" || tbm.TextBoxName == "ExternalFolder3Path" || tbm.TextBoxName == "ExternalFolder4Path")
                && string.IsNullOrEmpty(tbm.TextBoxText)
               )
            {
                return null;
            }

            string shortPcFolderPath = Path.GetFileName(pcFolderPath);
            string shortExternalDrivePath = Path.GetFileName(tbm.TextBoxText);

            if (Path.GetFileName(shortPcFolderPath) != Path.GetFileName(shortExternalDrivePath))
            {
                return $"Error: Sorry the end of path: {shortPcFolderPath} does not match the end of path {shortExternalDrivePath}. Please try again.";
            }

            return null;
        }
    }
}
