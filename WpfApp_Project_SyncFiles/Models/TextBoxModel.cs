
namespace WpfApp_Project_SyncFiles.Models
{
    public class TextBoxModel
    {
        public string TextBoxName { get; set; }
        public string TextBoxText { get; set; }

        public TextBoxModel()
        {
            TextBoxName = null;
            TextBoxText = null;
        }

        public TextBoxModel(string textBoxName, string textBoxText)
        {
            TextBoxName = textBoxName;
            TextBoxText = textBoxText;
        }
    }
}
