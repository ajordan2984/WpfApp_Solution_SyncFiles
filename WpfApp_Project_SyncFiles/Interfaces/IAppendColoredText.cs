using System.Drawing;
using System.Windows.Controls;

namespace WpfApp_Project_SyncFiles.Interfaces
{
    public interface IAppendColoredText
    {
        void SetRichTextBox(TextBlock textBlock);

        void AppendColoredText(string message, Color color);
    }
}
