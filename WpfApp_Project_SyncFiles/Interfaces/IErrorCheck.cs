using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using WpfApp_Project_SyncFiles.Models;

namespace WpfApp_Project_SyncFiles.Interfaces
{
    interface IErrorCheck
    {
        void AddOrRemoveListBoxItem(bool add, ObservableCollection<string> ListBoxItems, string folder);
        HasErrorModel CheckPaths(string pcFolder, Dictionary<string, string> textBoxPaths);
    }
}
