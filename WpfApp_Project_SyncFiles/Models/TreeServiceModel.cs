using System;
using System.Collections.ObjectModel;
using System.IO;
using WpfApp_Project_SyncFiles.Interfaces;

namespace WpfApp_Project_SyncFiles.Models
{
    public class TreeServiceModel : ITreeServiceModel
    {
        #region Private Members
        private ObservableCollection<ITreeNodeModel> _folders;
        #endregion

        #region Constructors
        public TreeServiceModel() { SelectedItem = null; }
        #endregion

        public ITreeNodeModel SelectedItem { get; set; }

        public ObservableCollection<ITreeNodeModel> Drives
        {
            get
            {
                if (_folders == null)
                {
                    _folders = new ObservableCollection<ITreeNodeModel>();
                    DriveInfo[] drives = DriveInfo.GetDrives();

                    foreach (DriveInfo drive in drives)
                    {
                        if (drive.IsReady)
                        {
                            FolderNodeModel rootNode = new(drive.Name, drive.Name, this);
                            _folders.Add(rootNode);
                        }
                    }
                }

                return _folders;
            }
            set
            {
                // Not implemented
            }
        }

        public void AddFile(string name = "NewFile")
        {
            //if (SelectedItem != null && SelectedItem.GetType() == typeof(FolderNodeModel))
            //{
            //    var folder = (FolderNodeModel)SelectedItem;
            //    folder.Children.Add(new FileNodeModel(name, (FolderNodeModel)SelectedItem, folder.LinkToTree));
            //}
        }

        public void AddFolder(string name = "NewFolder")
        {
            if (SelectedItem != null && SelectedItem.GetType() == typeof(FolderNodeModel))
            {
                FolderNodeModel folder = (FolderNodeModel)SelectedItem;
                folder.Children.Add(new FolderNodeModel(name, (FolderNodeModel)SelectedItem, folder.LinkToTree));
            }
        }

        public void DeleteSelected()
        {
            if (SelectedItem != null && SelectedItem.Parent != null)
            {
                if (SelectedItem.GetType() == typeof(FileNodeModel))
                {
                    SelectedItem.Parent.Children.Remove(SelectedItem);
                }
                else if (SelectedItem.GetType() == typeof(FolderNodeModel))
                {
                    FolderNodeModel folder = (FolderNodeModel)SelectedItem;
                    if (folder.Children.Count == 0)
                    {
                        SelectedItem.Parent.Children.Remove(SelectedItem);
                    }
                }
            }
        }

        public void DeleteFolder()
        {
            throw new NotImplementedException();
        }

        public void MoveFile()
        {
            throw new NotImplementedException();
        }

        public void MoveFolder()
        {
            throw new NotImplementedException();
        }

        public void NewTree()
        {
            throw new NotImplementedException();
        }
    }
}
