using System.Collections.ObjectModel;

namespace WpfApp_Project_SyncFiles.Interfaces
{
    public interface ITreeServiceModel
    {
        ObservableCollection<ITreeNodeModel> Drives { get; set; }
        ITreeNodeModel SelectedItem { get; }
        void AddFolder(string name = "NewFolder");
        void MoveFolder();
        void MoveFile();
        void NewTree();
        void DeleteSelected();
    }
}
