
namespace WpfApp_Project_SyncFiles.Models
{
    public class HasErrorModel
    {
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; }

        public HasErrorModel()
        {
            HasError = false;
            ErrorMessage = null;
        }

        public HasErrorModel(bool hasError, string errorMessage)
        {
            HasError = hasError;
            ErrorMessage = errorMessage;
        }
    }
}
