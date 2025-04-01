
namespace WpfApp_Project_SyncFiles.Interfaces
{
    public interface ITextUpdater
    {
        string Message { get; set; }
        void UpdateMessage(string newMessage);
    }
}
