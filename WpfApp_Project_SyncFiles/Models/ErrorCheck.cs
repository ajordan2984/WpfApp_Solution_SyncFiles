using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Controls;
using WpfApp_Project_SyncFiles.Interfaces;

namespace WpfApp_Project_SyncFiles.HelperClasses
{
    public class ErrorCheck : IErrorCheck
    {
        IFolderDialogService _FDS;

        public ErrorCheck()
        {
        }

        public void SetFolderDialogService(IFolderDialogService folderDialogService)
        {
            _FDS = folderDialogService;
        }

        void IErrorCheck.selectDirectory(IAppendColoredText iact, TextBlock rtb, TextBox tb)
        {
            string result = _FDS.ShowFolderDialog();

            if (!string.IsNullOrWhiteSpace(result) && Directory.Exists(result))
            {
                tb.Text = result;
            }
            else
            {
                iact.AppendText("Error: Please select a folder.");
                tb.Text = "";
            }
        }

        Triple<bool, string, Color> IErrorCheck.CheckPaths(string pcFolder, List<TextBox> textBoxes)
        {
            if (string.IsNullOrEmpty(pcFolder))
            {
                return new Triple<bool, string, Color>(false, "Error: The PC path cannot be empty. Please Try again.", Color.Red);
            }

            foreach (var tb in textBoxes)
            {
                tb.Text = tb.Text.Trim();
                
                if (tb.Text == pcFolder)
                {
                    return new Triple<bool, string, Color>(false, "Error: The PC path and External Path cannot be the same. Please Try again.", Color.Red);
                }

                string error = StartCheck(pcFolder, tb);

                if (!string.IsNullOrEmpty(error))
                {
                    return new Triple<bool, string, Color>(false, error, Color.Red);
                }
            }
            
            return new Triple<bool, string, Color>(true, "", Color.Black);
        }

        private string StartCheck(string PathToFilesOnPc, TextBox tb)
        {
            if (!Directory.Exists(PathToFilesOnPc))
            {
                return $"Error: Sorry the path on your PC: \"{PathToFilesOnPc}\" does not exist. Please try again.";
            }

            if ((tb.Name == "externalFolder2" && string.IsNullOrEmpty(tb.Text)) || 
                (tb.Name == "externalFolder3" && string.IsNullOrEmpty(tb.Text)) ||
                (tb.Name == "externalFolder4" && string.IsNullOrEmpty(tb.Text)))
            {
                return null;
            }

            if ((tb.Name == "externalFolder1" && string.IsNullOrEmpty(tb.Text)))
            {
                return "Error: Sorry you must have a valid path in the \"External Folder 1\" textbox. Please try again.";
            }

            if (!Directory.Exists(tb.Text))
            {
                return $"Error: Sorry the path on your External Drive: {tb.Text} does not exist. Please try again.";
            }

            string pathA = Path.GetFileName(PathToFilesOnPc);
            string pathB = Path.GetFileName(tb.Text);

            if (Path.GetFileName(pathA) != Path.GetFileName(pathB))
            {
                return $"Error: Sorry the end of path: {PathToFilesOnPc} does not match the end of path {tb.Text}. Please try again.";
            }

            return null;
        }
    }
}
