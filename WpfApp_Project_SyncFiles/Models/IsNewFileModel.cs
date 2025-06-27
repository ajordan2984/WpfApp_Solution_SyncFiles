
namespace WpfApp_Project_SyncFiles.Models
{
    public class IsNewFileModel
    {
        private string _fileSource;
        private string _fileDestination;
        private bool _newFile;
        
        public string FileSource
        {
            get => _fileSource;
        }

        public string FileDestination
        {
            get => _fileDestination;
        }

        public bool NewFile
        {
            get => _newFile;
        }

        public IsNewFileModel(string fileSource, string fileDestination, bool newFile)
        {
            _fileSource = fileSource;
            _fileDestination = fileDestination;
            _newFile = newFile;
        }
    }
}
