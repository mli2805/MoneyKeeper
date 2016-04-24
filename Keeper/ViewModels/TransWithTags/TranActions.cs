using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel.Transactions;

namespace Keeper.ViewModels.TransWithTags
{
    public class TranActions
    {
        public static IWindowManager WindowManager => IoC.Get<IWindowManager>();

        private TransModel _data;
        public void Do(int code, TransModel data)
        {
            _data = data;
            switch (code)
            {
                case 0: Edit(); break;
                case 1: MoveUp(_data.SelectedTranIndex); break;
                case 2: MoveDown(_data.SelectedTranIndex); break;
                case 3: AddAfterSelected(); break;
                case 4: Delete(); break;
                default: break;
            }
        }

        private void Edit()
        {
            OneTranViewModel oneTranForm = IoC.Get<OneTranViewModel>();
            oneTranForm.SetTran(_data.SelectedTranWrappedForDatagrid.Tran);
            bool? result = WindowManager.ShowDialog(oneTranForm);
            if (result.HasValue && result.Value)
                _data.SelectedTranWrappedForDatagrid.Tran = oneTranForm.GetTran().Clone();
        }

        private void MoveUp(int selectedTranIndex)
        {
            var previousTran = _data.Rows[selectedTranIndex-1];
            if (previousTran.Tran.Timestamp.Date == _data.SelectedTranWrappedForDatagrid.Tran.Timestamp.Date)
            {// exchange timestamps
                var temp = previousTran.Tran.Timestamp;
                previousTran.Tran.Timestamp = _data.SelectedTranWrappedForDatagrid.Tran.Timestamp;
                _data.SelectedTranWrappedForDatagrid.Tran.Timestamp = temp;
            }
            else
            {// insert in previous day
                var temp = previousTran.Tran.Timestamp;
                previousTran.Tran.Timestamp = previousTran.Tran.Timestamp.AddMinutes(1);
                _data.SelectedTranWrappedForDatagrid.Tran.Timestamp = temp;
            }
            _data.SortedRows.Refresh();
        }

        private void MoveDown(int selectedTranIndex)
        {

        }
        private void AddAfterSelected()
        {
            var oneTranForm = IoC.Get<OneTranViewModel>();
            var tranForAdding = PrepareTranForAdding();
            oneTranForm.SetTran(tranForAdding);
            bool? result = WindowManager.ShowDialog(oneTranForm);
            if (result.HasValue && result.Value)
            {
                var tran = oneTranForm.GetTran().Clone();
                _data.Rows.Add(new TranWrappedForDatagrid() {Tran = tran });
                _data.Db.TransWithTags.Add(tran);
            }
        }

        private TranWithTags PrepareTranForAdding()
        {
            var tranForAdding = _data.SelectedTranWrappedForDatagrid.Tran.Clone();
            tranForAdding.Timestamp = tranForAdding.Timestamp.AddMinutes(1);
            tranForAdding.Comment = "";
            return tranForAdding;
        }

        private void Delete()
        {
            int n = _data.Rows.IndexOf(_data.SelectedTranWrappedForDatagrid);
            _data.Rows.Remove(_data.SelectedTranWrappedForDatagrid);
            _data.SelectedTranWrappedForDatagrid = _data.Rows.ElementAt(n);
        }

    }
}
