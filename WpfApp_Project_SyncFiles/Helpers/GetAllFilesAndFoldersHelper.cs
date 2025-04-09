using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using WpfApp_Project_SyncFiles.Models;

namespace WpfApp_Project_SyncFiles.Helpers
{
    public class GetAllFilesAndFoldersHelper
    {
        private Action<string> _updateTextBlockUI;
        private string _startingDirectory;

        public GetAllFilesAndFoldersHelper(Action<string> updateTextBlockUI)
        {
            _updateTextBlockUI = updateTextBlockUI;
        }

        public SortedDictionary<string, FileInfoHolderModel> CheckForChanges(string pathToChangesFile)
        {
            SortedDictionary<string, FileInfoHolderModel> sortedFiles = new SortedDictionary<string, FileInfoHolderModel>();

            try
            {
                if (File.Exists(pathToChangesFile))
                {
                    _updateTextBlockUI($"File \"{pathToChangesFile}\" found. Reading the file contents.");

                    string newPathRoot = Path.GetPathRoot(pathToChangesFile);
                    string[] lines = File.ReadAllLines(pathToChangesFile);

                    for (int i = 0; i < lines.Length - 1; i += 2)
                    {
                        var fih = new FileInfoHolderModel("", DateTime.Parse(lines[i + 1]).ToUniversalTime());
                        string oldPathRoot = Path.GetPathRoot(lines[i]);
                        sortedFiles.Add(lines[i].Replace(oldPathRoot, newPathRoot), fih);
                    }

                    _updateTextBlockUI($"Completed reading the file contents from: \"{ pathToChangesFile}\"");
                }
                else
                {
                    _updateTextBlockUI($"Cannot find: \"{pathToChangesFile}\" | Moving to collect directories and files.");
                }
            }
            catch (Exception ex)
            {
                _updateTextBlockUI(ex.Message);
            }

            return sortedFiles;
        }

        public List<string> GetAllDirectories(string startingDirectory, CancellationToken token)
        {
            _startingDirectory = startingDirectory;

            _updateTextBlockUI($@"Getting all folders from: {_startingDirectory}");

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
                        if (token.IsCancellationRequested)
                        {
                            _updateTextBlockUI($@"Cancelling getting all folders from: {_startingDirectory}.");
                            return new List<string>();
                        }

                        allDirectories.AddRange(Directory.GetDirectories(allDirectories[i]));
                    }
                    catch (Exception ex)
                    {
                        _updateTextBlockUI(ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                _updateTextBlockUI(ex.Message);
            }

            _updateTextBlockUI($@"Completed getting all folders from: {_startingDirectory}.");
            return allDirectories;
        }

        public SortedDictionary<string, FileInfoHolderModel> GetAllFiles(List<string> allDirectories, CancellationToken token)
        {
            _updateTextBlockUI($@"Getting all files from: {_startingDirectory}");

            SortedDictionary<string, FileInfoHolderModel> allSortedFiles = new();
            ConcurrentBag<FileInfoHolderModel> bagOfAllFiles = new();

            // CANCEL SYNCING FILES TO EXTERNAL FOLDER
            if (token.IsCancellationRequested)
            {
                _updateTextBlockUI($@"Cancelling getting all files.");
                return new SortedDictionary<string, FileInfoHolderModel>();
            }

            ParallelOptions options = new() { MaxDegreeOfParallelism = Environment.ProcessorCount };

            _ = Parallel.ForEach(allDirectories, options, directory =>
              {
                  ConcurrentGetFiles(directory, bagOfAllFiles, token);
              });

            foreach (var fih in bagOfAllFiles)
            {
                if (!allSortedFiles.ContainsKey(fih.FullFilePath))
                {
                    allSortedFiles.Add(fih.FullFilePath, fih);
                }
            }

            _updateTextBlockUI($@"Completed getting all files from: {_startingDirectory}.");
            return allSortedFiles;
        }

        private void ConcurrentGetFiles(string directory, ConcurrentBag<FileInfoHolderModel> bagOfAllFiles, CancellationToken token)
        {
            List<string> files = new();

            try
            {
                if (token.IsCancellationRequested)
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
                _updateTextBlockUI(ex.Message);
            }
        }
    }
}
