
namespace WpfApp_Project_SyncFiles.Models
{
    public class IsNewFileModel
    {
        private string _fileSource;
        private string _fileDestination;
        private bool _newFile;
        private long _fileSize;
        
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

        public long FileSize
        {
            get => _fileSize;
        }

        public IsNewFileModel(string fileSource, string fileDestination, bool newFile, long fileSize)
        {
            _fileSource = fileSource;
            _fileDestination = fileDestination;
            _newFile = newFile;
            _fileSize = fileSize;
        }
    }
}
