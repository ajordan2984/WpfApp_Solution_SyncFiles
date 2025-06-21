using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Media;
using WpfApp_Project_SyncFiles.Helpers;
using WpfApp_Project_SyncFiles.Models;

namespace WpfApp_Project_SyncFiles.Controllers
{
    public class SyncFilesFromPcToExternalDriveController
    {
        private Action<string, SolidColorBrush> _updateTextBlockUI;
        private ConcurrentQueue<string> _logMessages;

        private string _pathToFilesOnPc;
        private string _pathToFilesOnExternal;

        private string _shortPathToFilesOnPc;
        private string _shortPathToFilesOnExternal;

        ConcurrentBag<string> _ConcurrentListBoxItems;

        private ConcurrentDictionary<string, FileInfoHolderModel> _allSortedFilesFromPcPath;
        private ConcurrentDictionary<string, FileInfoHolderModel> _allFilesFromFromExternalDrive;

        private CancellationToken _ct;
        private HelperFunctions _hf;
    

        public SyncFilesFromPcToExternalDriveController(CancellationToken ct)
        {
            _logMessages = new ConcurrentQueue<string>();
            _ct = ct;
            _hf = new HelperFunctions(_ct, _logMessages);
        }
        
        public void SetUpdateTextBlockOnUI(Action<string, SolidColorBrush> updateTextBlockUI)
        {
            _updateTextBlockUI = updateTextBlockUI;
        }

        public void SetConcurrentListBoxItems(ConcurrentBag<string> concurrentListBoxItems)
        {
            _ConcurrentListBoxItems = concurrentListBoxItems;
        }

        public void SetAllSortedFilesFromPcPath(ConcurrentDictionary<string, FileInfoHolderModel> files)
        {
            _allSortedFilesFromPcPath = files;
        }

        public void SetPaths(string pathToFilesOnPc, string pathToFilesOnExternal)
        {
            _pathToFilesOnPc = pathToFilesOnPc;
            _pathToFilesOnExternal = pathToFilesOnExternal;
            _shortPathToFilesOnPc = _hf.ShortenedPath(_pathToFilesOnPc);
            _shortPathToFilesOnExternal = _hf.ShortenedPath(_pathToFilesOnExternal);
        }

