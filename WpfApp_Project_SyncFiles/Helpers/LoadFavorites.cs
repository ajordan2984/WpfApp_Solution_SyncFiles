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

        public void LoadSavedListBoxItems(ObservableCollection<string> TypeOfListBoxItems, string PathToLoad)
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

        public void AddOrRemoveListBoxItem(bool add, ObservableCollection<string> FoldersListBoxItems, string folder, Action<string, SolidColorBrush> error)
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
                    FoldersListBoxItems.Remove(folder);
                    FoldersListBoxItems.Add(folder);
                    UpdateSavedFoldersListBoxItemsFile(FoldersListBoxItems, folder, true);
                }
                else
                {
                    FoldersListBoxItems.Remove(folder);
                    UpdateSavedFoldersListBoxItemsFile(FoldersListBoxItems, folder, false);
                }
            }
        }

        public void UpdateSavedFoldersListBoxItemsFile(ObservableCollection<string> SkipFoldersListBoxItems, string folderToRemove, bool add)
        {
            try
            {
                string SavingExcludedPath = $"{AppDomain.CurrentDomain.BaseDirectory}ExcludedPaths.txt";

                if (File.Exists(SavingExcludedPath))
                {
                    using StreamWriter writetext = new(SavingExcludedPath);
                    foreach (string folder in SkipFoldersListBoxItems)
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
            }
            catch { }
        }
    }
}
