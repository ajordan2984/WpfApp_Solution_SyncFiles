using System;

namespace WpfApp_Project_SyncFiles.Models
{
    public class FileInfoHolderModel
    {
        private DateTime _Modified;
        private string _FullFilePath;


        public DateTime Modified
        {
            get
            {
                return _Modified;
            }
            set
            {
                _Modified = value;
            }
        }

        public string FullFilePath
        {
            get
            {
                return _FullFilePath;
            }
            set
            {
                _FullFilePath = value;
            }
        }

        public FileInfoHolderModel(string fullFilePath, DateTime fileModified)
        {
            _Modified = fileModified;
            _FullFilePath = fullFilePath;
        }
    }
}
