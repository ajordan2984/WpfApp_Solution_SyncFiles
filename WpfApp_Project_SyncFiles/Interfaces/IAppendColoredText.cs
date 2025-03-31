using System.Drawing;
using System.Windows.Controls;

namespace WpfApp_Project_SyncFiles.Interfaces
{
    public interface IAppendColoredText
    {
        public void SetTextBock(TextBlock textBlock);

        public void AppendText(string message);
    }
}
