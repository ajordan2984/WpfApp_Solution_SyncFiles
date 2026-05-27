using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;
using WpfApp_Project_SyncFiles.Interfaces;

namespace WpfApp_Project_SyncFiles.Helpers
{
    public class LoadFavorites : ILoadFavorites
    {
        public LoadFavorites()
        {
        }

        public ConcurrentBag<string> CreateNewSkipFoldersBag(string ShortenedPcPath, ObservableCollection<string> SkipFoldersListBoxItems)
        {
            ConcurrentBag<string> bag = new();

            foreach (string folder in SkipFoldersListBoxItems)
            {
                if (folder.Contains(ShortenedPcPath))
                {
                    bag.Add(folder.Replace(ShortenedPcPath, ""));
                }
            }

            return bag;
        }

        public void LoadUserSelectedListBoxItems(ObservableCollection<string> TypeOfListBoxItems, string PathToLoad)
        {
            try
            {
                if (File.Exists(PathToLoad))
                {
                    string[] lines = File.ReadAllLines(PathToLoad);

                    foreach (string line in lines)
                    {
                        if (!string.IsNullOrEmpty(line) && Directory.Exists(line))
                        {
                            TypeOfListBoxItems.Add(line);
                        }
                    }
                }
            }
            catch { }
        }

        public void AddOrRemoveListBoxItem(bool add, ObservableCollection<string> UserSelectedListBoxItems, string folder, string pathToFile, Action<string, SolidColorBrush> error)
        {
            if (!Directory.Exists(folder))
            {
                error?.Invoke($"Error: The folder \"{folder}\" does not exist. Please try again.", Brushes.Red);
                return;
            }

            if (!string.IsNullOrEmpty(folder))
            {
                if (add)
                {
                    UserSelectedListBoxItems.Remove(folder);
                    UserSelectedListBoxItems.Add(folder);
                    UpdateSavedFoldersListBoxItemsFile(UserSelectedListBoxItems, folder, true, pathToFile);
                }
                else
                {
                    UserSelectedListBoxItems.Remove(folder);
                    UpdateSavedFoldersListBoxItemsFile(UserSelectedListBoxItems, folder, false, pathToFile);
                }
            }
        }

        private void UpdateSavedFoldersListBoxItemsFile(ObservableCollection<string> UserSelectedListBoxItems, string folderToRemove, bool add, string pathToFile)
        {
            try
            {
                using StreamWriter writetext = new(pathToFile);
                foreach (string folder in UserSelectedListBoxItems)
                {
                    if (add)
                    {
                        writetext.WriteLine(folder);
                    }
                    else
                    {
                        if (folder != folderToRemove)
                        {
                            writetext.WriteLine(folder);
                        }
                        else
                        {
                            writetext.WriteLine(string.Empty);
                        }
                    }
                }
                writetext.Close();
            }
            catch { }
        }
    }
}
