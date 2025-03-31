using WpfApp_Project_SyncFiles.Interfaces;

namespace WpfApp_Project_SyncFiles.Models
{
    public class FileNodeModel : ITreeNodeModel
    {
        #region Private Members
        private bool _IsSelected = false;
        #endregion

        #region Constructors
        public FileNodeModel(string path, string name, FolderNodeModel parent, TreeServiceModel linkToTree)
        {
            FullPath = path;
            Name = name;
            Parent = parent;
            LinkToTree = linkToTree;
        }

        public FileNodeModel(string name, FolderNodeModel parent, TreeServiceModel linkToTree)
        {
            Name = name;
            Parent = parent;
            LinkToTree = linkToTree;
        }
        #endregion

        #region Public Properties
        public bool IsExpanded { get; set; }

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

        public TreeServiceModel LinkToTree { get; set; }

        public string Name { get; set; }

        public FolderNodeModel Parent { get; set; }

        public string FullPath { get; set; }
        #endregion
    }
}
