using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using WpfApp_Project_SyncFiles.Models;

namespace WpfApp_Project_SyncFiles.Interfaces
{
    interface IErrorCheck
    {
        void AddOrRemoveListBoxItem(bool add, ObservableCollection<string> SkipFoldersListBoxItems, string folder, Action<string, SolidColorBrush> error);
        HasErrorModel CheckPaths(string pcFolder, Dictionary<string, string> textBoxPaths);
    }
}
