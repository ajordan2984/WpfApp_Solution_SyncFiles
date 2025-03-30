using System.Collections.Generic;
using System.Drawing;
using System.Windows.Controls;
using WpfApp_Project_SyncFiles.HelperClasses;

namespace WpfApp_Project_SyncFiles.Interfaces
{
    interface IErrorCheck
    {
        void selectDirectory(IAppendColoredText iact, TextBlock textBlock, TextBox tb);

        Triple<bool, string, Color> CheckPaths(string pcFolder, List<TextBox> textBoxes);
    }
}
