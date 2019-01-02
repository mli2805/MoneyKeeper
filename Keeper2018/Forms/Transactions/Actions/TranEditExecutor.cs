using System.Linq;
using Caliburn.Micro;

namespace Keeper2018
{
    public class TranEditExecutor
    {
        private readonly TransModel _model;

        private readonly IWindowManager _windowManager;
        private readonly KeeperDb _db;
        private readonly OneTranViewModel _oneTranViewModel;

        public TranEditExecutor(TransModel model, IWindowManager windowManager, KeeperDb db, OneTranViewModel oneTranViewModel)
        {
            _model = model;

            _windowManager = windowManager;
            _db = db;
            _oneTranViewModel = oneTranViewModel;
        }

   
        public void EditSelected()
        {
            _oneTranViewModel.Init(_model.SelectedTranWrappedForDatagrid.Tran, false);
            bool? result = _windowManager.ShowDialog(_oneTranViewModel);

            if (!result.HasValue || !result.Value) return;

            _oneTranViewModel.GetTran().CopyInto(_model.SelectedTranWrappedForDatagrid.Tran);
       //     var tranInDb = _db.Bin.Transactions.First(t=>t.)

            _model.SortedRows.Refresh();
            _model.IsCollectionChanged = true;
        }

     
        public void AddAfterSelected()
        {
            var tranForAdding = PrepareTranForAdding();
            _oneTranViewModel.Init(tranForAdding, true);
            bool? result = _windowManager.ShowDialog(_oneTranViewModel);

            if (!result.HasValue || !result.Value) return;

            if (_oneTranViewModel.ReceiptList != null)
                AddOneTranAndReceipt(_oneTranViewModel);
            else
                AddOneTran(_oneTranViewModel.GetTran().Clone());

            if (_oneTranViewModel.IsOneMore) AddAfterSelected();

            _model.IsCollectionChanged = true;
        }

        private void AddOneTran(TransactionModel tran)
        {
            var transAfter = _db.TransactionModels
                .Where(t => t.Timestamp.Date == tran.Timestamp.Date && t.Timestamp >= tran.Timestamp).ToList();
            foreach (var transactionModel in transAfter)
            {
                transactionModel.Timestamp = transactionModel.Timestamp.AddMinutes(1);
                var wrappedTran = _model.Rows.First(t => t.Tran.Equals(transactionModel));
                wrappedTran.Tran.Timestamp = transactionModel.Timestamp;
            }

            var tranWrappedForDatagrid = new TranWrappedForDatagrid() { Tran = tran };
            _model.Rows.Add(tranWrappedForDatagrid);
            _model.SelectedTranWrappedForDatagrid = tranWrappedForDatagrid;
            _db.TransactionModels.Add(tran);
            _db.Bin.Transactions.Add(tran.Map());
        }

        private void AddOneTranAndReceipt(OneTranViewModel oneTranForm)
        {
            var oneTran = oneTranForm.GetTran();
            var receiptId = _db.TransactionModels.Where(t => t.Timestamp.Date == oneTran.Timestamp.Date)
                .Max(r => r.Receipt) + 1;
            foreach (var tuple in oneTranForm.ReceiptList)
            {
                var tran = oneTran.Clone();
                tran.Receipt = receiptId;
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

        public void DeleteSelected()
        {
            _db.TransactionModels.Remove(_model.SelectedTranWrappedForDatagrid.Tran);
            _db.Bin.Transactions.RemoveAll(
                t => t.Timestamp.Equals(_model.SelectedTranWrappedForDatagrid.Tran.Timestamp));

            int n = _model.Rows.IndexOf(_model.SelectedTranWrappedForDatagrid);
            _model.Rows.Remove(_model.SelectedTranWrappedForDatagrid);
            if (n == _model.Rows.Count) n--;
            _model.SelectedTranWrappedForDatagrid = _model.Rows.ElementAt(n);
        }

    }
}
