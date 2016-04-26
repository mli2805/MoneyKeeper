using System.Collections.ObjectModel;
using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel.Transactions;

namespace Keeper.ViewModels.TransWithTags
{
    public class TranActions
    {
        public static IWindowManager WindowManager => IoC.Get<IWindowManager>();

        private TransModel _model;
        public void Do(int code, TransModel model)
        {
            _model = model;
            switch (code)
            {
                case 0: Edit(); break;
                case 1: MoveUp(_model.SelectedTranIndex); break;
                case 2: MoveDown(_model.SelectedTranIndex); break;
                case 3: AddAfterSelected(); break;
                case 4: Delete(); break;
                default: break;
            }
        }

        private void Edit()
        {
            OneTranViewModel oneTranForm = IoC.Get<OneTranViewModel>();
            oneTranForm.SetTran(_model.SelectedTranWrappedForDatagrid.Tran);
            bool? result = WindowManager.ShowDialog(oneTranForm);
            if (result.HasValue && result.Value)
                _model.SelectedTranWrappedForDatagrid.Tran = oneTranForm.GetTran().Clone();
        }

        private void MoveUp(int selectedTranIndex)
        {
            if (selectedTranIndex < 1) return;
            MoveTran(selectedTranIndex, -1);
        }

        private void MoveDown(int selectedTranIndex)
        {
            if (selectedTranIndex >= _model.Rows.Count - 1) return;
            MoveTran(selectedTranIndex, 1);
        }

        // destination  -1 - up  ;  1 - down
        private void MoveTran(int selectedTranIndex, int destination)
        {
            var nearbyTran = _model.Rows[selectedTranIndex + destination];
            if (nearbyTran.Tran.Timestamp.Date == _model.SelectedTranWrappedForDatagrid.Tran.Timestamp.Date)
            {// exchange timestamps
                var temp = nearbyTran.Tran.Timestamp;
                nearbyTran.Tran.Timestamp = _model.SelectedTranWrappedForDatagrid.Tran.Timestamp;
                _model.SelectedTranWrappedForDatagrid.Tran.Timestamp = temp;
            }
            else
            {// insert into another day
                _model.SelectedTranWrappedForDatagrid.Tran.Timestamp = nearbyTran.Tran.Timestamp;
                nearbyTran.Tran.Timestamp = nearbyTran.Tran.Timestamp.AddMinutes(-destination);
            }
            _model.SortedRows.Refresh();
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
                _model.Rows.Add(new TranWrappedForDatagrid() { Tran = tran });
                _model.Db.TransWithTags.Add(tran);
            }
        }

        private TranWithTags PrepareTranForAdding()
        {
            var tranForAdding = _model.SelectedTranWrappedForDatagrid.Tran.Clone();
            tranForAdding.Timestamp = tranForAdding.Timestamp.AddMinutes(1);
            tranForAdding.Comment = "";
            return tranForAdding;
        }

        private void Delete()
        {
            int n = _model.Rows.IndexOf(_model.SelectedTranWrappedForDatagrid);
            _model.Rows.Remove(_model.SelectedTranWrappedForDatagrid);
            _model.SelectedTranWrappedForDatagrid = _model.Rows.ElementAt(n);
        }

    }
}
