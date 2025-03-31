using WpfApp_Project_SyncFiles.Models;

namespace WpfApp_Project_SyncFiles.Interfaces
{
    public interface ITreeNodeModel
    {
        string FullPath { get; set; }
        string Name { get; }
        bool IsSelected { get; set; }
        bool IsExpanded { get; set; }
        FolderNodeModel Parent { get; set; }
    }
}
