using System;
using System.Collections.Generic;
using System.Linq;

namespace Keeper2018
{
    public class TranMoveExecutor
    {
        public enum Destination { Up, Down }

        private readonly TransModel _model;
        private readonly KeeperDb _db;

        public TranMoveExecutor(TransModel model, KeeperDb db)
        {
            _model = model;
            _db = db;
        }

        private List<TransactionModel> _transToElevate;
        private List<TransactionModel> _transToLower;
        private List<TransactionModel> _transToShiftTime;

        private bool FillLists(Destination destination)
        {
            var selected = _model.SelectedTranWrappedForDatagrid.Tran;
            if (selected.Receipt != 0)
            {
                _transToElevate = _model.Rows.Where(t =>
                    t.Tran.Timestamp.Date.Equals(selected.Timestamp.Date) && t.Tran.Receipt == selected.Receipt).Select(r => r.Tran).ToList();

                var edge = destination == Destination.Up ? _transToElevate.Min(t => t.Timestamp) : _transToElevate.Max(t => t.Timestamp);
                if (!selected.Timestamp.Equals(edge))
                    _transToElevate = new List<TransactionModel>{selected};
            }
            else _transToElevate = new List<TransactionModel> {selected};

            var nearbyTran = _model.Rows.LastOrDefault(t => t.Tran.Timestamp < selected.Timestamp);
            if (nearbyTran == null) return false;

            if (nearbyTran.Tran.Receipt != 0)
                _transToLower = _model.Rows.Where(t => t.Tran.Timestamp.Date.Equals(nearbyTran.Tran.Timestamp.Date)
                                                       && t.Tran.Receipt == nearbyTran.Tran.Receipt).Select(r => r.Tran).ToList();
            else _transToLower = new List<TransactionModel> {nearbyTran.Tran};

            if (destination == Destination.Down && selected.Timestamp.Date != nearbyTran.Tran.Timestamp.Date)
            {
                var maxOfElevate = _transToElevate.Max(t => t.Timestamp);
                _transToShiftTime = _model.Rows.Where(t => t.Tran.Timestamp.Date.Equals(maxOfElevate.Date) 
                                             && t.Tran.Timestamp.Date > maxOfElevate).Select(r => r.Tran).ToList();
            }
            else _transToShiftTime = null;

            return true;
        }

        public void MoveSelected(Destination destination)
        {
            if (!FillLists(destination)) return;

            if (_transToElevate.First().Timestamp.Date.Equals(_transToLower.First().Timestamp.Date))
            {
                var newTimestamp = _transToElevate.Min(t => t.Timestamp);
                SetNewTimes(newTimestamp);
            }
            else
            {
                var newTimestamp = _transToLower.Min(t => t.Timestamp);
               SetNewTimes(newTimestamp);
            }
           
            _model.SortedRows.Refresh();
            _model.IsCollectionChanged = true;
        }

        private void SetNewTimes(DateTime newTimestamp)
        {
            foreach (var transactionModel in _transToElevate)
            {
                transactionModel.Timestamp = newTimestamp;
                newTimestamp = newTimestamp.AddMinutes(1);
            }

            foreach (var transactionModel in _transToLower)
            {
                transactionModel.Timestamp = newTimestamp;
                newTimestamp = newTimestamp.AddMinutes(1);
            }

            foreach (var transactionModel in _transToShiftTime)
            {
                transactionModel.Timestamp = newTimestamp;
                newTimestamp = newTimestamp.AddMinutes(1);
            }
        }

        public void MoveSelectedDown()
        {
            var selectedTransactionModel = _model.SelectedTranWrappedForDatagrid.Tran;
            var nearbyTran = _model.Rows.FirstOrDefault(t => t.Tran.Timestamp > selectedTransactionModel.Timestamp && t.Tran.Receipt == 0);
            if (nearbyTran == null) return;
            var temp = nearbyTran.Tran.Timestamp;

            var tran = _db.Bin.Transactions.First(t => t.Timestamp.Equals(temp));
            var selectedTran = _db.Bin.Transactions.First(t => t.Timestamp.Equals(selectedTransactionModel.Timestamp));

            nearbyTran.Tran.Timestamp = selectedTransactionModel.Timestamp;
            tran.Timestamp = selectedTransactionModel.Timestamp;

            selectedTransactionModel.Timestamp = temp;
            selectedTran.Timestamp = temp;
            _model.SortedRows.Refresh();
            _model.IsCollectionChanged = true;
        }

    }
}