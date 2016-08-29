using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel.Trans;

namespace Keeper.ViewModels.TransWithTags
{
    public class TranEditActionsExecutor
    {
        public static IWindowManager WindowManager => IoC.Get<IWindowManager>();

        private TransModel _model;
        public bool Do(TranAction action, TransModel model)
        {
            _model = model;
            switch (action)
            {
                case TranAction.Edit: return Edit();
                case TranAction.MoveUp: return MoveUp(_model.SelectedTranIndex);
                case TranAction.MoveDown: return MoveDown(_model.SelectedTranIndex);
                case TranAction.AddAfterSelected: return AddAfterSelected();
                case TranAction.Delete: Delete(); return true;
                default:
                    return false;
            }
        }

        private bool Edit()
        {
            OneTranViewModel oneTranForm = IoC.Get<OneTranViewModel>();
            oneTranForm.Init(_model.SelectedTranWrappedForDatagrid.Tran, "Изменить");
            bool? result = WindowManager.ShowDialog(oneTranForm);

            if (!result.HasValue || !result.Value) return false;

            oneTranForm.GetTran().CopyInto(_model.SelectedTranWrappedForDatagrid.Tran);
            _model.SortedRows.Refresh();
            return true;
        }

        private bool MoveUp(int selectedTranIndex)
        {
            if (selectedTranIndex < 1) return false;
            MoveTran(selectedTranIndex, -1);
            return true;
        }

        private bool MoveDown(int selectedTranIndex)
        {
            if (selectedTranIndex >= _model.Rows.Count - 1) return false;
            MoveTran(selectedTranIndex, 1);
            return true;
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

        private bool AddAfterSelected()
        {
            var oneTranForm = IoC.Get<OneTranViewModel>();
            var tranForAdding = PrepareTranForAdding();
            oneTranForm.Init(tranForAdding, "Добавить");
            bool? result = WindowManager.ShowDialog(oneTranForm);

            if (!result.HasValue || !result.Value) return false;

            var tran = oneTranForm.GetTran().Clone();
            var tranWrappedForDatagrid = new TranWrappedForDatagrid() { Tran = tran };
            _model.Rows.Add(tranWrappedForDatagrid);
            _model.Db.TransWithTags.Add(tran);
            _model.SelectedTranWrappedForDatagrid = tranWrappedForDatagrid;

            if (oneTranForm.IsOneMore) AddAfterSelected();

            return true;
        }

        private TranWithTags PrepareTranForAdding()
        {
            var tranForAdding = _model.SelectedTranWrappedForDatagrid.Tran.Clone();
            tranForAdding.Timestamp = tranForAdding.Timestamp.AddMinutes(1);
            tranForAdding.Amount = 0;
            tranForAdding.Comment = "";
            return tranForAdding;
        }

        private void Delete()
        {
            _model.Db.TransWithTags.Remove(_model.SelectedTranWrappedForDatagrid.Tran);

            int n = _model.Rows.IndexOf(_model.SelectedTranWrappedForDatagrid);
            _model.Rows.Remove(_model.SelectedTranWrappedForDatagrid);
            if (n == _model.Rows.Count) n--;
            _model.SelectedTranWrappedForDatagrid = _model.Rows.ElementAt(n);
        }

    }
}
