using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using WpfApp_Project_SyncFiles.Interfaces;

namespace WpfApp_Project_SyncFiles.HelperClasses
{
    public class AppendColoredText : IAppendColoredText
    {
        private readonly StringBuilder _textBuilder = new StringBuilder();
        private TextBlock _textBlock;
        private object _door = new object();

        public void SetTextBock(TextBlock textBlock)
        {
            _textBlock = textBlock;
        }

        public void AppendText(string newText)
        {
            if (Application.Current.Dispatcher.CheckAccess())
            {
                AddText(newText);
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    AddText(newText);
                });
            }
        }

        private void AddText(string newText)
        {
            _textBuilder.AppendLine(DateTime.Now.ToString() + " | " + newText + Environment.NewLine + Environment.NewLine); // Append the text
            _textBlock.Text = _textBuilder.ToString();
        }
    }
}
