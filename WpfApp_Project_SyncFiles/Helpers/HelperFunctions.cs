
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WpfApp_Project_SyncFiles.Models;

namespace WpfApp_Project_SyncFiles.Helpers
{
    public class HelperFunctions
    {
        private Action<string> _updateTextBlockUI;

        public HelperFunctions(Action<string> updateTextBlockUI)
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
                _updateTextBlockUI(ex.Message);
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
                List<Tuple<string, string>> filesToCopy = new List<Tuple<string, string>>();

                foreach (string file in filesFromPcPath.Keys)
                {
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

                ParallelOptions options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };

                Parallel.ForEach(filesToCopy, options, ftc =>
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(ftc.Item2));

                    try
                    {
                        File.Copy(ftc.Item1, ftc.Item2, true);
                    }
                    catch (Exception ex)
                    {
                        _updateTextBlockUI(ex.Message);
                    }
                });
            }
            catch (Exception ex)
            {
                _updateTextBlockUI(ex.Message);
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
                List<string> keysToRemove = new List<string>();

                foreach (string fileFromExternalDrive in filesFromExternalDrive.Keys)
                {
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
                _updateTextBlockUI(ex.Message);
            }
        }

        public void RecursiveRemoveDirectories(string directory)
        {
            try
            {
                List<string> allDirectories = new List<string>(Directory.GetDirectories(directory));

                if (allDirectories.Count > 0)
                {
                    foreach (string subDirectory in allDirectories)
                    {
                        RecursiveRemoveDirectories(subDirectory);
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
                _updateTextBlockUI(ex.Message);
            }
        }

        public void UpdateChangesFile(string pathToChangesFile, SortedDictionary<string, FileInfoHolderModel> allSortedFilesFromFromExternalDrive)
        {
            try
            {
                using (StreamWriter writetext = new StreamWriter(pathToChangesFile))
                {
                    foreach (var file in allSortedFilesFromFromExternalDrive)
                    {
                        writetext.WriteLine(file.Key);
                        writetext.WriteLine(file.Value.Modified);
                    }
                    writetext.Close();
                }
            }
            catch (Exception ex)
            {
                _updateTextBlockUI(ex.Message);
            }
        }
    }
}
