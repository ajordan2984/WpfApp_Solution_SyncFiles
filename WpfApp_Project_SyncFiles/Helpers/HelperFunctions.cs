
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
    public class HelperFunctions
    {
        private Action<string, SolidColorBrush> _updateTextBlockUI;
        CancellationToken _ct;
        private ConcurrentQueue<string> _logMessages;
        private string _startingDirectory;

        public HelperFunctions(CancellationToken ct, ConcurrentQueue<string> logMessages)
        {
            _ct = ct;
            _logMessages = logMessages;
        }

        public void SetStartingDirectory(string startingDirectory)
        {
            _startingDirectory = startingDirectory;
        }

        public void SetUpdateTextBlockOnUI(Action<string, SolidColorBrush> updateTextBlockUI)
        {
            _updateTextBlockUI = updateTextBlockUI;
        }

        public string ShortenedPath(string path)
        {
            string[] tokens = null;
            string shortenedPath = "";

            try
            {
                tokens = path.Split('\\');

                if (tokens != null)
                {
                    for (int i = 0; i < tokens.Length - 1; i++)
                    {
                        shortenedPath += tokens[i] + '\\';
                    }
                }
            }
            catch (Exception ex)
            {
                _updateTextBlockUI(ex.Message, Brushes.Red);
                _logMessages.Enqueue($"{DateTime.Now} | {ex.Message}");
            }

            return shortenedPath.TrimEnd('\\');
        }

        public ConcurrentDictionary<string, FileInfoHolderModel> CheckForChanges(string pathToChangesFile)
        {
            ConcurrentDictionary<string, FileInfoHolderModel> files = new();

            try
            {
                if (File.Exists(pathToChangesFile))
                {
                    string FileFoundMsg = $"{DateTime.Now} | File \"{pathToChangesFile}\" found. Reading the file contents.";
                    _updateTextBlockUI(FileFoundMsg, Brushes.Blue);
                    _logMessages.Enqueue(FileFoundMsg);

                    string newPathRoot = Path.GetPathRoot(pathToChangesFile);
                    string[] lines = File.ReadAllLines(pathToChangesFile);

                    for (int i = 0; i < lines.Length - 1; i += 2)
                    {
                        string oldPathRoot = Path.GetPathRoot(lines[i]);
                        string filePathOnExternal = lines[i].Replace(oldPathRoot, newPathRoot);
                        FileInfoHolderModel fih = new(filePathOnExternal, DateTime.Parse(lines[i + 1]));

                        files.TryAdd(filePathOnExternal, fih);
                    }

                    string CompletedReadingFileMsg = $"{DateTime.Now} | Completed reading the file contents from: \"{ pathToChangesFile}\".";
                    _updateTextBlockUI(CompletedReadingFileMsg, Brushes.Blue);
                    _logMessages.Enqueue(CompletedReadingFileMsg);
                }
                else
                {
                    string FileNotFoundMsg = $"{DateTime.Now} | Cannot find: \"{pathToChangesFile}\" | Moving to collect directories and files.";
                    _updateTextBlockUI(FileNotFoundMsg, Brushes.Black);
                    _logMessages.Enqueue(FileNotFoundMsg);
                }
            }
            catch (Exception ex)
            {
                _updateTextBlockUI(ex.Message, Brushes.Red);
                _logMessages.Enqueue($"{DateTime.Now} | {ex.Message}");
            }

            return files;
        }

        public List<string> GetAllDirectories(string startingDirectory, ConcurrentBag<string> ConcurrentListBoxItems)
        {
            string GettingAllFoldersMsg = $"{DateTime.Now} | Getting all folders from: {startingDirectory}.";
            _updateTextBlockUI(GettingAllFoldersMsg, Brushes.Blue);
            _logMessages.Enqueue(GettingAllFoldersMsg);

            ConcurrentBag<string> bagOfDirectories = new();
            List<string> filteredList = new();
            List<string> allDirectories = new List<string>(Directory.GetDirectories(startingDirectory))
                .Where(item => !ConcurrentListBoxItems.Any(substring => item.Contains(substring)))
                .ToList();

            try
            {
                ParallelOptions options = new() { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 };

                _ = Parallel.ForEach(allDirectories, options, directory =>
                {
                    try
                    {
                        // CANCEL SYNCING FILES TO EXTERNAL FOLDER
                        if (_ct.IsCancellationRequested)
                        {
                            string UserCanceledMsg = $"{DateTime.Now} | Cancelling getting all folders from: {startingDirectory}.";
                            _updateTextBlockUI(UserCanceledMsg, Brushes.Red);
                            _logMessages.Enqueue(UserCanceledMsg);
                            return;
                        }

                        ConcurrentGetAllDirectories(bagOfDirectories, directory);
                    }
                    catch (Exception ex)
                    {
                        _updateTextBlockUI(ex.Message, Brushes.Red);
                        _logMessages.Enqueue($"{DateTime.Now} | {ex.Message}");
                    }
                });


                filteredList = bagOfDirectories
                            .Where(item => !ConcurrentListBoxItems.Any(substring => item.Contains(substring)))
                            .ToList();

                string FoldersFoundMsg = $"{DateTime.Now} | {filteredList.Count} folders found in: {startingDirectory}.";
                _updateTextBlockUI(FoldersFoundMsg, Brushes.Blue);
                _logMessages.Enqueue(FoldersFoundMsg);
            }
            catch (Exception ex)
            {
                _updateTextBlockUI(ex.Message, Brushes.Red);
                _logMessages.Enqueue($"{DateTime.Now} | {ex.Message}");
            }

            string CompletedGettingFoldersMsg = $"{DateTime.Now} | Completed getting all folders from: {startingDirectory}.";
            _updateTextBlockUI(CompletedGettingFoldersMsg, Brushes.Blue);
            _logMessages.Enqueue(CompletedGettingFoldersMsg);
            
            return filteredList;
        }

        private void ConcurrentGetAllDirectories(ConcurrentBag<string> allDirectories, string currentDirectory)
        {
            try
            {
                // CANCEL SYNCING FILES TO EXTERNAL FOLDER
                if (_ct.IsCancellationRequested)
                {
                    string UserCanceledMsg = $"{DateTime.Now} | Cancelling getting all folders from: {_startingDirectory}.";
                    _updateTextBlockUI(UserCanceledMsg, Brushes.Red);
                    _logMessages.Enqueue(UserCanceledMsg);
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
                _logMessages.Enqueue($"{DateTime.Now} | {ex.Message}");
            }
        }

        public ConcurrentDictionary<string, FileInfoHolderModel> GetAllFiles(List<string> allDirectories)
        {
            string GettingAllFilesMsg = $"{DateTime.Now} | Getting all files from: {_startingDirectory}.";
            _updateTextBlockUI(GettingAllFilesMsg, Brushes.Blue);
            _logMessages.Enqueue(GettingAllFilesMsg);

            ConcurrentDictionary<string, FileInfoHolderModel> allFiles = new();
            ConcurrentBag<FileInfoHolderModel> bagOfAllFiles = new();

            // CANCEL SYNCING FILES TO EXTERNAL FOLDER
            if (_ct.IsCancellationRequested)
            {
                string UserCanceledMsg = $"{DateTime.Now} | Cancelling getting all files.";
                _updateTextBlockUI(UserCanceledMsg, Brushes.Red);
                _logMessages.Enqueue(UserCanceledMsg);
                return new ConcurrentDictionary<string, FileInfoHolderModel>();
            }

            ParallelOptions options = new() { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 };

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


            string FileCountMsg = $"{DateTime.Now} | {allFiles.Count} files found in: {_startingDirectory}.";
            string CompletedGettingFilesMsg = $"{DateTime.Now} | Completed getting all files from: {_startingDirectory}.";

            _updateTextBlockUI(FileCountMsg, Brushes.Blue);
            _updateTextBlockUI(CompletedGettingFilesMsg, Brushes.Blue);
            _logMessages.Enqueue(FileCountMsg);
            _logMessages.Enqueue(CompletedGettingFilesMsg);

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
                    FileInfoHolderModel fih = new(file, fi.LastWriteTime.ToUniversalTime());
                    bagOfAllFiles.Add(fih);
                }
            }
            catch (Exception ex)
            {
                _updateTextBlockUI(ex.Message, Brushes.Red);
                _logMessages.Enqueue($"{DateTime.Now} | {ex.Message}");
            }
        }

        public long CopyFilesFromOneDriveToAnotherDrive(
            ConcurrentDictionary<string, FileInfoHolderModel> filesFromPcPath,
            ConcurrentDictionary<string, FileInfoHolderModel> filesFromExternalDrive,
            string shortPathToFilesOnPc,
            string shortPathToFilesOnExternal)
        {
            long filesCopied = 0;
            try
            {
                ParallelOptions options = new() { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 };
                ConcurrentBag<IsNewFileModel> filesToCopy = new();

                _ = Parallel.ForEach(filesFromPcPath.Keys, options, file =>
                {
                    try
                    {
                        // CANCEL SYNCING FILES TO EXTERNAL FOLDER
                        if (_ct.IsCancellationRequested)
                        {
                            return;
                        }

                        string destinationPathForFile = file.Replace(shortPathToFilesOnPc, shortPathToFilesOnExternal);

                        if (!filesFromExternalDrive.ContainsKey(destinationPathForFile))
                        {
                            filesToCopy.Add(new IsNewFileModel(file, destinationPathForFile, true));

                            // Update that the file has been added so it reflects in the "Changes.txt" file
                            FileInfo fi = new(file);
                            FileInfoHolderModel fihm = new(destinationPathForFile, fi.LastWriteTimeUtc);
                            filesFromExternalDrive.TryAdd(destinationPathForFile, fihm);
                        }
                        else
                        {
                            FileInfoHolderModel pcFih = filesFromPcPath[file];
                            FileInfoHolderModel exFih = filesFromExternalDrive[destinationPathForFile];

                            if ((pcFih.Modified - exFih.Modified).TotalSeconds > 1)
                            {
                                filesToCopy.Add(new IsNewFileModel(file, destinationPathForFile, false));

                                // Update that the file has changed so it reflects in the "Changes.txt" file
                                FileInfo fi = new(file);
                                FileInfoHolderModel fihm = new(destinationPathForFile, fi.LastWriteTimeUtc);

                                if (filesFromExternalDrive.TryRemove(destinationPathForFile, out _))
                                {
                                    filesFromExternalDrive.TryAdd(destinationPathForFile, fihm);
                                }
                                else
                                {
                                    string FileCopiedMsg = $"{DateTime.Now} | File {destinationPathForFile} will be copied to external drive.";
                                    string FailedToUpdateMsg = $"{DateTime.Now} | Failed to update value of {destinationPathForFile} for the \"Changes.txt\" file.";

                                    _updateTextBlockUI(FileCopiedMsg, Brushes.Black);
                                    _updateTextBlockUI(FailedToUpdateMsg, Brushes.Red);
                                    _logMessages.Enqueue(FileCopiedMsg);
                                    _logMessages.Enqueue(FailedToUpdateMsg);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _updateTextBlockUI(ex.Message, Brushes.Red);
                        _logMessages.Enqueue($"{DateTime.Now} | {ex.Message}");
                    }
                });

                _ = Parallel.ForEach(filesToCopy, options, ftc =>
                  {
                    // CANCEL SYNCING FILES TO EXTERNAL FOLDER
                    if (_ct.IsCancellationRequested)
                      {
                          return;
                      }

                      Directory.CreateDirectory(Path.GetDirectoryName(ftc.FileDestination));

                      try
                      {
                          File.Copy(ftc.FileSource, ftc.FileDestination, true);
                          
                          if (ftc.NewFile)
                          {
                              _logMessages.Enqueue($"{DateTime.Now} | Copying New File From: {ftc.FileSource}{Environment.NewLine}To: {ftc.FileDestination}.");
                          }
                          else
                          {
                              _logMessages.Enqueue($"{DateTime.Now} | Updating Previous File From: {ftc.FileSource}{Environment.NewLine}To: {ftc.FileDestination}.");
                          }
                          Interlocked.Increment(ref filesCopied);
                      }
                      catch (Exception ex)
                      {
                          _updateTextBlockUI(ex.Message, Brushes.Red);
                          _logMessages.Enqueue($"{DateTime.Now} | {ex.Message}");
                      }
                  });
            }
            catch (Exception ex)
            {
                _updateTextBlockUI(ex.Message, Brushes.Red);
                _logMessages.Enqueue($"{DateTime.Now} | {ex.Message}");
            }

            return filesCopied;
        }

        public void QuarantineFiles(
            ConcurrentDictionary<string, FileInfoHolderModel> filesFromPcPath,
            ConcurrentDictionary<string, FileInfoHolderModel> filesFromExternalDrive,
            string _shortPathToFilesOnPc,
            string _shortPathToFilesOnExternal
            )
        {
            try
            {
                long numberOfQuarantinedFiles = 0;
                ParallelOptions options = new() { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 };

                ConcurrentBag<string> keysToRemove = new();

                _ = Parallel.ForEach(filesFromExternalDrive.Keys, options, fileFromExternalDrive =>
                {
                    // CANCEL SYNCING FILES TO EXTERNAL FOLDER
                    if (_ct.IsCancellationRequested)
                    {
                        return;
                    }

                    try
                    {
                        string filePathOnPc = fileFromExternalDrive.Replace(_shortPathToFilesOnExternal, _shortPathToFilesOnPc);

                        if (!filesFromPcPath.ContainsKey(filePathOnPc))
                        {
                            string quarantineFilePath = filePathOnPc.Replace(_shortPathToFilesOnPc, _shortPathToFilesOnExternal + "\\QuarantineFolder");

                            Directory.CreateDirectory(Path.GetDirectoryName(quarantineFilePath));
                            //
                            _logMessages.Enqueue($"{DateTime.Now} | Moving File From: {fileFromExternalDrive} {Environment.NewLine}To: {quarantineFilePath}.");
                            //
                            File.Move(fileFromExternalDrive, quarantineFilePath);
                            Interlocked.Increment(ref numberOfQuarantinedFiles);
                            keysToRemove.Add(fileFromExternalDrive);
                        }
                    }
                    catch (Exception ex)
                    {
                        _updateTextBlockUI(ex.Message, Brushes.Red);
                        _logMessages.Enqueue($"{DateTime.Now} | {ex.Message}");
                    }
                });

                _logMessages.Enqueue($"{DateTime.Now} | {numberOfQuarantinedFiles} files quarantined.");

                foreach (string key in keysToRemove)
                {
                    filesFromExternalDrive.TryRemove(key, out _);
                }
            }
            catch (Exception ex)
            {
                _updateTextBlockUI(ex.Message, Brushes.Red);
                _logMessages.Enqueue($"{DateTime.Now} | {ex.Message}");
            }
        }

        public void ParallelRecursiveRemoveDirectories(string directory, ConcurrentBag<string> ConcurrentListBoxItems)
        {
            try
            {
                ParallelOptions options = new() { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 };

                // CANCEL SYNCING FILES TO EXTERNAL FOLDER
                if (_ct.IsCancellationRequested)
                {
                    return;
                }

                List<string> allDirectories = new List<string>(Directory.GetDirectories(directory))
                .Where(item => !ConcurrentListBoxItems.Any(substring => item.Contains(substring)))
                .ToList();

                _ = Parallel.ForEach(allDirectories, options, subDirectory =>
                  {
                    // CANCEL SYNCING FILES TO EXTERNAL FOLDER
                    if (_ct.IsCancellationRequested)
                      {
                          return;
                      }

                      try
                      {
                          RecursiveRemoveDirectories(subDirectory, ConcurrentListBoxItems);
                      }
                      catch (Exception ex)
                      {
                          _updateTextBlockUI(ex.Message, Brushes.Red);
                          _logMessages.Enqueue($"{DateTime.Now} | {ex.Message}");
                      }
                  });
            }
            catch (Exception ex)
            {
                _updateTextBlockUI(ex.Message, Brushes.Red);
                _logMessages.Enqueue($"{DateTime.Now} | {ex.Message}");
            }
        }

        private void RecursiveRemoveDirectories(string directory, ConcurrentBag<string> ConcurrentListBoxItems)
        {
            try
            {
                List<string> allDirectories = new List<string>(Directory.GetDirectories(directory))
                .Where(item => !ConcurrentListBoxItems.Any(substring => item.Contains(substring)))
                .ToList();

                if (allDirectories.Count > 0)
                {
                    foreach (string subDirectory in allDirectories)
                    {
                        // CANCEL SYNCING FILES TO EXTERNAL FOLDER
                        if (_ct.IsCancellationRequested)
                        {
                            return;
                        }

                        try
                        {
                            RecursiveRemoveDirectories(subDirectory, ConcurrentListBoxItems);
                        }
                        catch (Exception ex)
                        {
                            _updateTextBlockUI(ex.Message, Brushes.Red);
                            _logMessages.Enqueue($"{DateTime.Now} | {ex.Message}");
                        }
                    }
                }

                string[] hasFiles = Directory.GetFiles(directory);
                string[] hasSubDirectories = Directory.GetDirectories(directory);

                if (hasFiles.Length == 0 && hasSubDirectories.Length == 0)
                {
                    _logMessages.Enqueue($"{DateTime.Now} | Deleteing Folder: {directory}.");
                    Directory.Delete(directory);
                }
            }
            catch (Exception ex)
            {
                _updateTextBlockUI(ex.Message, Brushes.Red);
                _logMessages.Enqueue($"{DateTime.Now} | {ex.Message}");
            }
        }

        public void UpdateChangesFile(string pathToChangesFile, ConcurrentDictionary<string, FileInfoHolderModel> allFilesFromFromExternalDrive)
        {
            try
            {
                using StreamWriter writetext = new(pathToChangesFile);
                foreach (KeyValuePair<string, FileInfoHolderModel> file in allFilesFromFromExternalDrive)
                {
                    writetext.WriteLine(file.Key);
                    writetext.WriteLine(file.Value.Modified);
                }
                writetext.Close();
            }
            catch (Exception ex)
            {
                _updateTextBlockUI(ex.Message, Brushes.Red);
                _logMessages.Enqueue($"{DateTime.Now} | {ex.Message}");
            }
        }

        public void RemoveUpdateChangesFile(string pathToChangesFile)
        {
            try
            {
                if (File.Exists(pathToChangesFile))
                {
                    File.Delete(pathToChangesFile);
                }
            }
            catch (Exception ex)
            {
                _updateTextBlockUI(ex.Message, Brushes.Red);
                _logMessages.Enqueue($"{DateTime.Now} | {ex.Message}");
            }
        }
    }
}
