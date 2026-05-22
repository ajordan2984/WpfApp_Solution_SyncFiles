using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media;
using WpfApp_Project_SyncFiles.Interfaces;
using WpfApp_Project_SyncFiles.Models;

namespace WpfApp_Project_SyncFiles.Helpers
{
    public class ErrorCheckHelper : IErrorCheck
    {
        public ErrorCheckHelper()
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

        public void AddOrRemoveListBoxItem(bool add, ObservableCollection<string> SkipFoldersListBoxItems, string folder, Action<string, SolidColorBrush> error)
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
                    SkipFoldersListBoxItems.Remove(folder);
                    SkipFoldersListBoxItems.Add(folder);
                    UpdateSkipFoldersListBoxItemsFile(SkipFoldersListBoxItems, folder, true);
                }
                else
                {
                    SkipFoldersListBoxItems.Remove(folder);
                    UpdateSkipFoldersListBoxItemsFile(SkipFoldersListBoxItems, folder, false);
                }

                
            }
        }

        public void LoadSkipFoldersListBoxItems(ObservableCollection<string> SkipFoldersListBoxItems)
        {
            try
            {
                string SavingExcludedPath = $"{AppDomain.CurrentDomain.BaseDirectory}ExcludedPaths.txt";

                if (File.Exists(SavingExcludedPath))
                {
                    string[] lines = File.ReadAllLines(SavingExcludedPath);

                    foreach (string line in lines)
                    {
                        if (!string.IsNullOrEmpty(line) && Directory.Exists(line))
                        {
                            SkipFoldersListBoxItems.Add(line);
                        }
                    }
                }
            }
            catch { }
        }

        public void UpdateSkipFoldersListBoxItemsFile(ObservableCollection<string> SkipFoldersListBoxItems, string folderToRemove, bool add)
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

        HasErrorModel IErrorCheck.CheckPaths(string pcFolderPath, Dictionary<string, string> textBoxes)
        {
            if (string.IsNullOrEmpty(pcFolderPath))
            {
                return new HasErrorModel(true, "Error: The PC folder cannot be empty. Please Try again.");
            }

            if (!Directory.Exists(pcFolderPath))
            {
                return new HasErrorModel(true, $"Error: Sorry the folder on your PC: \"{pcFolderPath}\" does not exist. Please try again.");
            }

            if (textBoxes.Count == 0)
            {
                return new HasErrorModel(true, $"Error: You must have one external folder path selected. Please try again.");
            }

            foreach (string textBoxName in textBoxes.Keys)
            {
                string externalFolderPath = textBoxes[textBoxName];

                if (externalFolderPath == pcFolderPath)
                {
                    return new HasErrorModel(true, "Error: The PC folder and External folder cannot be the same. Please Try again.");
                }

                if (externalFolderPath[0] == pcFolderPath[0])
                {
                    return new HasErrorModel(true, $"Error: The PC folder \"{pcFolderPath}\" and External folder \"{externalFolderPath}\" start with the same drive letter. Please Try again.");
                }

                if (!Directory.Exists(externalFolderPath))
                {
                    return new HasErrorModel(true, $"Error: Sorry the folder on your external drive: \"{externalFolderPath}\" does not exist. Please try again.");
                }

                string shortPcFolderPath = Path.GetFileName(pcFolderPath);
                string shortExternalFolderPath = Path.GetFileName(externalFolderPath);

                if (shortPcFolderPath != shortExternalFolderPath)
                {
                    return new HasErrorModel(true, $"Error: Sorry the end of your PC folder: \"{pcFolderPath}\" does not match the end of the folder \"{externalFolderPath}\" on your external drive. Please try again.");
                }
            }

            return new HasErrorModel(false, null);
        }
    }
}
