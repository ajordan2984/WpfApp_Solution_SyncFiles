using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using WpfApp_Project_SyncFiles.Models;

namespace WpfApp_Project_SyncFiles.Helpers
{
    public class GetAllFilesAndFoldersHelper
    {
        private Action<string, SolidColorBrush> _updateTextBlockUI;
        private CancellationToken _ct;
        private string _startingDirectory;

        public GetAllFilesAndFoldersHelper(Action<string, SolidColorBrush> updateTextBlockUI, CancellationToken token)
        {
            _updateTextBlockUI = updateTextBlockUI;
            _ct = token;
        }

        public ConcurrentDictionary<string, FileInfoHolderModel> CheckForChanges(string pathToChangesFile)
        {
            ConcurrentDictionary<string, FileInfoHolderModel> files = new();

            try
            {
                if (File.Exists(pathToChangesFile))
                {
                    _updateTextBlockUI($"File \"{pathToChangesFile}\" found. Reading the file contents.", Brushes.Blue);

                    string newPathRoot = Path.GetPathRoot(pathToChangesFile);
                    string[] lines = File.ReadAllLines(pathToChangesFile);

                    for (int i = 0; i < lines.Length - 1; i += 2)
                    {
                        FileInfoHolderModel fih = new("", DateTime.Parse(lines[i + 1]).ToUniversalTime());
                        string oldPathRoot = Path.GetPathRoot(lines[i]);
                        files.TryAdd(lines[i].Replace(oldPathRoot, newPathRoot), fih);
                    }

                    _updateTextBlockUI($"Completed reading the file contents from: \"{ pathToChangesFile}\"", Brushes.Blue);
                }
                else
                {
                    _updateTextBlockUI($"Cannot find: \"{pathToChangesFile}\" | Moving to collect directories and files.", Brushes.Black);
                }
            }
            catch (Exception ex)
            {
                _updateTextBlockUI(ex.Message, Brushes.Red);
            }

            return files;
        }

        public List<string> GetAllDirectories(string startingDirectory, ConcurrentBag<string> ConcurrentListBoxItems)
        {
            _startingDirectory = startingDirectory;

            _updateTextBlockUI($@"Getting all folders from: {_startingDirectory}", Brushes.Blue);

            ConcurrentBag<string> bagOfDirectories = new();
            List<string> filteredList = new();
            List<string> allDirectories = new List<string>(Directory.GetDirectories(startingDirectory))
                .Where(item => !ConcurrentListBoxItems.Any(substring => item.Contains(substring)))
                .ToList();

            try
            {
                ParallelOptions options = new() { MaxDegreeOfParallelism = Environment.ProcessorCount };

                _ = Parallel.ForEach(allDirectories, options, directory =>
                {
                    try
                    {
                        // CANCEL SYNCING FILES TO EXTERNAL FOLDER
                        if (_ct.IsCancellationRequested)
                        {
                            _updateTextBlockUI($@"Cancelling getting all folders from: {_startingDirectory}.", Brushes.Red);
                            return;
                        }

                        ConcurrentGetAllDirectories(bagOfDirectories, directory);
                    }
                    catch (Exception ex)
                    {
                        _updateTextBlockUI(ex.Message, Brushes.Red);
                    }
                });


                filteredList = bagOfDirectories
                            .Where(item => !ConcurrentListBoxItems.Any(substring => item.Contains(substring)))
                            .ToList();

                _updateTextBlockUI($@"{filteredList.Count} folders found in: {_startingDirectory}", Brushes.Blue);
            }
            catch (Exception ex)
            {
                _updateTextBlockUI(ex.Message, Brushes.Red);
            }

            _updateTextBlockUI($@"Completed getting all folders from: {_startingDirectory}.", Brushes.Blue);
            return filteredList;
        }

        private void ConcurrentGetAllDirectories(ConcurrentBag<string> allDirectories, string currentDirectory)
        {
            try
            {
                // CANCEL SYNCING FILES TO EXTERNAL FOLDER
                if (_ct.IsCancellationRequested)
                {
                    _updateTextBlockUI($@"Cancelling getting all folders from: {_startingDirectory}.", Brushes.Red);
                    return;
                }

                List<string> subDirectories = new(Directory.GetDirectories(currentDirectory));

                if (subDirectories.Count > 0)
                {
                    foreach (string subDirectory in subDirectories)
                    {
                        ConcurrentGetAllDirectories(allDirectories, subDirectory);
                    }
                }
                
                 allDirectories.Add(currentDirectory);

            }
            catch (Exception ex)
            {
                _updateTextBlockUI(ex.Message, Brushes.Red);
            }
        }

        public ConcurrentDictionary<string, FileInfoHolderModel> GetAllFiles(List<string> allDirectories)
        {
            _updateTextBlockUI($@"Getting all files from: {_startingDirectory}", Brushes.Blue);

            ConcurrentDictionary<string, FileInfoHolderModel> allFiles = new();
            ConcurrentBag<FileInfoHolderModel> bagOfAllFiles = new();

            // CANCEL SYNCING FILES TO EXTERNAL FOLDER
            if (_ct.IsCancellationRequested)
            {
                _updateTextBlockUI($@"Cancelling getting all files.", Brushes.Red);
                return new ConcurrentDictionary<string, FileInfoHolderModel>();
            }

            ParallelOptions options = new() { MaxDegreeOfParallelism = Environment.ProcessorCount };

            _ = Parallel.ForEach(allDirectories, options, directory =>
              {
                  ConcurrentGetFiles(directory, bagOfAllFiles);
              });

            foreach (FileInfoHolderModel fih in bagOfAllFiles)
            {
                if (!allFiles.ContainsKey(fih.FullFilePath))
                {
                    allFiles.TryAdd(fih.FullFilePath, fih);
                }
            }

            _updateTextBlockUI($@"{allFiles.Count} files found in: {_startingDirectory}.", Brushes.Blue);
            _updateTextBlockUI($@"Completed getting all files from: {_startingDirectory}.", Brushes.Blue);
            return allFiles;
        }

        private void ConcurrentGetFiles(string directory, ConcurrentBag<FileInfoHolderModel> bagOfAllFiles)
        {
            List<string> files = new();

            try
            {
                // CANCEL SYNCING FILES TO EXTERNAL FOLDER
                if (_ct.IsCancellationRequested)
                {
                    return;
                }

                files.AddRange(Directory.GetFiles(directory, "*"));

                foreach (string file in files)
                {
                    FileInfo fi = new(file);
                    FileInfoHolderModel fih = new FileInfoHolderModel(file, fi.LastWriteTimeUtc);
                    bagOfAllFiles.Add(fih);
                }
            }
            catch (Exception ex)
            {
                _updateTextBlockUI(ex.Message, Brushes.Red);
            }
        }
    }
}
