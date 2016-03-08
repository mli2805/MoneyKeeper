using System.Collections.ObjectModel;
using Caliburn.Micro;
using Keeper.DomainModel;
using Keeper.DomainModel.Enumes;
using Keeper.DomainModel.Transactions;
using Keeper.Views.Transactions;

namespace Keeper.ViewModels.TransWithTags
{
    public class TranActions
    {
        public static IWindowManager WindowManager => IoC.Get<IWindowManager>();

        private ObservableCollection<TranCocoon> _rows;
        private TranCocoon _selectedItem;

        public void Do(int code, ObservableCollection<TranCocoon> rows, TranCocoon selectedItem)
        {
            _rows = rows;
            _selectedItem = selectedItem;
            switch (code)
            {
                case 0: Edit(); break;
                case 1: MoveUp(); break;
                case 2: MoveDown(); break;
                case 3: AddAfterSelected(); break;
                case 4: Delete(); break;
                default: break;
            }
        }

        private void Edit()
        {
            IOneTranView oneTranForm;
            switch (_selectedItem.Tran.Operation)
            {
                case OperationType.Доход:
                    oneTranForm = IoC.Get<IncomeTranViewModel>();
                    break;
                case OperationType.Расход:
                    oneTranForm = IoC.Get<ExpenseTranViewModel>();
                    break;
                case OperationType.Перенос: oneTranForm = IoC.Get<TransferTranViewModel>();
                    break;
                default:
                    oneTranForm = IoC.Get<IncomeTranViewModel>();
                    break;
            }
            oneTranForm.SetTran(_selectedItem.Tran);
            WindowManager.ShowDialog(oneTranForm);
        }

        private void MoveUp()
        {

        }

        private void MoveDown()
        {

        }
        private void AddAfterSelected()
        {
            var oneTrForm = IoC.Get<ExpenseTranViewModel>();
            WindowManager.ShowWindow(oneTrForm);
        }
        private void Delete()
        {

        }

    }
}
