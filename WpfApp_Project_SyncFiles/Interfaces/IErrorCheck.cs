using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using WpfApp_Project_SyncFiles.Models;

namespace WpfApp_Project_SyncFiles.Interfaces
{
    interface IErrorCheck
    {
        ConcurrentBag<string> CreateNewSkipFoldersBag(string ShortenedPcPath, ObservableCollection<string> SkipFoldersListBoxItems);
        void AddOrRemoveListBoxItem(bool add, ObservableCollection<string> SkipFoldersListBoxItems, string folder, Action<string, SolidColorBrush> error);
        void LoadSkipFoldersListBoxItems(ObservableCollection<string> SkipFoldersListBoxItems);
        void UpdateSkipFoldersListBoxItemsFile(ObservableCollection<string> SkipFoldersListBoxItems, string folderToRemove, bool add);
        HasErrorModel CheckPaths(string pcFolder, Dictionary<string, string> textBoxPaths);
    }
}
