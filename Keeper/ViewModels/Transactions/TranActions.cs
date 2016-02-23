using System.Collections.ObjectModel;
using Caliburn.Micro;
using Keeper.DomainModel.Transactions;

namespace Keeper.ViewModels.Transactions
{
    public class TranActions
    {
        public static IWindowManager WindowManager => IoC.Get<IWindowManager>();

        private ObservableCollection<TranCocoon> _rows;
        private TranCocoon _selectedItem;
        public TranActions(ObservableCollection<TranCocoon> rows, TranCocoon selectedItem)
        {
            _rows = rows;
            _selectedItem = selectedItem;
        }

        public void Do(int code)
        {
            switch (code)
            {
                case 0: Edit(); break;
                case 1: MoveUp(); break;
                case 2: MoveDown(); break;
                case 3: AddAfterSelected(); break;
                case 4: Delete(); break;
                                default : break; 
            }

        }

        private void Edit()
        {
            var oneTrForm = IoC.Get<OneTransactionViewModel>();
            oneTrForm.SetTran(_selectedItem.Tran);
            WindowManager.ShowWindow(oneTrForm);

        }

        private void MoveUp()
        {
            
        }

        private void MoveDown()
        {
            
        }
        private void AddAfterSelected()
        {
            var oneTrForm = IoC.Get<OneTransactionViewModel>();
            WindowManager.ShowWindow(oneTrForm);
        }
        private void Delete()
        {

        }

    }
}
