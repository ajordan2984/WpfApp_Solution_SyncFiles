
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

        public void CopyFilesFromOneDriveToAnotherDrive(
            ConcurrentDictionary<string, FileInfoHolderModel> filesFromPcPath,
            SortedDictionary<string, FileInfoHolderModel> filesFromExternalDrive,
            string _shortPathToFilesOnPc,
            string _shortPathToFilesOnExternal)
        {
            try
            {
                List<Tuple<string, string>> filesToCopy = new();

                foreach (string file in filesFromPcPath.Keys)
                {
                    // CANCEL SYNCING FILES TO EXTERNAL FOLDER
                    if (_ct.IsCancellationRequested)
                    {
                        _updateTextBlockUI($"Cancelling Syncing Files in \"CopyFilesFromOneDriveToAnotherDrive\".", Brushes.Red);
                        return;
                    }

                    string destinationPathForFile = file.Replace(_shortPathToFilesOnPc, _shortPathToFilesOnExternal);

                    if (!filesFromExternalDrive.ContainsKey(destinationPathForFile))
                    {
                        filesToCopy.Add(new Tuple<string, string>(file, destinationPathForFile));
                    }
                    else
                    {
                        var pcFih = filesFromPcPath[file];
                        var exFih = filesFromExternalDrive[destinationPathForFile];

                        if (pcFih.Modified > exFih.Modified)
                        {
                            filesToCopy.Add(new Tuple<string, string>(file, destinationPathForFile));
                        }
                    }
                }

                ParallelOptions options = new() { MaxDegreeOfParallelism = Environment.ProcessorCount };

                Parallel.ForEach(filesToCopy, options, ftc =>
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

        public void QuarantineFiles(
            ConcurrentDictionary<string, FileInfoHolderModel> filesFromPcPath,
            SortedDictionary<string, FileInfoHolderModel> filesFromExternalDrive,
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
                    filesFromExternalDrive.Remove(key);
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
                ParallelOptions options = new() { MaxDegreeOfParallelism = Environment.ProcessorCount };

                // CANCEL SYNCING FILES TO EXTERNAL FOLDER
                if (_ct.IsCancellationRequested)
                {
                    return;
                }

                List<string> allDirectories = new(Directory.GetDirectories(directory));

                Parallel.ForEach(allDirectories, options, subDirectory =>
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

        public void RecursiveRemoveDirectories(string subDirectory)
        {
            try
            {
                List<string> allDirectories = new(Directory.GetDirectories(subDirectory));

                if (allDirectories.Count > 0)
                {
                    foreach (string directory in allDirectories)
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

                string[] hasFiles = Directory.GetFiles(subDirectory);
                string[] hasSubDirectories = Directory.GetDirectories(subDirectory);

                if (hasFiles.Length == 0 && hasSubDirectories.Length == 0)
                {
                    Directory.Delete(subDirectory);
                }
            }
            catch (Exception ex)
            {
                _updateTextBlockUI(ex.Message, Brushes.Red);
            }

        }

        public void UpdateChangesFile(string pathToChangesFile, SortedDictionary<string, FileInfoHolderModel> allSortedFilesFromFromExternalDrive)
        {
            try
            {
                using StreamWriter writetext = new(pathToChangesFile);
                foreach (var file in allSortedFilesFromFromExternalDrive)
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
