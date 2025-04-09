using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using WpfApp_Project_SyncFiles.Helpers;
using WpfApp_Project_SyncFiles.Models;

namespace WpfApp_Project_SyncFiles.Controllers
{
    public class SyncFilesFromPcToExternalDriveController
    {
        private Action<string> _updateTextBlockUI;

        private string _pathToFilesOnPc;
        private string _pathToFilesOnExternal;

        private string _shortPathToFilesOnPc;
        private string _shortPathToFilesOnExternal;

        private ConcurrentDictionary<string, FileInfoHolderModel> _allSortedFilesFromPcPath;
        private SortedDictionary<string, FileInfoHolderModel> _allSortedFilesFromFromExternalDrive;

        private CancellationToken _ct;
        private HelperFunctions _hf;
    

        public SyncFilesFromPcToExternalDriveController(Action<string> updateTextBlockUI, CancellationToken ct)
        {
            _updateTextBlockUI = updateTextBlockUI;
            _ct = ct;
            _hf = new HelperFunctions(updateTextBlockUI, _ct);
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
            GetAllFilesAndFoldersHelper gafh = new(_updateTextBlockUI);

            _updateTextBlockUI($"Checking for the file: \"{_pathToFilesOnExternal}\\Changes.txt\"");
            _allSortedFilesFromFromExternalDrive = gafh.CheckForChanges($"{_pathToFilesOnExternal}\\Changes.txt");

            // CANCEL SYNCING FILES TO EXTERNAL FOLDER
            if (_ct.IsCancellationRequested)
            {
                _updateTextBlockUI($"Cancelling Syncing Files to \"{_pathToFilesOnExternal}\".");
                return false;
            }

            if (_allSortedFilesFromFromExternalDrive.Count == 0)
            {
                _allSortedFilesFromFromExternalDrive = gafh.GetAllFiles(gafh.GetAllDirectories(_pathToFilesOnExternal, _ct), _ct);
            }

            // CANCEL SYNCING FILES TO EXTERNAL FOLDER
            if (_ct.IsCancellationRequested)
            {
                _updateTextBlockUI($"Cancelling Syncing Files to \"{_pathToFilesOnExternal}\".");
                return false;
            }

            _updateTextBlockUI($"Copying files from: \"{_pathToFilesOnPc}\" to \"{_pathToFilesOnExternal}\"");
            _hf.CopyFilesFromOneDriveToAnotherDrive(
                _allSortedFilesFromPcPath,
                _allSortedFilesFromFromExternalDrive,
                _shortPathToFilesOnPc,
                _shortPathToFilesOnExternal);
            _updateTextBlockUI($"Done copying files from: \"{_pathToFilesOnPc}\" to \"{_pathToFilesOnExternal}\"");
            
            // CANCEL SYNCING FILES TO EXTERNAL FOLDER
            if (_ct.IsCancellationRequested)
            {
                _updateTextBlockUI($"Cancelling Syncing Files to \"{_pathToFilesOnExternal}\".");
                return false;
            }

            _updateTextBlockUI($"Quarantining any files on: \"{_pathToFilesOnExternal}\"");
            _hf.QuarantineFiles(
            _allSortedFilesFromPcPath,
            _allSortedFilesFromFromExternalDrive,
            _shortPathToFilesOnPc,
            _shortPathToFilesOnExternal);
            _updateTextBlockUI($"Done quarantining files on: \"{_pathToFilesOnExternal}\"");

            // CANCEL SYNCING FILES TO EXTERNAL FOLDER
            if (_ct.IsCancellationRequested)
            {
                _updateTextBlockUI($"Cancelling Syncing Files to \"{_pathToFilesOnExternal}\".");
                return false;
            }

            _updateTextBlockUI($"Removing any empty folders on: \"{ _pathToFilesOnExternal}\"");
            _hf.RecursiveRemoveDirectories(_pathToFilesOnExternal);
            _updateTextBlockUI($"Done removing any empty folders on: \"{ _pathToFilesOnExternal}\"");

            // CANCEL SYNCING FILES TO EXTERNAL FOLDER
            if (_ct.IsCancellationRequested)
            {
                _updateTextBlockUI($"Cancelling Syncing Files to \"{_pathToFilesOnExternal}\".");
                return false;
            }

            _updateTextBlockUI($"Writing \"Changes.txt\" on: \"{_pathToFilesOnExternal}\"");
            _hf.UpdateChangesFile($@"{_pathToFilesOnExternal}\Changes.txt", _allSortedFilesFromFromExternalDrive);
            _updateTextBlockUI($"Done writing \"Changes.txt\" on: \"{_pathToFilesOnExternal}\"");

            return true;
        }
    }
}
