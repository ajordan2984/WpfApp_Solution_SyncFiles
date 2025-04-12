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

        public SortedDictionary<string, FileInfoHolderModel> CheckForChanges(string pathToChangesFile)
        {
            SortedDictionary<string, FileInfoHolderModel> sortedFiles = new SortedDictionary<string, FileInfoHolderModel>();

            try
            {
                if (File.Exists(pathToChangesFile))
                {
                    _updateTextBlockUI($"File \"{pathToChangesFile}\" found. Reading the file contents.", Brushes.Blue);

                    string newPathRoot = Path.GetPathRoot(pathToChangesFile);
                    string[] lines = File.ReadAllLines(pathToChangesFile);

                    for (int i = 0; i < lines.Length - 1; i += 2)
                    {
                        var fih = new FileInfoHolderModel("", DateTime.Parse(lines[i + 1]).ToUniversalTime());
                        string oldPathRoot = Path.GetPathRoot(lines[i]);
                        sortedFiles.Add(lines[i].Replace(oldPathRoot, newPathRoot), fih);
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

            return sortedFiles;
        }

        public List<string> GetAllDirectories(string startingDirectory)
        {
            _startingDirectory = startingDirectory;

            _updateTextBlockUI($@"Getting all folders from: {_startingDirectory}", Brushes.Blue);

            List<string> allDirectories =
                Directory.GetDirectories(startingDirectory)
                .Where(dir => !dir.Contains("GitHub"))
                .ToList();

            try
            {
                for (int i = 0; i < allDirectories.Count; i++)
                {
                    try
                    {
                        // CANCEL SYNCING FILES TO EXTERNAL FOLDER
                        if (_ct.IsCancellationRequested)
                        {
                            _updateTextBlockUI($@"Cancelling getting all folders from: {_startingDirectory}.", Brushes.Red);
                            return new List<string>();
                        }

                        allDirectories.AddRange(Directory.GetDirectories(allDirectories[i]));
                    }
                    catch (Exception ex)
                    {
                        _updateTextBlockUI(ex.Message, Brushes.Red);
                    }
                }
            }
            catch (Exception ex)
            {
                _updateTextBlockUI(ex.Message, Brushes.Red);
            }

            _updateTextBlockUI($@"Completed getting all folders from: {_startingDirectory}.", Brushes.Blue);
            return allDirectories;
        }

        public SortedDictionary<string, FileInfoHolderModel> GetAllFiles(List<string> allDirectories)
        {
            _updateTextBlockUI($@"Getting all files from: {_startingDirectory}", Brushes.Blue);

            SortedDictionary<string, FileInfoHolderModel> allSortedFiles = new();
            ConcurrentBag<FileInfoHolderModel> bagOfAllFiles = new();

            // CANCEL SYNCING FILES TO EXTERNAL FOLDER
            if (_ct.IsCancellationRequested)
            {
                _updateTextBlockUI($@"Cancelling getting all files.", Brushes.Red);
                return new SortedDictionary<string, FileInfoHolderModel>();
            }

            ParallelOptions options = new() { MaxDegreeOfParallelism = Environment.ProcessorCount };

            _ = Parallel.ForEach(allDirectories, options, directory =>
              {
                  ConcurrentGetFiles(directory, bagOfAllFiles);
              });

            foreach (var fih in bagOfAllFiles)
            {
                if (!allSortedFiles.ContainsKey(fih.FullFilePath))
                {
                    allSortedFiles.Add(fih.FullFilePath, fih);
                }
            }

            _updateTextBlockUI($@"Completed getting all files from: {_startingDirectory}.", Brushes.Blue);
            return allSortedFiles;
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
                    FileInfo fi = new FileInfo(file);
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
