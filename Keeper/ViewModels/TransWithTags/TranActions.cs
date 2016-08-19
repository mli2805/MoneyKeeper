using System.Linq;
using Caliburn.Micro;
using Keeper.DomainModel.Trans;

namespace Keeper.ViewModels.TransWithTags
{
    public class TranActions
    {
        public static IWindowManager WindowManager => IoC.Get<IWindowManager>();

        private TransModel _model;
        public bool Do(int code, TransModel model)
        {
            _model = model;
            switch (code)
            {
                case 0: return Edit();
                case 1: return MoveUp(_model.SelectedTranIndex);
                case 2: return MoveDown(_model.SelectedTranIndex);
                case 3: return AddAfterSelected();
                case 4: Delete(); return true;
                default:
                    return false;
            }
        }

        private bool Edit()
        {
            OneTranViewModel oneTranForm = IoC.Get<OneTranViewModel>();
            oneTranForm.SetTran(_model.SelectedTranWrappedForDatagrid.Tran);
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
            oneTranForm.SetTran(tranForAdding);
            bool? result = WindowManager.ShowDialog(oneTranForm);

            if (!result.HasValue || !result.Value) return false;

            var tran = oneTranForm.GetTran().Clone();
            var tranWrappedForDatagrid = new TranWrappedForDatagrid() { Tran = tran };
            _model.Rows.Add(tranWrappedForDatagrid);
            _model.Db.TransWithTags.Add(tran);
            _model.SelectedTranWrappedForDatagrid = tranWrappedForDatagrid;
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
