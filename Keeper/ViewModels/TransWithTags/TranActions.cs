using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel.Transactions;
using Microsoft.Vbe.Interop;

namespace Keeper.ViewModels.TransWithTags
{
    public class TranActions
    {
        public static IWindowManager WindowManager => IoC.Get<IWindowManager>();

        private ObservableCollection<TranWithTags> _transWithTags;
        private ObservableCollection<TranWrappedForDatagrid> _rows;
        private TranWrappedForDatagrid _selectedItem;

        public void Do(ObservableCollection<TranWithTags> transWithTags, int code, ObservableCollection<TranWrappedForDatagrid> rows, ref TranWrappedForDatagrid selectedItem)
        {
            _transWithTags = transWithTags;
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
            selectedItem = _selectedItem;
        }

        private void Edit()
        {
            OneTranViewModel oneTranForm = IoC.Get<OneTranViewModel>();
            oneTranForm.SetTran(_selectedItem.Tran);
            bool? result = WindowManager.ShowDialog(oneTranForm);
            if (result.HasValue && result.Value)
                _selectedItem.Tran = oneTranForm.GetTran().Clone();
        }

        private void MoveUp()
        {

        }

        private void MoveDown()
        {

        }
        private void AddAfterSelected()
        {
            var oneTranForm = IoC.Get<OneTranViewModel>();
            var tranForAdding = _selectedItem.Tran.Clone();
            tranForAdding.Timestamp = tranForAdding.Timestamp.AddMinutes(1);
            oneTranForm.SetTran(tranForAdding);
            bool? result = WindowManager.ShowDialog(oneTranForm);
            if (result.HasValue && result.Value)
            {
                var tran = oneTranForm.GetTran().Clone();
                _rows.Add(new TranWrappedForDatagrid() {Tran = tran });
                _transWithTags.Add(tran);
            }
        }
        private void Delete()
        {
            int n = _rows.IndexOf(_selectedItem);
            int count = _rows.Count;

            _rows.Remove(_selectedItem);

            _selectedItem = _rows.ElementAt(n);
        }

    }
}
