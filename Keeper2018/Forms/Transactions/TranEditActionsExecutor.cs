using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public class TranEditActionsExecutor
    {
        private readonly IWindowManager _windowManager;
        private readonly KeeperDb _db;
        private readonly OneTranViewModel _oneTranViewModel;

        public TranEditActionsExecutor(IWindowManager windowManager, KeeperDb db, OneTranViewModel oneTranViewModel)
        {
            _windowManager = windowManager;
            _db = db;
            _oneTranViewModel = oneTranViewModel;
        }

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
            _oneTranViewModel.Init(_model.SelectedTranWrappedForDatagrid.Tran, false);
            bool? result = _windowManager.ShowDialog(_oneTranViewModel);

            if (!result.HasValue || !result.Value) return false;

            _oneTranViewModel.GetTran().CopyInto(_model.SelectedTranWrappedForDatagrid.Tran);
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
            var tranForAdding = PrepareTranForAdding();
            _oneTranViewModel.Init(tranForAdding, true);
            bool? result = _windowManager.ShowDialog(_oneTranViewModel);

            if (!result.HasValue || !result.Value) return false;

            if (_oneTranViewModel.ReceiptList != null)
                AddOneTranAndReceipt(_oneTranViewModel);
            else
                AddOneTran(_oneTranViewModel.GetTran().Clone());

            if (_oneTranViewModel.IsOneMore) AddAfterSelected();

            return true;
        }

        private void AddOneTran(TransactionModel tran)
        {
            var tranWrappedForDatagrid = new TranWrappedForDatagrid() { Tran = tran };
            _model.Rows.Add(tranWrappedForDatagrid);
            _model.SelectedTranWrappedForDatagrid = tranWrappedForDatagrid;
            _db.TransactionModels.Add(tran);
            _db.Bin.Transactions.Add(tran.Map());
        }

        private void AddOneTranAndReceipt(OneTranViewModel oneTranForm)
        {
            foreach (var tuple in oneTranForm.ReceiptList)
            {
                var tran = oneTranForm.GetTran().Clone();
                tran.Amount = tuple.Item1;
                tran.Tags.Add(tuple.Item2);
                tran.Comment = tuple.Item3;
                AddOneTran(tran);
            }
        }
        private TransactionModel PrepareTranForAdding()
        {
            var tranForAdding = _model.SelectedTranWrappedForDatagrid.Tran.Clone();
            tranForAdding.Timestamp = tranForAdding.Timestamp.AddMinutes(1);
            tranForAdding.Amount = 0;
            tranForAdding.AmountInReturn = 0;
            tranForAdding.Comment = "";
            return tranForAdding;
        }

        private void Delete()
        {
            _db.TransactionModels.Remove(_model.SelectedTranWrappedForDatagrid.Tran);

            int n = _model.Rows.IndexOf(_model.SelectedTranWrappedForDatagrid);
            _model.Rows.Remove(_model.SelectedTranWrappedForDatagrid);
            if (n == _model.Rows.Count) n--;
            _model.SelectedTranWrappedForDatagrid = _model.Rows.ElementAt(n);
        }

    }
}
