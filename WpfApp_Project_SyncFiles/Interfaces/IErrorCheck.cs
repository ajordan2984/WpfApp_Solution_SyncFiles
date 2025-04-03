using System;
using System.Collections.Generic;
using WpfApp_Project_SyncFiles.Models;

namespace WpfApp_Project_SyncFiles.Interfaces
{
    interface IErrorCheck
    {
        HasErrorModel CheckPaths(string pcFolder, Dictionary<string, string> textBoxPaths);
    }
}
