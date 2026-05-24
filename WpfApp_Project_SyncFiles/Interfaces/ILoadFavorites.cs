using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace WpfApp_Project_SyncFiles.Interfaces
{
    internal interface ILoadFavorites
    {
        ConcurrentBag<string> CreateNewSkipFoldersBag(string ShortenedPcPath, ObservableCollection<string> SkipFoldersListBoxItems);
        void LoadSavedListBoxItems(ObservableCollection<string> TypeOfListBoxItems, string PathToLoad);
        void AddOrRemoveListBoxItem(bool add, ObservableCollection<string> SkipFoldersListBoxItems, string folder, Action<string, SolidColorBrush> error);
        void UpdateSavedFoldersListBoxItemsFile(ObservableCollection<string> SkipFoldersListBoxItems, string folderToRemove, bool add);
    }
}
