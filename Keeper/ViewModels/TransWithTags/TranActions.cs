using System.Collections.ObjectModel;
using Caliburn.Micro;
using Keeper.DomainModel.Transactions;

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
            OneTranViewModel oneTranForm = IoC.Get<OneTranViewModel>();
            oneTranForm.SetTran(_selectedItem.Tran);
            bool? result = WindowManager.ShowDialog(oneTranForm);
            if (result.HasValue && result.Value) _selectedItem.Tran = oneTranForm.GetTran().Clone();
        }

        private void MoveUp()
        {

        }

        private void MoveDown()
        {

        }
        private void AddAfterSelected()
        {
            var oneTrForm = IoC.Get<OneTranViewModel>();
            WindowManager.ShowWindow(oneTrForm);
        }
        private void Delete()
        {

        }

    }
}
