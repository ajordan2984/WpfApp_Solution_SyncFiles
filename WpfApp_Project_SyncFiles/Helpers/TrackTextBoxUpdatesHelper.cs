using System.Collections.Generic;

namespace WpfApp_Project_SyncFiles.Helpers
{
    public class TrackTextBoxUpdatesHelper
    {
        private Dictionary<string, string> _ExternalFoldersSelected;
        
        public Dictionary<string, string> ExternalFoldersSelected
        {
            get
            {
                return _ExternalFoldersSelected;
            }
            private set { }
        }

        public TrackTextBoxUpdatesHelper()
        {
            _ExternalFoldersSelected = new();
        }

        public void AddSelectedExternalFolder(string TextBoxName, string TextBoxPath)
        {
            if (string.IsNullOrEmpty(TextBoxPath))
            {
                if (_ExternalFoldersSelected.ContainsKey(TextBoxName))
                {
                    _ExternalFoldersSelected.Remove(TextBoxName);
                }
            }
            else
            {
                if (_ExternalFoldersSelected.ContainsKey(TextBoxName))
                {
                    _ExternalFoldersSelected[TextBoxName] = TextBoxPath.Trim();
                }
                else
                {
                    _ExternalFoldersSelected.Add(TextBoxName, TextBoxPath.Trim());
                }
            }
        }
    }
}
