using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Media;
using WpfApp_Project_SyncFiles.Helpers;
using WpfApp_Project_SyncFiles.Models;

namespace WpfApp_Project_SyncFiles.Controllers
{
    public class SyncFilesFromPcToExternalDriveController
    {
        private Action<string, SolidColorBrush> _updateTextBlockUI;

        private string _pathToFilesOnPc;
        private string _pathToFilesOnExternal;

        private string _shortPathToFilesOnPc;
        private string _shortPathToFilesOnExternal;

        ConcurrentBag<string> _ConcurrentListBoxItems;

        private ConcurrentDictionary<string, FileInfoHolderModel> _allSortedFilesFromPcPath;
        private ConcurrentDictionary<string, FileInfoHolderModel> _allFilesFromFromExternalDrive;

        private CancellationToken _ct;
        private HelperFunctions _hf;
    

        public SyncFilesFromPcToExternalDriveController(Action<string, SolidColorBrush> updateTextBlockUI, CancellationToken ct)
        {
            _updateTextBlockUI = updateTextBlockUI;
            _ct = ct;
            _hf = new HelperFunctions(updateTextBlockUI, _ct);
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
            GetAllFilesAndFoldersHelper gafh = new(_updateTextBlockUI, _ct);

            _updateTextBlockUI($"Checking for the file: \"{_pathToFilesOnExternal}\\Changes.txt\"", Brushes.Blue);
            _allFilesFromFromExternalDrive = gafh.CheckForChanges($"{_pathToFilesOnExternal}\\Changes.txt");

            // CANCEL SYNCING FILES TO EXTERNAL FOLDER
            if (_ct.IsCancellationRequested)
            {
                _updateTextBlockUI($"Cancelling Syncing Files to \"{_pathToFilesOnExternal}\".", Brushes.Red);
                return false;
            }

            if (_allFilesFromFromExternalDrive.IsEmpty)
            {
                _allFilesFromFromExternalDrive = gafh.GetAllFiles(gafh.GetAllDirectories(_pathToFilesOnExternal, _ConcurrentListBoxItems));
            }

            // CANCEL SYNCING FILES TO EXTERNAL FOLDER
            if (_ct.IsCancellationRequested)
            {
                _updateTextBlockUI($"Cancelling Syncing Files to \"{_pathToFilesOnExternal}\".", Brushes.Red);
                return false;
            }

            _updateTextBlockUI($"Copying files from: \"{_pathToFilesOnPc}\" to \"{_pathToFilesOnExternal}\"", Brushes.Blue);
            int filesCopied = _hf.CopyFilesFromOneDriveToAnotherDrive(
                _allSortedFilesFromPcPath,
                _allFilesFromFromExternalDrive,
                _shortPathToFilesOnPc,
                _shortPathToFilesOnExternal);
            
            if (filesCopied > 0)
            {
                _updateTextBlockUI($"Done copying {filesCopied} files from: \"{_pathToFilesOnPc}\" to \"{_pathToFilesOnExternal}\".", Brushes.Black);
            }
            else
            {
                _updateTextBlockUI($"No files copied from: \"{_pathToFilesOnPc}\" to \"{_pathToFilesOnExternal}\".", Brushes.Black);
            }
            
            // CANCEL SYNCING FILES TO EXTERNAL FOLDER
            if (_ct.IsCancellationRequested)
            {
                _updateTextBlockUI($"Cancelling Syncing Files to \"{_pathToFilesOnExternal}\".", Brushes.Red);
                return false;
            }

            _updateTextBlockUI($"Quarantining any files on: \"{_pathToFilesOnExternal}\"", Brushes.Blue);
            _hf.QuarantineFiles(
            _allSortedFilesFromPcPath,
            _allFilesFromFromExternalDrive,
            _shortPathToFilesOnPc,
            _shortPathToFilesOnExternal);
            _updateTextBlockUI($"Done quarantining files on: \"{_pathToFilesOnExternal}\"", Brushes.Blue);

            // CANCEL SYNCING FILES TO EXTERNAL FOLDER
            if (_ct.IsCancellationRequested)
            {
                _updateTextBlockUI($"Cancelling Syncing Files to \"{_pathToFilesOnExternal}\".", Brushes.Red);
                return false;
            }

            _updateTextBlockUI($"Removing any empty folders on: \"{ _pathToFilesOnExternal}\"", Brushes.Blue);
            _hf.ParallelRecursiveRemoveDirectories(_pathToFilesOnExternal);
            _updateTextBlockUI($"Done removing any empty folders on: \"{ _pathToFilesOnExternal}\"", Brushes.Blue);

            // CANCEL SYNCING FILES TO EXTERNAL FOLDER
            if (_ct.IsCancellationRequested)
            {
                _updateTextBlockUI($"Cancelling Syncing Files to \"{_pathToFilesOnExternal}\".", Brushes.Red);
                return false;
            }

            _updateTextBlockUI($"Writing \"Changes.txt\" on: \"{_pathToFilesOnExternal}\"", Brushes.Blue);
            _hf.UpdateChangesFile($"{_pathToFilesOnExternal}\\Changes.txt", _allFilesFromFromExternalDrive);
            _updateTextBlockUI($"Done writing \"Changes.txt\" on: \"{_pathToFilesOnExternal}\"", Brushes.Blue);

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
    }
}
