using Caliburn.Micro;

namespace Keeper2018
{
    public class AskReceiptDeletionViewModel : Screen
    {
        public int Result { get; set; }
        protected override void OnViewLoaded(object view)
        {
            DisplayName = "Выберите";
            Result = 0;
        }

        public void WholeReceipt()
        {
            Result = 99;
            TryClose();
        }

        public void OneTransaction()
        {
            Result = 1;
            TryClose();
        }

        public void Cancel()
        {
            Result = 0;
            TryClose();
        }
    }
}
