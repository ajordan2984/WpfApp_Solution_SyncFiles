
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
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

        public HelperFunctions(Action<string, SolidColorBrush> updateTextBlockUI, CancellationToken ct)
        {
            _updateTextBlockUI = updateTextBlockUI;
            _ct = ct;
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
            }

            return shortenedPath.TrimEnd('\\');
        }

        public int CopyFilesFromOneDriveToAnotherDrive(
            ConcurrentDictionary<string, FileInfoHolderModel> filesFromPcPath,
            ConcurrentDictionary<string, FileInfoHolderModel> filesFromExternalDrive,
            string shortPathToFilesOnPc,
            string shortPathToFilesOnExternal)
        {
            int filesCopied = 0;
            try
            {
                ParallelOptions options = new() { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 };
                ConcurrentBag<Tuple<string, string>> filesToCopy = new();

                _ = Parallel.ForEach(filesFromPcPath.Keys, options, file =>
                {
                    try
                    {
                        // CANCEL SYNCING FILES TO EXTERNAL FOLDER
                        if (_ct.IsCancellationRequested)
                        {
                            _updateTextBlockUI($"Cancelling Syncing Files in \"CopyFilesFromOneDriveToAnotherDrive\".", Brushes.Red);
                            return;
                        }

                        string destinationPathForFile = file.Replace(shortPathToFilesOnPc, shortPathToFilesOnExternal);

                        if (!filesFromExternalDrive.ContainsKey(destinationPathForFile))
                        {
                            filesToCopy.Add(new Tuple<string, string>(file, destinationPathForFile));

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
                                filesToCopy.Add(new Tuple<string, string>(file, destinationPathForFile));

                                // Update that the file has changed so it reflects in the "Changes.txt" file
                                FileInfo fi = new(file);
                                FileInfoHolderModel fihm = new(destinationPathForFile, fi.LastWriteTimeUtc);

                                if (filesFromExternalDrive.TryRemove(destinationPathForFile, out _))
                                {
                                    filesFromExternalDrive.TryAdd(destinationPathForFile, fihm);
                                }
                                else
                                {
                                    _updateTextBlockUI($"File {destinationPathForFile} will be copied to external drive.", Brushes.Black);
                                    _updateTextBlockUI($"Failed to update value of {destinationPathForFile} for the \"Changes.txt\" file.", Brushes.Red);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _updateTextBlockUI(ex.Message, Brushes.Red);
                    }
                });

                _ = Parallel.ForEach(filesToCopy, options, ftc =>
                  {
                    // CANCEL SYNCING FILES TO EXTERNAL FOLDER
                    if (_ct.IsCancellationRequested)
                      {
                          return;
                      }

                      Directory.CreateDirectory(Path.GetDirectoryName(ftc.Item2));

                      try
                      {
                          File.Copy(ftc.Item1, ftc.Item2, true);
                          Interlocked.Increment(ref filesCopied);
                      }
                      catch (Exception ex)
                      {
                          _updateTextBlockUI(ex.Message, Brushes.Red);
                      }
                  });
            }
            catch (Exception ex)
            {
                _updateTextBlockUI(ex.Message, Brushes.Red);
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
                List<string> keysToRemove = new();

                foreach (string fileFromExternalDrive in filesFromExternalDrive.Keys)
                {
                    // CANCEL SYNCING FILES TO EXTERNAL FOLDER
                    if (_ct.IsCancellationRequested)
                    {
                        return;
                    }

                    string filePathOnPc = fileFromExternalDrive.Replace(_shortPathToFilesOnExternal, _shortPathToFilesOnPc);

                    if (!filesFromPcPath.ContainsKey(filePathOnPc))
                    {
                        string quarantineFilePath = filePathOnPc.Replace(_shortPathToFilesOnPc, _shortPathToFilesOnExternal + "\\QuarantineFolder");

                        Directory.CreateDirectory(Path.GetDirectoryName(quarantineFilePath));
                        File.Move(fileFromExternalDrive, quarantineFilePath);
                        keysToRemove.Add(fileFromExternalDrive);
                    }
                }

                foreach (string key in keysToRemove)
                {
                    filesFromExternalDrive.TryRemove(key, out _);
                }
            }
            catch (Exception ex)
            {
                _updateTextBlockUI(ex.Message, Brushes.Red);
            }
        }

        public void ParallelRecursiveRemoveDirectories(string directory)
        {
            try
            {
                ParallelOptions options = new() { MaxDegreeOfParallelism = Environment.ProcessorCount * 2 };

                // CANCEL SYNCING FILES TO EXTERNAL FOLDER
                if (_ct.IsCancellationRequested)
                {
                    return;
                }

                List<string> allDirectories = new(Directory.GetDirectories(directory));

                _ = Parallel.ForEach(allDirectories, options, subDirectory =>
                  {
                    // CANCEL SYNCING FILES TO EXTERNAL FOLDER
                    if (_ct.IsCancellationRequested)
                      {
                          return;
                      }

                      try
                      {
                          RecursiveRemoveDirectories(subDirectory);
                      }
                      catch (Exception ex)
                      {
                          _updateTextBlockUI(ex.Message, Brushes.Red);
                      }
                  });
            }
            catch (Exception ex)
            {
                _updateTextBlockUI(ex.Message, Brushes.Red);
            }
        }

        private void RecursiveRemoveDirectories(string directory)
        {
            try
            {
                List<string> allDirectories = new(Directory.GetDirectories(directory));

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
                            RecursiveRemoveDirectories(subDirectory);
                        }
                        catch (Exception ex)
                        {
                            _updateTextBlockUI(ex.Message, Brushes.Red);
                        }
                    }
                }

                string[] hasFiles = Directory.GetFiles(directory);
                string[] hasSubDirectories = Directory.GetDirectories(directory);

                if (hasFiles.Length == 0 && hasSubDirectories.Length == 0)
                {
                    Directory.Delete(directory);
                }
            }
            catch (Exception ex)
            {
                _updateTextBlockUI(ex.Message, Brushes.Red);
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
            }
        }
    }
}