        public bool SyncFiles()
        {
            _hf.SetStartingDirectory(_pathToFilesOnExternal);
            _hf.SetUpdateTextBlockOnUI(_updateTextBlockUI);

            string userCanceledSyncMsg = $"{DateTime.Now} | Cancelling Syncing Files to \"{_pathToFilesOnExternal}\".";

            string CheckingForChangesMsg = $"{DateTime.Now} | Checking for the file: \"{_pathToFilesOnExternal}\\Changes.txt\".";
            _logMessages.Enqueue(CheckingForChangesMsg);
            _updateTextBlockUI(CheckingForChangesMsg, Brushes.Blue);
            //
            _allFilesFromFromExternalDrive = _hf.CheckForChanges($"{_pathToFilesOnExternal}\\Changes.txt");

            // CANCEL SYNCING FILES TO EXTERNAL FOLDER
            if (_ct.IsCancellationRequested)
            {
                _logMessages.Enqueue(userCanceledSyncMsg);
                _updateTextBlockUI(userCanceledSyncMsg, Brushes.Red);
                return false;
            }

            if (_allFilesFromFromExternalDrive.IsEmpty)
            {
                List<string> allDirectories = _hf.GetAllDirectories(_pathToFilesOnExternal, _ConcurrentListBoxItems);
                _allFilesFromFromExternalDrive = _hf.GetAllFiles(allDirectories);
            }

            // CANCEL SYNCING FILES TO EXTERNAL FOLDER
            if (_ct.IsCancellationRequested)
            {
                _logMessages.Enqueue(userCanceledSyncMsg);
                _updateTextBlockUI(userCanceledSyncMsg, Brushes.Red);
                return false;
            }

            string CopyingFilesMsg = $"{DateTime.Now} | Copying files from: \"{_pathToFilesOnPc}\" to \"{_pathToFilesOnExternal}\".";
            _logMessages.Enqueue(CopyingFilesMsg);
            _updateTextBlockUI(CopyingFilesMsg, Brushes.Blue);
            //
            long filesCopied = _hf.CopyFilesFromOneDriveToAnotherDrive(
                _allSortedFilesFromPcPath,
                _allFilesFromFromExternalDrive,
                _shortPathToFilesOnPc,
                _shortPathToFilesOnExternal);
            
            if (filesCopied > 0)
            {
                string DoneCopyingMsg = $"{DateTime.Now} | Done copying {filesCopied} files from: \"{_pathToFilesOnPc}\" to \"{_pathToFilesOnExternal}\".";
                _logMessages.Enqueue(DoneCopyingMsg);
                _updateTextBlockUI(DoneCopyingMsg, Brushes.Black);
            }
            else
            {
                string NoFileCopiedMsg = $"{DateTime.Now} | 0 files copied from: \"{_pathToFilesOnPc}\" to \"{_pathToFilesOnExternal}\".";
                _logMessages.Enqueue(NoFileCopiedMsg);
                _updateTextBlockUI(NoFileCopiedMsg, Brushes.Black);
            }
            
            // CANCEL SYNCING FILES TO EXTERNAL FOLDER
            if (_ct.IsCancellationRequested)
            {
                _logMessages.Enqueue(userCanceledSyncMsg);
                _updateTextBlockUI(userCanceledSyncMsg, Brushes.Red);
                return false;
            }

            string QuarantiningFilesMsg = $"{DateTime.Now} | Quarantining any files on: \"{_pathToFilesOnExternal}\".";
            _logMessages.Enqueue(QuarantiningFilesMsg);
            _updateTextBlockUI(QuarantiningFilesMsg, Brushes.Blue);
            //
            _hf.QuarantineFiles(
            _allSortedFilesFromPcPath,
            _allFilesFromFromExternalDrive,
            _shortPathToFilesOnPc,
            _shortPathToFilesOnExternal);
            //
            string DoneQuarantiningMsg = $"{DateTime.Now} | Done quarantining files on: \"{_pathToFilesOnExternal}\".";
            _logMessages.Enqueue(DoneQuarantiningMsg);
            _updateTextBlockUI(DoneQuarantiningMsg, Brushes.Blue);

            // CANCEL SYNCING FILES TO EXTERNAL FOLDER
            if (_ct.IsCancellationRequested)
            {
                _logMessages.Enqueue(userCanceledSyncMsg);
                _updateTextBlockUI(userCanceledSyncMsg, Brushes.Red);
                return false;
            }

            string RemovingEmptyFoldersMsg = $"{DateTime.Now} | Removing any empty folders on: \"{ _pathToFilesOnExternal}\".";
            _logMessages.Enqueue(RemovingEmptyFoldersMsg);
            _updateTextBlockUI(RemovingEmptyFoldersMsg, Brushes.Blue);
            //
            _hf.ParallelRecursiveRemoveDirectories(_pathToFilesOnExternal);
            //
            string DoneRemovingEmptyFoldersMsg = $"{DateTime.Now} | Done removing any empty folders on: \"{ _pathToFilesOnExternal}\".";
            _logMessages.Enqueue(DoneRemovingEmptyFoldersMsg);
            _updateTextBlockUI(DoneRemovingEmptyFoldersMsg, Brushes.Blue);

            // CANCEL SYNCING FILES TO EXTERNAL FOLDER
            if (_ct.IsCancellationRequested)
            {
                _logMessages.Enqueue(userCanceledSyncMsg);
                _updateTextBlockUI(userCanceledSyncMsg, Brushes.Red);
                return false;
            }

            string WritingChangesFileMsg = $"{DateTime.Now} | Writing \"Changes.txt\" on: \"{_pathToFilesOnExternal}\".";
            _logMessages.Enqueue(WritingChangesFileMsg);
            _updateTextBlockUI(WritingChangesFileMsg, Brushes.Blue);
            //
            _hf.UpdateChangesFile($"{_pathToFilesOnExternal}\\Changes.txt", _allFilesFromFromExternalDrive);
            //
            string DoneWritingChangesFileMsg = $"{DateTime.Now} | Done writing \"Changes.txt\" on: \"{_pathToFilesOnExternal}\".";
            _logMessages.Enqueue(DoneWritingChangesFileMsg);
            _updateTextBlockUI(DoneWritingChangesFileMsg, Brushes.Blue);

            return true;
        }

        public void RemoveUpdateChangesFile()
        {
            try
            {
                _hf.RemoveUpdateChangesFile($"{_pathToFilesOnExternal}\\Changes.txt");
            }
            catch (Exception ex)
            {
                _updateTextBlockUI(ex.Message, Brushes.Red);
            }
        }

        public void WriteLogFile(ConcurrentQueue<string> pcLogMessages)
        {
            try
            {
                using StreamWriter textFile = new($"{_pathToFilesOnExternal}/log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

                foreach (string line in pcLogMessages)
                {
                    textFile.WriteLine(line + Environment.NewLine);
                }

                foreach (string line in _logMessages)
                {
                    textFile.WriteLine(line + Environment.NewLine);
                }
                
                textFile.Close();
            }
            catch (Exception ex)
            {
                _updateTextBlockUI(ex.Message, Brushes.Red);
            }
        }
    }
}
