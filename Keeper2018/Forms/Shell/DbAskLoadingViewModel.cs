using Caliburn.Micro;

namespace Keeper2018
{
    public class DbAskLoadingViewModel : Screen
    {
        public string Message { get; set; }

        public bool Result;

        public DbAskLoadingViewModel(string message)
        {
            Message = message;
        }

      
        public void LoadFromNew()
        {
            Result = true;
            TryClose();
        }
        public void Cancel()
        {
            Result = false;
            TryClose();
        }
    }
}
