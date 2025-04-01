using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using WpfApp_Project_SyncFiles.Interfaces;

namespace WpfApp_Project_SyncFiles.Models
{
    public class FolderNodeModel : ITreeNodeModel, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged = (sender, e) => { };
        #endregion

        #region Private Members
        private string _FolderClosed = @"Images/FolderClosed.png";
        private string _FolderOpen = @"Images/FolderOpen.png";
        private string _FolderIcon = @"Images/FolderClosed.png";
        private bool _IsSelected = false;
        private bool _IsExpanded = false;
        private string _Name = null;
        #endregion

        #region Constructors
        public FolderNodeModel(string path, string name, TreeServiceModel linkToTree)
        {
            Children = new ObservableCollection<ITreeNodeModel>
            {
                // Add in null child so the parrent can expand
                null
            };
            
            FullPath = path;
            _Name = name;
            LinkToTree = linkToTree;
        }

        public FolderNodeModel(string path, string name, FolderNodeModel parent, TreeServiceModel linkToTree)
        {
            Children = new ObservableCollection<ITreeNodeModel>
            {
                // Add in null child so the parrent can expand
                null
            };
            FullPath = path;
            _Name = name;
            Parent = parent;
            LinkToTree = linkToTree;
        }

        public FolderNodeModel(string name, FolderNodeModel parent, TreeServiceModel linkToTree)
        {
            Children = new ObservableCollection<ITreeNodeModel>
            {
                // Add in null child so the parrent can expand
                null
            };
            _Name = name;
            Parent = parent;
            LinkToTree = linkToTree;
        }
        #endregion

        #region Public Properties
        public ObservableCollection<ITreeNodeModel> Children { get; set; }

        public string FolderIcon
        {
            get
            {
                return _FolderIcon;
            }
            set
            {
                _FolderIcon = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(FolderIcon)));
            }
        }

        public bool IsSelected
        {
            get
            {
                return _IsSelected;
            }
            set
            {
                _IsSelected = value;

                if (value)
                {
                    LinkToTree.SelectedItem = this;
                }
            }
        }

        public bool IsExpanded
        {
            get
            {
                return _IsExpanded;
            }
            set
            {
                _IsExpanded = value;

                // Change the text symbol of the folder to open or close
                FolderIcon = value ? _FolderOpen : _FolderClosed;

                if (Children[0] == null)
                {
                    // Clear out the null child that allowed it to expand
                    Children.Clear();
                    // Add all the sub-folders and files below the selected item
                    DynamicFolderAddExample();
                    // If no children display to the user
                    if (Children.Count == 0)
                    {
                        Name = string.Format("{0} (0 Images)", Name);
                    }
                }
            }
        }

        public TreeServiceModel LinkToTree { get; set; }

        public string Name 
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
                PropertyChanged(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        
        }

        public FolderNodeModel Parent { get; set; }

        public string FullPath { get; set; }
        #endregion

        #region Private Methods
        private void DynamicFolderAddExample()
        {
            try
            {
                // Skips the exception of trying to get files out of a folder the user does not have access to
                var badFolder = false;

                #region Get Folders
                try
                {
                    foreach (var subFolderPath in Directory.GetDirectories(FullPath))
                    {
                        // Add child under the parent
                        Children.Add(new FolderNodeModel(subFolderPath, Path.GetFileName(subFolderPath), this, LinkToTree));
                    }
                }
                catch (Exception)
                {
                    // Log exception
                    badFolder = true;
                }
                #endregion
            }
            catch (Exception)
            {
                // Log exception
            }
        }
        #endregion
    }
}
