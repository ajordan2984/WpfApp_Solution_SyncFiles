using System;

namespace WpfApp_Project_SyncFiles.Models
{
    public sealed class FileInfoHolderModel
    {
        public DateTime Modified { get; }
        public string FullFilePath { get; }

        public FileInfoHolderModel(string fullFilePath, DateTime fileModified)
        {
            Modified = fileModified;
            FullFilePath = fullFilePath;
        }
    }
}
