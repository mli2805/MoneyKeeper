using Caliburn.Micro;

namespace Keeper2018
{
    public class DbAskLoadingViewModel : Screen
    {
        public string Message { get; set; }

        public int Result;

        public DbAskLoadingViewModel(string message)
        {
            Message = message;
        }

        public void LoadFromOld()
        {
            Result = 1;
            TryClose();
        }
        public void LoadFromNew()
        {
            Result = 2;
            TryClose();
        }
        public void Cancel()
        {
            Result = 0;
            TryClose();
        }
    }
}
